using System.ComponentModel;
using System.Reflection;
using System.Reflection.Emit;
using Clutch.Building;
using Clutch.Building.ProxySupport;
using Clutch.Utility;

namespace Clutch.CoreExtensions.NotifyPropertyChanged
{
    class NotifyPropertyChangedGenerator : ITypeInfluencer, IPropertySetterInfluencer
    {
        private static readonly LazyConstructor s_propertyChangedEventArgsCtor = new LazyConstructor(() => TypeInfoHelper.GetConstructor(() => new PropertyChangedEventArgs(null)));
        private static readonly LazyMethod s_propertyChangedEventHandlerInvoke = new LazyMethod(() => TypeInfoHelper.GetDelegateInvokeMethod<PropertyChangedEventHandler>(s => s(null, null)));

        public void Generate(ProxyBuilderContext ctx)
        {
            var build = ctx.Node.Extensions.GetData<NotifyPropertyChangedExtension, NotifyPropertyChangedEntityInfo>();
            if (build != null)
            {
                if (build.AddInterface)
                    ctx.TypeBuilder.AddInterfaceImplementation(typeof(INotifyPropertyChanged));

                if (build.RaisePropertyChangedMethodInfo == null)
                    build.RaisePropertyChangedMethodInfo = GenerateEventsAndRaiser(ctx.TypeBuilder);
            }
        }

        public void Generate(PropertySetterContext context)
        {
            if (context.Stage == PropertySetterStage.AfterSet && 
                context.Property.PropertyNode.Extensions.GetData<NotifyPropertyChangedExtension, bool>())
            {
                var build = context.Ctx.Node.Extensions.GetData<NotifyPropertyChangedExtension, NotifyPropertyChangedEntityInfo>();
                if (build == null)
                    throw new ClutchInternalErrorException("NotifyPropertyChangedEntityInfo is not set");
                context.Generator.LoadThis();
                context.Generator.LoadString(context.Property.Name);
                context.Generator.Call(build.RaisePropertyChangedMethodInfo);
            }
        }

        public MethodBuilder GenerateEventsAndRaiser(TypeBuilder type)
        {
            var propertyChangedField = type.DefineField(nameof(INotifyPropertyChanged.PropertyChanged), typeof(PropertyChangedEventHandler),
                             FieldAttributes.Private);

            GenerateAddRemove(type, "add", MethodInfos.DelegateCombine.Value, propertyChangedField);
            GenerateAddRemove(type, "remove", MethodInfos.DelegateRemove.Value, propertyChangedField);

            return GenerateRaiseEvent(type, "Raise" + typeof(INotifyPropertyChanged), propertyChangedField);
        }

        private static MethodBuilder GenerateRaiseEvent(TypeBuilder type, string name, FieldBuilder propertyChangedField)
        {
            var method = type.DefineMethod(name,
                                           MethodAttributes.HideBySig |
                                           MethodAttributes.Private,
                                           CallingConventions.Standard,
                                           null,
                                           new[] { typeof(string) });
            var g = method.GetILGenerator();

            var end = g.DefineLabel();
            var doCall = g.DefineLabel();
            var nameArument = 1;

            g.LoadThisField(propertyChangedField);
            g.Duplicate();

            g.IfTrueGoto(doCall);
            g.Pop();
            g.Goto(end);

            g.MarkLabel(doCall);
            g.LoadThis();
            g.LoadArgument(nameArument);
            g.New(s_propertyChangedEventArgsCtor.Value);
            g.CallVirt(s_propertyChangedEventHandlerInvoke.Value);

            g.MarkLabel(end);
            g.Return();
            return method;
        }

        private void GenerateAddRemove(TypeBuilder type, string name, MethodInfo combine, FieldBuilder propertyChangedField)
        {
            var method = type.DefineMethod(name + "_" + nameof(INotifyPropertyChanged.PropertyChanged),
                                           MethodAttributes.Public |
                                           MethodAttributes.Final |
                                           MethodAttributes.HideBySig |
                                           MethodAttributes.Virtual |
                                           MethodAttributes.NewSlot |
                                           MethodAttributes.SpecialName,
                                           CallingConventions.Standard,
                                           null,
                                           new[] { typeof(PropertyChangedEventHandler) });
            var g = method.GetILGenerator();
            var local0 = g.DeclareLocal(typeof(PropertyChangedEventHandler));
            var local1 = g.DeclareLocal(typeof(PropertyChangedEventHandler));
            var local2 = g.DeclareLocal(typeof(PropertyChangedEventHandler));

            int valueArgument = 1;

            g.LoadThisField(propertyChangedField);
            g.StoreLocal(local0);

            var loopBegin = g.DefineAndMarkLebel();

            // local1 = local0
            g.LoadLocal(local0);
            g.StoreLocal(local1);
            g.LoadLocal(local1);
            g.LoadArgument(valueArgument);

            // local2 = Delegate.Combine(local1, value);
            g.Call(combine);
            g.CastClass<PropertyChangedEventHandler>();
            g.StoreLocal(local2);

            g.LoadThis();
            g.LoadFieldAddress(propertyChangedField);
            g.LoadLocal(local2);
            g.LoadLocal(local1);

            // local0 = InterlockedExchange( ref this.PropertyChanged, local2, local1);
            g.Call(MethodInfos.InterlockedCompareExchange.Value);
            g.StoreLocal(local0);
            g.LoadLocal(local0);
            g.LoadLocal(local1);
            // if (local0 != local1) goto :loopBegin
            g.IfNotEqualGoto(loopBegin);

            g.Return();
        }
    }
}
