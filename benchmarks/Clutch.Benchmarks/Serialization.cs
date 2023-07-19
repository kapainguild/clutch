using System.IO;
using BenchmarkDotNet.Attributes;
using Newtonsoft.Json;

namespace Clutch.Benchmarks
{
    public class Serialization
    {
        private ObjectMaterializationClass[] _contextArray;
        private ObjectMaterializationClass[] _clrArray;
        private ClutchContext _context;

        [GlobalSetup]
        public void Setup()
        {
            _context = ClutchContext.CreateContext(b => { b.Entity<ObjectMaterializationClass>(); }, out _);
            _contextArray = InitArrayContext(_context);
            _clrArray = InitArray();

            //allow serializers to cache models:
             System.Text.Json.JsonSerializer.Deserialize<ObjectMaterializationClass[]>(
                 System.Text.Json.JsonSerializer.Serialize(_clrArray));

            JsonConvert.DeserializeObject(JsonConvert.SerializeObject(_clrArray));

        }

        private static ObjectMaterializationClass[] InitArray()
        {
            int n = 10000;
            var array = new ObjectMaterializationClass[n];

            for (int q = 0; q < n; q++)
            {
                var obj = new ObjectMaterializationClass();

                obj.Test1 = q;
                obj.Test2 = q;
                obj.Test3 = q;
                obj.String1 = "Te";
                obj.String2 = "Te1";
                obj.String3 = "Te2";

                array[q] = obj;
            }

            return array;
        }

        private static ObjectMaterializationClass[] InitArrayContext(ClutchContext context)
        {
            int n = 10000;
            var array = new ObjectMaterializationClass[n];

            var factory = context.GetFactoryMethod<ObjectMaterializationClass>();
            for (int q = 0; q < n; q++)
            {
                var obj = factory();

                obj.Test1 = q;
                obj.Test2 = q;
                obj.Test3 = q;
                obj.String1 = "Te";
                obj.String2 = "Te1";
                obj.String3 = "Te2";

                array[q] = obj;
            }

            return array;
        }

        [Benchmark]
        public void NewtonsoftJsonNet_Serialize()
        {
            JsonConvert.SerializeObject(_clrArray);
        }

        [Benchmark]
        public void NewtonsoftJsonNet_SerializeAndDeserialize()
        {
            var str = JsonConvert.SerializeObject(_clrArray);
            JsonConvert.DeserializeObject(str);
        }

        [Benchmark]
        public void SystemTextJson_Serialize()
        {
            System.Text.Json.JsonSerializer.Serialize(_clrArray);
        }

        [Benchmark]
        public void SystemTextJson_SerializeDeserialize()
        {
            var str = System.Text.Json.JsonSerializer.Serialize(_clrArray);
            var list = System.Text.Json.JsonSerializer.Deserialize<ObjectMaterializationClass[]>(str);
        }

        [Benchmark]
        public void Clutch_Serialize()
        {
            _context.Serialize(_contextArray);
        }

        [Benchmark]
        public void Clutch_SerializeDeserialize()
        {
            var str = _context.Serialize(_contextArray);
            _ = _context.Deserialize<ObjectMaterializationClass>(str);
        }

        [Benchmark]
        public void Clutch_SerializeDeserializeWithBuildingContext()
        {
            var context = ClutchContext.CreateContext(b => { b.Entity<ObjectMaterializationClass>(); }, out _);
            var array = InitArrayContext(context);

            var str = context.Serialize(array);
            context.Deserialize<ObjectMaterializationClass>(str);
        }

