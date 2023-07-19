using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Clutch.Benchmarks
{
    public enum ProductType
    {
        Type1,
        Type2,
        Type3,
        Type4,
        Type5,
        Type6,
        Type7,
        Type8,
    }

    public class BaseProduct
    {
        public virtual Guid Id { get; set; }

        public virtual string Name { get; set; }

        public virtual string Description { get; set; }

        public virtual int Quantity { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public virtual ProductType ProductType { get; set; }

        public virtual string SupplierName { get; set; }

        public virtual decimal Price { get; set; }

        public virtual int? Nullable { get; set; }
    }

    public class SingleClutch : BaseProduct
    {
        public virtual int SpringQuatity { get; set; }
    }

    public class DualClutch : SingleClutch
    {
        public virtual int SpringQuatity2 { get; set; }
    }

    public class BrakePads : BaseProduct
    {
        public virtual string ExternalValue { get; set; }
    }

    public class BrakePads1 : BaseProduct { }
    public class BrakePads2 : BaseProduct { }
    public class BrakePads3 : BaseProduct { }
    public class BrakePads4 : BaseProduct { }
    public class BrakePads5 : BaseProduct { }
    public class BrakePads6 : BaseProduct { }



    public class SerializationWithInheritance
    {
        private BaseProduct[] _contextArray;
        private BaseProduct[] _clrArray;
        private ClutchContext _context;

        [GlobalSetup]
        public void Setup()
        {
            _context = ClutchContext.CreateContext(b =>
                                                   {
                                                       b.Entity<BaseProduct>();
                                                       b.Entity<SingleClutch>();
                                                       b.Entity<DualClutch>();
                                                       b.Entity<BrakePads>();
                                                       b.Entity<BrakePads1>();
                                                       b.Entity<BrakePads2>();
                                                       b.Entity<BrakePads3>();
                                                       b.Entity<BrakePads4>();
                                                       b.Entity<BrakePads5>();
                                                       b.Entity<BrakePads6>();
                                                   }, out _);
            _contextArray = InitContextArray();
            _clrArray = InitClrArray();
        }

        private BaseProduct[] InitContextArray()
        {
            var result = new List<BaseProduct>();
            Add(result, ContextCreator<BaseProduct>);
            Add(result, ContextCreator<SingleClutch>);
            Add(result, ContextCreator<DualClutch>);
            Add(result, ContextCreator<BrakePads>);
            Add(result, ContextCreator<BrakePads1>);
            Add(result, ContextCreator<BrakePads2>);
            Add(result, ContextCreator<BrakePads3>);
            Add(result, ContextCreator<BrakePads4>);
            Add(result, ContextCreator<BrakePads5>);
            Add(result, ContextCreator<BrakePads6>);

            return result.ToArray();
        }

        private BaseProduct[] InitClrArray()
        {
            var result = new List<BaseProduct>();
            Add(result, ClrCreator<BaseProduct>);
            Add(result, ClrCreator<SingleClutch>);
            Add(result, ClrCreator<DualClutch>);
            Add(result, ClrCreator<BrakePads>);
            Add(result, ClrCreator<BrakePads1>);
            Add(result, ClrCreator<BrakePads2>);
            Add(result, ClrCreator<BrakePads3>);
            Add(result, ClrCreator<BrakePads4>);
            Add(result, ClrCreator<BrakePads5>);
            Add(result, ClrCreator<BrakePads6>);

            return result.ToArray();
        }

        private void Init(BaseProduct product)
        {
            product.Id = Guid.NewGuid();
            product.Name = "Some Name";
            product.Description = "Bla-Bla-Bla-Bla-Bla-Bla-Bla-Bla-Bla-Bla-Bla-Bla\r\n-Bla-Bla-Bla-Bla-Bla-Bla-Bla-Bla-Bla-Bla-Bla-Bla";
            product.Quantity = 4200;
            product.ProductType = ProductType.Type5;
            product.SupplierName = "Bla-Bla-Bla-Bla-Bla-Bla ltd";
            product.Price = 4.222m;
            product.Nullable = 42;
        }

        private void Add<T>(List<BaseProduct> result, Func<T> creator) where T : BaseProduct
        {
            for (int q = 0; q < 1000; q++)
            {
                var obj = creator();
                Init(obj);
                result.Add(obj);
                if (obj is DualClutch d)
                {
                    d.SpringQuatity2 = 10;
                }
            }
        }

        public T ContextCreator<T>() where T : BaseProduct
        {
            return _context.Create<T>();
        }

        public T ClrCreator<T>() where T : BaseProduct, new()
        {
            return new T();
        }

        [Benchmark]
        public void JsonNetSerializeAndDeserialize()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            };
            var str = JsonConvert.SerializeObject(_clrArray, settings);
            var res = JsonConvert.DeserializeObject(str, settings);
        }

        [Benchmark]
        public void Core3SerializeDeserialize()
        {
            // TODO: support polymorph once available
            var str = System.Text.Json.JsonSerializer.Serialize(_clrArray);
            var list = System.Text.Json.JsonSerializer.Deserialize<BaseProduct[]>(str);
        }

        [Benchmark]
        public void ContextSerializeDeserialize()
        {
            var str = _context.Serialize(_contextArray);
            var obj = _context.Deserialize<BaseProduct>(str);
        }
    }
    /* 
    Initial
    |                         Method |      Mean |     Error |    StdDev |    Median |
    |------------------------------- |----------:|----------:|----------:|----------:|
    | JsonNetSerializeAndDeserialize | 158.27 ms | 3.3121 ms | 8.1866 ms | 155.18 ms |
    |      Core3SerializeDeserialize |  49.09 ms | 0.3348 ms | 0.3131 ms |  49.01 ms |  <- no polymorphism supported
    |    ContextSerializeDeserialize |  43.34 ms | 0.8941 ms | 1.6123 ms |  42.60 ms |
    

    Minor optimization
    |                         Method |      Mean |     Error |    StdDev |
    |------------------------------- |----------:|----------:|----------:|
    | JsonNetSerializeAndDeserialize | 157.68 ms | 3.3602 ms | 7.2331 ms |
    |      Core3SerializeDeserialize |  50.80 ms | 0.9908 ms | 1.3562 ms |  <- no polymorphism supported
    |    ContextSerializeDeserialize |  42.70 ms | 0.5707 ms | 0.4766 ms |
    

    migration to .Net core preview 6
    |                         Method |      Mean |     Error |    StdDev |
    |------------------------------- |----------:|----------:|----------:|
    | JsonNetSerializeAndDeserialize | 150.88 ms | 2.9893 ms | 3.6711 ms |
    |      Core3SerializeDeserialize |  54.38 ms | 0.6863 ms | 0.6084 ms |
    |    ContextSerializeDeserialize |  42.73 ms | 0.4748 ms | 0.3707 ms |
    

    migration to .Net 6
    |                         Method |        Job |            Toolchain |      Mean |    Error |   StdDev |
    |------------------------------- |----------- |--------------------- |----------:|---------:|---------:|
    | JsonNetSerializeAndDeserialize | Job-AZZLJS |             .NET 6.0 | 137.69 ms | 2.294 ms | 2.145 ms |
    |      Core3SerializeDeserialize | Job-AZZLJS |             .NET 6.0 |  25.06 ms | 0.037 ms | 0.035 ms |
    |    ContextSerializeDeserialize | Job-AZZLJS |             .NET 6.0 |  22.96 ms | 0.440 ms | 0.540 ms |
    | JsonNetSerializeAndDeserialize | Job-KHPADE | .NET Framework 4.7.2 | 151.04 ms | 2.205 ms | 2.062 ms |
    |      Core3SerializeDeserialize | Job-KHPADE | .NET Framework 4.7.2 |  48.91 ms | 0.412 ms | 0.386 ms |
    |    ContextSerializeDeserialize | Job-KHPADE | .NET Framework 4.7.2 |  48.37 ms | 0.547 ms | 0.512 ms |



     */

}
