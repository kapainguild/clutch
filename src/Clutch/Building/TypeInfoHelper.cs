using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Clutch.Building
{
    static class TypeInfoHelper
    {
        public static MethodInfo GetGenericStaticMethod<TParam, TResult>(Expression<Func<TParam, TResult>> expression)
        {
            return ((MethodCallExpression)expression.Body).Method;
        }

        public static MethodInfo GetStaticMethod<TResult>(Expression<Func<TResult>> expression)
        {
            return ((MethodCallExpression)expression.Body).Method;
        }

        public static MethodInfo GetStaticMethod(Expression<Action> expression)
        {
            return ((MethodCallExpression)expression.Body).Method;
        }

        public static MethodInfo GetStaticMethod(Type type, string name, Type[] arguments)
        {
            return type.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null,
                                       arguments, null);
        }

        public static MethodInfo GetInstanceMethod<T, TResult>(Expression<Func<T, TResult>> expression)
        {
            return ((MethodCallExpression)expression.Body).Method;
        }

        public static MethodInfo GetInstanceMethod(Expression<Action> expression)
        {
            return ((MethodCallExpression)expression.Body).Method;
        }

        public static MethodInfo GetInstanceMethod(Type type, string name)
        {
            return type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }


        public static MethodInfo GetInstanceMethod<TResult>(Expression<Func<TResult>> expression)
        {
            return ((MethodCallExpression)expression.Body).Method;
        }

        public static ConstructorInfo GetConstructor<T>(Expression<Func<T>> expression)
        {
            return ((NewExpression)expression.Body).Constructor;
        }

        public static ConstructorInfo GetConstructor(Expression expression)
        {
            return ((NewExpression)expression).Constructor;
        }

        public static ConstructorInfo GetConstructor(Type t, Type[] arguments)
        {
            return t.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, arguments, null);
        }

        public static MethodInfo GetDelegateInvokeMethod<T>(Expression<Action<T>> expression)
        {
            var invokation = ((InvocationExpression)expression.Body);

            var type = typeof(T);
            var result = type.GetMethod(nameof(PropertyChangedEventHandler.Invoke), invokation.Arguments.Select(s => s.Type).ToArray());
            return result;
        }

        internal static ConstructorInfo GetParameterlessConstructor(Type type)
        {
            return type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
        }

        public static MethodInfo GetPropertyGetter<TResult>(Expression<Func<TResult>> expression)
        {
            return GetPropertyGetter(expression.Body);
        }

        public static MethodInfo GetPropertyGetter(Type type, string name)
        {
            return type.GetProperty(name).GetGetMethod();
        }


        public static MethodInfo GetPropertyGetter(Expression expression)
        {
            if (expression.NodeType != ExpressionType.MemberAccess)
                throw new ClutchInternalErrorException($"Expression of '{ExpressionType.MemberAccess}' type expected while '{expression.NodeType}' is provided");

            var call = (MemberExpression)expression;

            return ((PropertyInfo)call.Member).GetMethod;
        }

        public static Expression ViaToString(Expression<Func<string>> expression)
        {
            if (expression.Body.NodeType != ExpressionType.Call)
                throw new ClutchInternalErrorException($"Expression of '{ExpressionType.Call}' type expected while '{expression.NodeType}' is provided ({nameof(ViaToString)})");

            var call = (MethodCallExpression)expression.Body;

            return call.Object;
        }

        public static MethodInfo GetInstanceMethod(LambdaExpression expression)
        {
            if (expression.Body.NodeType != ExpressionType.Call)
                throw new ClutchInternalErrorException($"Expression of '{ExpressionType.Call}' type expected while '{expression.NodeType}' is provided ({nameof(GetInstanceMethod)})");

            var call = (MethodCallExpression)expression.Body;

            return call.Method;
        }
    }
}
