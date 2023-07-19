using System;
using BenchmarkDotNet.Attributes;

namespace Clutch.Benchmarks
{
    public class ObjectMaterializationClass
    {
        public int _test1;
        public int _test2;
        public int _test3;
        public string _string1;
        public string _string2;
        public string _string3;

        public virtual int Test1
        {
            get => _test1;
            set => _test1 = value;
        }

        public virtual int Test2
        {
            get => _test2;
            set => _test2 = value;
        }

        public virtual int Test3
        {
            get => _test3;
            set => _test3 = value;
        }

        public virtual string String1
        {
            get => _string1;
            set => _string1 = value;
        }

        public virtual string String2
        {
            get => _string2;
            set => _string2 = value;
        }

        public virtual string String3
        {
            get => _string3;
            set => _string3 = value;
        }
    }

    public class ObjectMaterializationClass1 { }
    public class ObjectMaterializationClass2 { }
    public class ObjectMaterializationClass3 { }
    public class ObjectMaterializationClass4 { }
    public class ObjectMaterializationClass5 { }
    public class ObjectMaterializationClass6 { }
    public class ObjectMaterializationClass7 { }
    public class ObjectMaterializationClass8 { }
    public class ObjectMaterializationClass9 { }
    public class ObjectMaterializationClass10 { }
    public class ObjectMaterializationClass11 { }
    public class ObjectMaterializationClass12 { }
    public class ObjectMaterializationClass13 { }
    public class ObjectMaterializationClass14 { }
    public class ObjectMaterializationClass15 { }

    public class ObjectMaterialization
    {
        private ClutchContext _context;
        private Func<ObjectMaterializationClass> _factory;
        private Func<ObjectMaterializationClass> _factoryCompareAndSetProperty;

