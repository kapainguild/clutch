using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clutch.Benchmarks
{
    class BaseClassEntity
    {
        public virtual int Id { get; set; }

        public virtual string Name { get; set; }
    }

    class DerivedClassEntity : BaseClassEntity
    {
        public virtual int SomeInt { get; set; }

        public virtual string SomeString { get; set; }
    }

    class ClassModelConfigurator : IContextConfigurator
    {
        public void Configure(ClutchContextBuilder builder)
        {
            builder.Entity<BaseClassEntity>();
            builder.Entity<DerivedClassEntity>();
        }
    }
}
