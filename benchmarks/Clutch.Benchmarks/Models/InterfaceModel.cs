using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clutch.Benchmarks
{
    interface IBaseInterfaceEntity
    {
        int Id { get; set; }

        string Name { get; set; }
    }

    interface IDerivedInterfaceEntity : IBaseInterfaceEntity
    {
        int SomeInt { get; set; }

        string SomeString { get; set; }
    }

    class InterfaceModelConfigurator : IContextConfigurator
    {
        public void Configure(ClutchContextBuilder builder)
        {
            builder.Entity<BaseClassEntity>();
            builder.Entity<DerivedClassEntity>();
        }
    }
}
