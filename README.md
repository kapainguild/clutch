# Clutch

## Idea
This is an attempt to implement AOP (Aspect Oriented Programming) approches on data structures.
The final target of the project is yet unknown. One of the outcomes of the approach is high performance json
serializer that outperforms the one from **System.Text.Json** in many scenarious.

So, it is supposed that the data is presented as a set of interfaces like this

```cs
interface IRoot
{
    int IntProperty { get; set; } 
    IChild Child { get; set; }
}

interface IChild
{
    string StrProperty {get;set;}
    IList<string> { get; }
    int? NullableInt { get; set; }
}
```

We also support classes with virtual properties but approach with the interfaces seems to be more elegant.
So, idea of the project is that the Clutch framework will generate implementation of the interfaces under the hood. The implementation is done by using **System.Reflection.Emit** namespace. Implemented objects may receive different aspects depending on configuration. Configuration may look like:

```cs
var context = ClutchContext.CreateContext(c =>
{
    c.UseNotifyPropertyChanged(NotifyPropertyChangedBehavior.ImplementOnlyOnEntitiesWithEnabledProperties);

    c.Entity<IChild>(c =>
    {
        c.Property(e => e.StrProperty).
            HasDefaultValue("").
            UsePropertySetterMode(PropertySetterMode.CompareAndSet).
            EnableNotifyPropertyChanged(true);
        c.Property(e => e.NullableInt).
            HasDefaultValue(null).
            UsePropertySetterMode(PropertySetterMode.Set).
            EnableNotifyPropertyChanged(true);
    });
    c.Entity<IRoot>().AnyProperty().EnableNotifyPropertyChanged(true);

}, out var issueList);
```

Based on the configuration, the implementation of interfaces will be generated. Then you can do like:
```cs
bool raised = false;
var entity = context.Create<IRoot>();
((INotifyPropertyChanged)entity).PropertyChanged += (e, s) => raised = true;

entity.IntProperty = 42;
Assert.IsTrue(raised);
```


## Performance

Credits to Benchmark.Net project, the following setup was used (some details omitted for clarity):

```cs

    public interface IEntity
    {
        int Test1 { get; set; }
        int Test2 { get; set; }
        int Test3 { get; set; }
        string String1 { get; set; }
        string String2 { get; set; }
        string String3 { get; set; }
    }

    public class Serialization
    {
        private IEntity[] _array;
        private ClutchContext _context;

        [GlobalSetup]
        public void Setup()
        {
            _context = ClutchContext.CreateContext(b => { b.Entity<IEntity>(); }, out _);
            _array = InitArray(10000);
        }

        private static ObjectMaterializationClass[] InitArray(int amount) {...}


        [Benchmark]
        public void Clutch_SerializeDeserialize()
        {
            var str = _context.Serialize(_contextArray);
            _ = _context.Deserialize<IEntity>(str);
        }
    }

```

Here is performance benchmark on serializing a list of 10000 objects with 3 string and 3 integer properties set:

|                                         Method |            Toolchain |       Mean |
|----------------------------------------------- |--------------------- |-----------:|
|                    NewtonsoftJsonNet_Serialize |             .NET 6.0 |  14.907 ms |
|      NewtonsoftJsonNet_SerializeAndDeserialize |             .NET 6.0 | 132.012 ms |
|                       SystemTextJson_Serialize |             .NET 6.0 |   3.622 ms |
|            SystemTextJson_SerializeDeserialize |             .NET 6.0 |  11.184 ms |
|                               Clutch_Serialize |             .NET 6.0 |   2.228 ms |
|                    Clutch_SerializeDeserialize |             .NET 6.0 |   9.798 ms |
| Clutch_SerializeDeserializeWithBuildingContext |             .NET 6.0 |  25.934 ms |
|----------------------------------------------- |--------------------- |-----------:|
|                    NewtonsoftJsonNet_Serialize | .NET Framework 4.7.2 |  19.872 ms |
|      NewtonsoftJsonNet_SerializeAndDeserialize | .NET Framework 4.7.2 | 150.681 ms |
|                       SystemTextJson_Serialize | .NET Framework 4.7.2 |   8.639 ms |
|            SystemTextJson_SerializeDeserialize | .NET Framework 4.7.2 |  24.103 ms |
|                               Clutch_Serialize | .NET Framework 4.7.2 |   5.683 ms |
|                    Clutch_SerializeDeserialize | .NET Framework 4.7.2 |  24.167 ms |
| Clutch_SerializeDeserializeWithBuildingContext | .NET Framework 4.7.2 |  33.751 ms |