        [Params(10000)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {
            _context = ClutchContext.CreateContext(b =>
                                                   {
                                                       b.AnyEntityType().AnyProperty();
                                                       b.Entity<ObjectMaterializationClass>().AnyProperty().UsePropertySetterMode(PropertySetterMode.Set).UsePropertyAccessMode(PropertyAccessMode.Field); 
                                                       b.Entity<ObjectMaterializationClass1>();
                                                       b.Entity<ObjectMaterializationClass2>();
                                                       b.Entity<ObjectMaterializationClass3>();
                                                       b.Entity<ObjectMaterializationClass4>();
                                                       b.Entity<ObjectMaterializationClass5>();
                                                       b.Entity<ObjectMaterializationClass6>();
                                                       b.Entity<ObjectMaterializationClass7>();
                                                       b.Entity<ObjectMaterializationClass8>();
                                                       b.Entity<ObjectMaterializationClass9>();
                                                       b.Entity<ObjectMaterializationClass10>();
                                                       b.Entity<ObjectMaterializationClass11>();
                                                       b.Entity<ObjectMaterializationClass12>();
                                                       b.Entity<ObjectMaterializationClass13>();
                                                       b.Entity<ObjectMaterializationClass14>();
                                                       b.Entity<ObjectMaterializationClass15>();
                                                   }, out _);
            _factory = _context.GetFactoryMethod<ObjectMaterializationClass>();

            _factoryCompareAndSetProperty = ClutchContext.CreateContext(b =>
                                                          {
                                                              b.Entity<ObjectMaterializationClass>().AnyProperty().UsePropertySetterMode(PropertySetterMode.CompareAndSet).UsePropertyAccessMode(PropertyAccessMode.Property);
                                                          }, out _).GetFactoryMethod<ObjectMaterializationClass>();
        }

        private static void SetProperties(ObjectMaterializationClass obj, int count)
        {
            for (int q = 0; q < count; q++)
            {
                obj.Test1 = q;
                obj.Test2 = q;
                obj.Test3 = q;
                obj.String1 = "Te";
                obj.String2 = "Te1";
                obj.String3 = "Te2";
            }
        }

        [Benchmark]
        public ObjectMaterializationClass ClrSetProperty()
        {
            ObjectMaterializationClass last = new ObjectMaterializationClass();
            SetProperties(last, N);
            return last;
        }

        [Benchmark]
        public ObjectMaterializationClass ContextSetPropertyWithSetField()
        {
            ObjectMaterializationClass last = _factory();
            SetProperties(last, N);
            return last;
        }

        [Benchmark]
        public ObjectMaterializationClass ContextSetPropertyWithCompareAndSetProperty()
        {
            ObjectMaterializationClass last = _factoryCompareAndSetProperty();
            SetProperties(last, N);
            return last;
        }

        [Benchmark]
        public ObjectMaterializationClass ClrNew()
        {
            ObjectMaterializationClass last = null;
            for (int q = 0; q < N; q++)
                last = new ObjectMaterializationClass();
            return last;
        }

        [Benchmark]
        public ObjectMaterializationClass ContextNew()
        {
            ObjectMaterializationClass last = null;
            for (int q = 0; q < N; q++)
                last = _context.Create<ObjectMaterializationClass>();
            return last;
        }

        [Benchmark]
        public ObjectMaterializationClass ContextFactoryNew()
        {
            ObjectMaterializationClass last = null;
            for (int q = 0; q < N; q++)
                last = _factory();
            return last;
        }

        /* 
         * Initial
        |                                      Method |     N |      Mean |     Error |    StdDev |
        |-------------------------------------------- |------ |----------:|----------:|----------:|
        |                              ClrSetProperty | 10000 | 125.48 us | 0.9297 us | 0.8696 us |
        |              ContextSetPropertyWithSetField | 10000 | 125.89 us | 0.9798 us | 0.8686 us |
        | ContextSetPropertyWithCompareAndSetProperty | 10000 | 131.46 us | 2.6238 us | 4.0850 us |
        |                                      ClrNew | 10000 |  51.33 us | 0.7027 us | 0.6573 us |
        |                                  ContextNew | 10000 | 330.86 us | 4.1924 us | 3.7165 us |
        |                           ContextFactoryNew | 10000 | 159.56 us | 3.1167 us | 5.0328 us |

           
        CreateNew optimization via proxy factory
        |            Method |     N |      Mean |     Error |    StdDev |
        |------------------ |------ |----------:|----------:|----------:|
        |            ClrNew | 10000 |  50.00 us | 0.8010 us | 0.7493 us |
        |        ContextNew | 10000 | 170.86 us | 1.8876 us | 1.6733 us |
        | ContextFactoryNew | 10000 | 153.06 us | 2.9847 us | 4.7341 us |
        
         
        Migration to .Net 6
        |                                      Method |        Job |            Toolchain |     N |      Mean |    Error |   StdDev |
        |-------------------------------------------- |----------- |--------------------- |------ |----------:|---------:|---------:|
        |                              ClrSetProperty | Job-DHTKKM |             .NET 6.0 | 10000 | 102.66 us | 0.354 us | 0.331 us |
        |              ContextSetPropertyWithSetField | Job-DHTKKM |             .NET 6.0 | 10000 | 101.30 us | 0.765 us | 0.716 us |
        | ContextSetPropertyWithCompareAndSetProperty | Job-DHTKKM |             .NET 6.0 | 10000 |  78.71 us | 1.051 us | 0.983 us |
        |                                      ClrNew | Job-DHTKKM |             .NET 6.0 | 10000 |  32.99 us | 0.363 us | 0.321 us |
        |                                  ContextNew | Job-DHTKKM |             .NET 6.0 | 10000 | 147.41 us | 2.089 us | 1.852 us |
        |                           ContextFactoryNew | Job-DHTKKM |             .NET 6.0 | 10000 | 101.06 us | 1.044 us | 0.977 us |
        |                              ClrSetProperty | Job-QRNXDV | .NET Framework 4.7.2 | 10000 | 110.85 us | 0.244 us | 0.228 us |
        |              ContextSetPropertyWithSetField | Job-QRNXDV | .NET Framework 4.7.2 | 10000 | 110.94 us | 0.293 us | 0.274 us |
        | ContextSetPropertyWithCompareAndSetProperty | Job-QRNXDV | .NET Framework 4.7.2 | 10000 | 127.73 us | 0.618 us | 0.578 us |
        |                                      ClrNew | Job-QRNXDV | .NET Framework 4.7.2 | 10000 |  31.45 us | 0.582 us | 0.545 us |
        |                                  ContextNew | Job-QRNXDV | .NET Framework 4.7.2 | 10000 | 147.80 us | 2.356 us | 2.203 us |
        |                           ContextFactoryNew | Job-QRNXDV | .NET Framework 4.7.2 | 10000 | 117.82 us | 1.304 us | 1.089 us |
         
         
         
         */
    }
}
