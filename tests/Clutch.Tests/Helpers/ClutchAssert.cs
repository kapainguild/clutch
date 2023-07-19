using System;
using System.ComponentModel;
using System.Linq.Expressions;
using Xunit;

namespace Clutch.Tests.Helpers
{
    static class ClutchAssert
    {
        public static object ImplementsNotifyPropertyChanged(object obj, out INotifyPropertyChanged npc)
        {
            npc = obj as INotifyPropertyChanged;
            Assert.NotNull(npc);
            return obj;
        }

        public static object ImplementsNotifyPropertyChanged(object obj)
        {
            var npc = obj as INotifyPropertyChanged;
            Assert.NotNull(npc);
            return obj;
        }

        public static object DoesNotImplementNotifyPropertyChanged(object obj)
        {
            var npc = obj as INotifyPropertyChanged;
            Assert.Null(npc);
            return obj;
        }

        public static void DoesNotRaisePropertyChanged<TObj, TProperty>(TObj obj, Expression<Func<TObj, TProperty>> getter, Action<TObj> testCode)
        {
            ImplementsNotifyPropertyChanged(obj, out var npc);
            Assert.Equal(0, GetPropertyChangedCount(npc, GetMemberName(getter), () => testCode(obj)));
        }

        public static void RaisesPropertyChanged<TObj, TProperty>(TObj obj, Expression<Func<TObj, TProperty>> getter, Action<TObj> testCode)
        {
            ImplementsNotifyPropertyChanged(obj, out var npc);
            Assert.Equal(1, GetPropertyChangedCount(npc, GetMemberName(getter), () => testCode(obj)));
        }

        private static string GetMemberName<TObj, TProperty>(Expression<Func<TObj, TProperty>> getter)
        {
            if (getter.Body is MemberExpression memberAccess)
            {
                return memberAccess.Member.Name;
            }
            Assert.False(true, "Getter is not getter");
            return null;
        }

        private static int GetPropertyChangedCount(INotifyPropertyChanged npc, string propertyName, Action testCode)
        {
            int propertyChangeCount = 0;

            void ChangedEventHandler(object sender, PropertyChangedEventArgs args)
            {
                if (propertyName == args.PropertyName) propertyChangeCount++;
            }

            npc.PropertyChanged += ChangedEventHandler;
            try
            {
                testCode();
            }
            finally
            {
                npc.PropertyChanged -= ChangedEventHandler;
            }

            return propertyChangeCount;
        }
    }
}
