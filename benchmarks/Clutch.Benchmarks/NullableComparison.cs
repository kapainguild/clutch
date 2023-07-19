using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Clutch.Benchmarks
{
    interface INullableHolder
    {
        Guid? Guid { get; set; }

        int Int { get; set; }
    }

    class NullableHolder : INullableHolder
    {
        private Guid? _guid;
        private int _i;

        public Guid? Guid
        {
            get => _guid;
            set
            {
                if (_guid != value)
                {
                    _guid = value;
                }
            }
        }

        public int Int
        {
            get => _i;
            set
            {
                if (_i != value)
                {
                    _i = value;
                }
            }
        }
    }

    public class NullableComparison
    {
        private INullableHolder _clutch;
        private INullableHolder _native;

        public NullableComparison()
        {
            _native = new NullableHolder();
            var ctx = ClutchContext.CreateContext(b => { b.Entity<INullableHolder>().AnyProperty().UsePropertySetterMode(PropertySetterMode.CompareAndSet); }, out _);
            _clutch = ctx.Create<INullableHolder>();
        }

        [Benchmark]
        public void Native()
        {
            for (int q = 0; q < 10000; q++)
            {
                _native.Guid = Guid.Empty;
                _native.Int = q;
            }
        }

        [Benchmark]
        public void Clutch()
        {
            for (int q = 0; q < 10000; q++)
            {
                _clutch.Guid = Guid.Empty;
                _clutch.Int = q;
            }
        }

        /* 
        Toolchain=.NET Core 2.2

        | Method |     Mean |    Error |   StdDev |
        |------- |---------:|---------:|---------:|
        | Native | 246.0 us | 4.897 us | 6.865 us |
        | Clutch | 221.1 us | 4.316 us | 4.038 us |
        */
    }
}
