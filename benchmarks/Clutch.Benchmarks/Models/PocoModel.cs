using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clutch.Benchmarks
{
    class BaseClassPoco
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    class DerivedClassPoco : BaseClassPoco
    {
        public int SomeInt { get; set; }

        public string SomeString { get; set; }
    }
}
