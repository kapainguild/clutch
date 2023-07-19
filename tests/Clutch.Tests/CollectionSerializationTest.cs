using System.Collections.Generic;
using Clutch.CoreExtensions.NotifyPropertyChanged;
using Clutch.Tests.Helpers;
using Clutch.Tests.Models;
using Xunit;

namespace Clutch.Tests
{
    public class CollectionSerializationTest
    {
        [Fact]
        public void PrimitiveTypes()
        {
            var ctx = Checker.BuildsWithoutIssues(c =>
                                                  {
                                                      c.Entity<ICollectionModel>().Property(p => p.StringList); //TODO
                                                      c.Entity<ICollectionModel>().Property(p => p.Str); //TODO
                                                  });

            var obj = ctx.Create<ICollectionModel>();
            obj.StringList.Add("4");
            obj.StringList.Add("2");
            var list = new[] { obj };
            var str = ctx.Serialize(list);
            var de = ctx.Deserialize<ICollectionModel>(str);
            Assert.Single(de);
            
            Assert.Equal(2, obj.StringList.Count);
            Assert.Equal("4", obj.StringList[0]);
            Assert.Equal("2", obj.StringList[1]);
        }
    }
}
