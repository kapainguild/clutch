using System;
using System.Linq;
using System.Reflection.Emit;

namespace Clutch.Building
{
    public abstract class StaticFieldDeclaration
    {
        public Type Type { get; set; }

        public string Name { get; set; }

        public FieldBuilder FieldBuilder { get; set; }

        public abstract void Init(StaticFieldDeclaration[] fields, Delegate del);
    }

    class StaticFieldDeclaration<T> : StaticFieldDeclaration
    {
        public Func<T> ValueGetter { get; set; }

        public StaticFieldDeclaration(string name, Func<T> valueGetter)
        {
            ValueGetter = valueGetter;
            Name = name;
            Type = typeof(T);
        }

        public override void Init(StaticFieldDeclaration[] fields, Delegate del)
        {
            var init = (Action<T[]>)del;
            var values = fields.OfType<StaticFieldDeclaration<T>>().Select(s => s.ValueGetter()).ToArray();
            init(values);
        }
    }
}
