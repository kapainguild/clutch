using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Clutch.Building.ProxySupport;
using Clutch.Helpers;

namespace Clutch.Building
{
    public class StaticFieldBuilder
    {
        private readonly TypeBuilder _typeBuilder;
        private readonly MemoryManager _memoryManager;
        private readonly List<StaticFieldDeclaration> _staticFields = new List<StaticFieldDeclaration>();

        private readonly List<StaticMethodDeclaration> _staticMethods = new List<StaticMethodDeclaration>();
        private readonly Dictionary<object, StaticFieldDeclaration> _groupedByValues = new Dictionary<object, StaticFieldDeclaration>();

        public StaticFieldBuilder(ModuleBuilder moduleBuilder, MemoryManager memoryManager)
        {
            _typeBuilder = moduleBuilder.DefineType("StaticFields", TypeAttributes.Class | TypeAttributes.Public);
            _memoryManager = memoryManager;
        }

        public StaticFieldDeclaration Add<T>(string name, Func<T> valueGetter)
        {
            var result = new StaticFieldDeclaration<T>(GetName(name), valueGetter);
            result.FieldBuilder = _typeBuilder.DefineField(ProxyBuilderConstants.StaticFieldPrefix + result.Name, result.Type, ProxyBuilderConstants.DefaultStaticFieldAttributes);
            _staticFields.Add(result);

            return result;
        }

        public StaticFieldDeclaration AddGroupedByValue<T>(string name, Func<T> valueGetter)
        {
            if (!_groupedByValues.TryGetValue(valueGetter(), out var declaration))
            {
                declaration = Add(name, valueGetter);
                _groupedByValues.Add(valueGetter(), declaration);
            }
            return declaration;
        }

        private static string GetName(string name) => $"_{name}_{Guid.NewGuid():N}";

        public StaticMethodDeclaration DeclareMethod(string name, Type delegateType, bool requiresDelegate = false)
        {
            Type returnType = null;
            Type[] parameterTypes = null;
            if (!delegateType.IsGenericType)
            {
                if (delegateType == typeof(Action))
                {
                    parameterTypes = Type.EmptyTypes;
                }
                else 
                    throw new ClutchInternalErrorException($"Unsupported delegate type {delegateType}");
            }
            else
            {
                Type generic = delegateType.GetGenericTypeDefinition();
                if (generic == typeof(Func<>) || generic == typeof(Func<,>) || generic == typeof(Func<,,>) || generic == typeof(Func<,,,>) || generic == typeof(Func<,,,,>))
                {
                    returnType = delegateType.GenericTypeArguments.Last();
                    parameterTypes = delegateType.GenericTypeArguments.Take(delegateType.GenericTypeArguments.Length - 1).ToArray();
                }
                else if (generic == typeof(Action<>) || generic == typeof(Action<,>) || generic == typeof(Action<,,>) || generic == typeof(Action<,,,>) || generic == typeof(Action<,,,,>))
                {
                    parameterTypes = delegateType.GenericTypeArguments;
                }
                else
                    throw new ClutchInternalErrorException($"Unsupported delegate type {delegateType}");
            }

            return DeclareMethod(name, delegateType, returnType, parameterTypes, requiresDelegate);
        }

        public StaticMethodDeclaration DeclareMethod(string name, Type delegateType, Type returnType, Type[] paramTypes, bool requiresDelegate = false)
        {
            var methodName = GetName(name);
            var result = new StaticMethodDeclaration
                         {
                             RequiresDelegate = requiresDelegate,
                             DelegateType = delegateType,
                             Name = methodName,
                             MethodBuilder = _typeBuilder.DefineMethod(methodName, MethodAttributes.Public | MethodAttributes.Static, returnType, paramTypes)
                         };

            _staticMethods.Add(result);
            return result;
        }

        public void Initialize()
        {
            var groups = _staticFields.GroupBy(s => s.Type).Select(s => new { type = s.Key, fields = s.ToArray()}).ToArray();

            var methods = groups.Select(s => new { group = s, method = DeclareInitializeField(s.type, s.fields)}).ToArray();

            var typeInfo = _typeBuilder.CreateTypeInfo();

            methods.ForEach(group => InitializeStaticField(typeInfo, group.method, group.group.type, group.group.fields));


            _staticMethods.Where(s => s.RequiresDelegate).ForEach(s => BuildDelegate(typeInfo, s));
        }

        private static void BuildDelegate(TypeInfo typeInfo, StaticMethodDeclaration staticMethodDeclaration)
        {
            staticMethodDeclaration.UntypedDelegate = typeInfo.GetDeclaredMethod(staticMethodDeclaration.Name).CreateDelegate(staticMethodDeclaration.DelegateType);
        }

        private static void InitializeStaticField(TypeInfo typeInfo, MethodBuilder method, Type type, StaticFieldDeclaration[] fields)
        {
            var del = typeInfo.GetDeclaredMethod(method.Name).CreateDelegate(typeof(Action<>).MakeGenericType(type.MakeArrayType()));
            fields[0].Init(fields, del);
        }

        private MethodBuilder DeclareInitializeField(Type type, StaticFieldDeclaration[] field)
        {
            // set method
            var methodBuilder = _typeBuilder.DefineMethod(ProxyBuilderConstants.StaticFieldSetMethodPrefix + "initializeStatic" + type.Name,
                                                              ProxyBuilderConstants.DefaultStaticMethodAttributes,
                                                              null,
                                                              new[] { type.MakeArrayType() });

            var g = methodBuilder.GetILGenerator();
            for (int idx = 0; idx < field.Length; idx++)
            {
                g.LoadArgument(0);
                g.LoadInteger(idx);
                g.Emit(OpCodes.Ldelem, type);
                g.StoreStaticField(field[idx].FieldBuilder);
            }
            g.Return();

            return methodBuilder;
        }

        public StaticFieldDeclaration AllocateString(string str)
        {
            var field = _memoryManager.Allocate(str);
            return Add(str + "Value", () => field.EncodedText);
        }
    }
}