        /*
        Initial

        |  Method |      Mean |     Error |    StdDev |
        |-------- |----------:|----------:|----------:|
        | JsonNet | 22.980 ms | 0.4522 ms | 0.4230 ms |
        | Context |  4.442 ms | 0.0515 ms | 0.0482 ms |
        

        moving to escaped property names
        Toolchain=.NET Core 2.2
        
        |  Method |      Mean |     Error |    StdDev |
        |-------- |----------:|----------:|----------:|
        | JsonNet | 22.936 ms | 0.2497 ms | 0.2336 ms |
        | Context |  3.997 ms | 0.0937 ms | 0.0920 ms |
        

        building included(2)
        |   Method |      Mean |     Error |    StdDev |
        |--------- |----------:|----------:|----------:|
        |  JsonNet | 23.445 ms | 0.4689 ms | 0.9145 ms |
        |  Context |  3.961 ms | 0.0410 ms | 0.0363 ms |
        | JsonNet2 | 23.632 ms | 0.4740 ms | 0.9018 ms |
        | Context2 | 12.552 ms | 0.1932 ms | 0.1807 ms |
        

        Core3Serialize is added
        |          Method |     Toolchain |      Mean |     Error |    StdDev |
        |---------------- |-------------- |----------:|----------:|----------:|
        |         JsonNet | .NET Core 2.2 | 23.537 ms | 0.3605 ms | 0.3196 ms |
        |  Core3Serialize | .NET Core 2.2 |  8.043 ms | 0.1565 ms | 0.2343 ms |
        |         Context | .NET Core 2.2 |  6.011 ms | 0.0556 ms | 0.0520 ms |
        |        JsonNet2 | .NET Core 2.2 | 23.835 ms | 0.4417 ms | 0.3915 ms |
        | Core3Serialize2 | .NET Core 2.2 |  8.366 ms | 0.1655 ms | 0.2624 ms |
        |        Context2 | .NET Core 2.2 | 13.239 ms | 0.1791 ms | 0.1588 ms |
        |         JsonNet |        net472 | 25.806 ms | 0.5036 ms | 0.9081 ms |
        |  Core3Serialize |        net472 | 10.736 ms | 0.2001 ms | 0.1965 ms |
        |         Context |        net472 |  6.395 ms | 0.1420 ms | 0.1991 ms |
        |        JsonNet2 |        net472 | 25.687 ms | 0.4363 ms | 0.3643 ms |
        | Core3Serialize2 |        net472 | 10.733 ms | 0.0775 ms | 0.0647 ms |
        |        Context2 |        net472 | 17.232 ms | 0.3276 ms | 0.3899 ms |
        

        Deserialization implemented
        |                          Method |     Toolchain |       Mean |     Error |    StdDev |
        |-------------------------------- |-------------- |-----------:|----------:|----------:|
        |                JsonNetSerialize | .NET Core 2.2 |  25.093 ms | 0.9812 ms | 2.7190 ms |
        |  JsonNetSerializeAndDeserialize | .NET Core 2.2 | 169.533 ms | 3.3851 ms | 4.9618 ms |
        |                  Core3Serialize | .NET Core 2.2 |   8.299 ms | 0.0900 ms | 0.0842 ms |
        |       Core3SerializeDeserialize | .NET Core 2.2 |  22.030 ms | 0.1765 ms | 0.1651 ms |
        |                ContextSerialize | .NET Core 2.2 |   5.980 ms | 0.1242 ms | 0.2143 ms |
        |     ContextSerializeDeserialize | .NET Core 2.2 |  19.574 ms | 0.2603 ms | 0.2307 ms |
        | ContextInitSerializeDeserialize | .NET Core 2.2 |  31.996 ms | 0.6393 ms | 0.8962 ms |
        |                JsonNetSerialize |        net472 |  23.944 ms | 0.1609 ms | 0.1505 ms |
        |  JsonNetSerializeAndDeserialize |        net472 | 176.500 ms | 1.3558 ms | 1.1322 ms |
        |                  Core3Serialize |        net472 |  11.686 ms | 0.2939 ms | 0.2749 ms |
        |       Core3SerializeDeserialize |        net472 |  33.482 ms | 0.3076 ms | 0.2877 ms |
        |                ContextSerialize |        net472 |   7.844 ms | 0.0441 ms | 0.0391 ms |
        |     ContextSerializeDeserialize |        net472 |  32.803 ms | 0.3317 ms | 0.2941 ms |
        | ContextInitSerializeDeserialize |        net472 |  56.637 ms | 0.8895 ms | 0.8321 ms |
        

        After optimization of init
        |                          Method |     Toolchain |       Mean |     Error |    StdDev |
        |-------------------------------- |-------------- |-----------:|----------:|----------:|
        |                JsonNetSerialize | .NET Core 2.2 |  23.953 ms | 0.3960 ms | 0.3510 ms |
        |  JsonNetSerializeAndDeserialize | .NET Core 2.2 | 168.974 ms | 3.5238 ms | 5.4862 ms |
        |                  Core3Serialize | .NET Core 2.2 |   7.897 ms | 0.1819 ms | 0.2022 ms |
        |       Core3SerializeDeserialize | .NET Core 2.2 |  24.283 ms | 0.5476 ms | 0.8998 ms |
        |                ContextSerialize | .NET Core 2.2 |   4.794 ms | 0.1073 ms | 0.1357 ms |
        |     ContextSerializeDeserialize | .NET Core 2.2 |  18.393 ms | 0.1323 ms | 0.1237 ms |
        | ContextInitSerializeDeserialize | .NET Core 2.2 |  24.305 ms | 0.2094 ms | 0.1748 ms | <<
        |                JsonNetSerialize |        net472 |  24.170 ms | 0.4861 ms | 0.6321 ms |
        |  JsonNetSerializeAndDeserialize |        net472 | 178.010 ms | 3.4390 ms | 5.0409 ms |
        |                  Core3Serialize |        net472 |  10.099 ms | 0.1017 ms | 0.0849 ms |
        |       Core3SerializeDeserialize |        net472 |  34.577 ms | 0.2425 ms | 0.2150 ms |
        |                ContextSerialize |        net472 |   6.497 ms | 0.1384 ms | 0.2071 ms |
        |     ContextSerializeDeserialize |        net472 |  30.162 ms | 0.5684 ms | 0.5837 ms |
        | ContextInitSerializeDeserialize |        net472 |  41.539 ms | 0.3226 ms | 0.3017 ms | <<
        

        migrated on latest core 3 libraries
        |                          Method |     Toolchain |       Mean |     Error |    StdDev |     Median |
        |-------------------------------- |-------------- |-----------:|----------:|----------:|-----------:|
        |                JsonNetSerialize | .NET Core 2.2 |  26.961 ms | 0.5212 ms | 0.7959 ms |  26.868 ms |
        |  JsonNetSerializeAndDeserialize | .NET Core 2.2 | 161.592 ms | 3.1191 ms | 3.4669 ms | 161.452 ms |
        |                  Core3Serialize | .NET Core 2.2 |  12.900 ms | 0.2590 ms | 0.3367 ms |  12.815 ms |
        |       Core3SerializeDeserialize | .NET Core 2.2 |  31.778 ms | 0.6270 ms | 1.2079 ms |  31.286 ms |
        |                ContextSerialize | .NET Core 2.2 |   4.694 ms | 0.0713 ms | 0.0632 ms |   4.705 ms |
        |     ContextSerializeDeserialize | .NET Core 2.2 |  19.284 ms | 0.2586 ms | 0.2419 ms |  19.254 ms |
        | ContextInitSerializeDeserialize | .NET Core 2.2 |  26.778 ms | 0.5254 ms | 0.4657 ms |  26.793 ms |
        |                JsonNetSerialize |        net472 |  26.623 ms | 0.5222 ms | 0.9808 ms |  26.292 ms |
        |  JsonNetSerializeAndDeserialize |        net472 | 163.445 ms | 4.0105 ms | 7.3334 ms | 161.374 ms |
        |                  Core3Serialize |        net472 |  15.304 ms | 0.3115 ms | 0.3463 ms |  15.245 ms |
        |       Core3SerializeDeserialize |        net472 |  42.778 ms | 0.7859 ms | 0.6966 ms |  42.790 ms |
        |                ContextSerialize |        net472 |   6.714 ms | 0.1289 ms | 0.1485 ms |   6.675 ms |
        |     ContextSerializeDeserialize |        net472 |  32.047 ms | 0.5520 ms | 0.4610 ms |  31.896 ms |
        | ContextInitSerializeDeserialize |        net472 |  45.361 ms | 0.5671 ms | 0.5304 ms |  45.312 ms |
        


        Updated to .net 6.0
        |                                         Method |        Job |            Toolchain |       Mean |     Error |    StdDev |
        |----------------------------------------------- |----------- |--------------------- |-----------:|----------:|----------:|
        |                    NewtonsoftJsonNet_Serialize | Job-KZPAWI |             .NET 6.0 |  14.907 ms | 0.0221 ms | 0.0196 ms |
        |      NewtonsoftJsonNet_SerializeAndDeserialize | Job-KZPAWI |             .NET 6.0 | 132.012 ms | 1.2940 ms | 1.0103 ms |
        |                       SystemTextJson_Serialize | Job-KZPAWI |             .NET 6.0 |   3.622 ms | 0.0345 ms | 0.0322 ms |
        |            SystemTextJson_SerializeDeserialize | Job-KZPAWI |             .NET 6.0 |  11.184 ms | 0.0143 ms | 0.0134 ms |
        |                               Clutch_Serialize | Job-KZPAWI |             .NET 6.0 |   2.228 ms | 0.0263 ms | 0.0246 ms |
        |                    Clutch_SerializeDeserialize | Job-KZPAWI |             .NET 6.0 |   9.798 ms | 0.0755 ms | 0.0669 ms |
        | Clutch_SerializeDeserializeWithBuildingContext | Job-KZPAWI |             .NET 6.0 |  25.934 ms | 0.1856 ms | 0.1736 ms |
        |                    NewtonsoftJsonNet_Serialize | Job-IMDXNF | .NET Framework 4.7.2 |  19.872 ms | 0.1025 ms | 0.0959 ms |
        |      NewtonsoftJsonNet_SerializeAndDeserialize | Job-IMDXNF | .NET Framework 4.7.2 | 150.681 ms | 2.4712 ms | 2.1907 ms |
        |                       SystemTextJson_Serialize | Job-IMDXNF | .NET Framework 4.7.2 |   8.639 ms | 0.0111 ms | 0.0104 ms |
        |            SystemTextJson_SerializeDeserialize | Job-IMDXNF | .NET Framework 4.7.2 |  24.103 ms | 0.0637 ms | 0.0564 ms |
        |                               Clutch_Serialize | Job-IMDXNF | .NET Framework 4.7.2 |   5.683 ms | 0.0270 ms | 0.0252 ms |
        |                    Clutch_SerializeDeserialize | Job-IMDXNF | .NET Framework 4.7.2 |  24.167 ms | 0.1217 ms | 0.1139 ms |
        | Clutch_SerializeDeserializeWithBuildingContext | Job-IMDXNF | .NET Framework 4.7.2 |  33.751 ms | 0.3051 ms | 0.2854 ms |


        */
    }
}
