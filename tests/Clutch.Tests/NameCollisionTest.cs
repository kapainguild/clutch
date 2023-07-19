using System;
using System.Collections.Generic;
using System.Text;
using Clutch.Tests.Helpers;
using Xunit;

namespace Clutch.Tests2
{
    class NameCollision { }
}

namespace Clutch.Tests
{
    class NameCollision { }

    public class NameCollisionTest
    {
        [Fact]
        public void NamespaceCollision()
        {
            Checker.ConfigurationFails(c =>
                                       {
                                           c.Entity<Tests.NameCollision>();
                                           c.Entity<Tests2.NameCollision>();
                                       },
                                       CoreIssues.TypeNameCollisionIsNotSupported.With(s => s.Type == typeof(Tests2.NameCollision), 
                                                                                       args => args.typeName1 == typeof(Tests2.NameCollision).FullName && args.typeName2 == typeof(Tests.NameCollision).FullName));
        }

        class NameCollision
        {

        }

        [Fact]
        public void InnerClassCollision()
        {
            Checker.ConfigurationFails(c =>
                                       {
                                           c.Entity<NameCollision>();
                                           c.Entity<Tests.NameCollision>();
                                       },
                                       CoreIssues.TypeNameCollisionIsNotSupported.With(s => s.Type == typeof(Tests.NameCollision),
                                                                                       args => args.typeName1 == typeof(Tests.NameCollision).FullName && args.typeName2 == typeof(NameCollision).FullName));
        }
    }
}
