using System.Reflection;
using System.Reflection.Emit;

namespace Clutch.Building.ProxySupport
{
    public static class ILGeneratorExtensions
    {
        public static void Return(this ILGenerator g)
        {
            g.Emit(OpCodes.Ret);
        }

        public static Label DefineAndMarkLebel(this ILGenerator g)
        {
            var label = g.DefineLabel();
            g.MarkLabel(label);
            return label;
        }

        public static void LoadThis(this ILGenerator g) => g.Emit(OpCodes.Ldarg_0);

        public static void LoadThisField(this ILGenerator g, FieldInfo field)
        {
            g.LoadThis();
            if (field.IsStatic)
                throw new ClutchInternalErrorException($"Field {field.Name} is declared as static while it is used as instance field");
            g.Emit(OpCodes.Ldfld, field);
        }

        public static void LoadFieldAddress(this ILGenerator g, FieldInfo field)
        {
            if (field.IsStatic)
                throw new ClutchInternalErrorException($"Field {field.Name} is declared as static while it is used as instance field");
            g.Emit(OpCodes.Ldflda, field);
        }

        public static void LoadField(this ILGenerator g, FieldInfo field)
        {
            if (field.IsStatic)
                throw new ClutchInternalErrorException($"Field {field.Name} is declared as static while it is used as instance field");
            g.Emit(OpCodes.Ldfld, field);
        }

        public static void StoreField(this ILGenerator g, FieldInfo field)
        {
            if (field.IsStatic)
                throw new ClutchInternalErrorException($"Field {field.Name} is declared as static while it is used as instance field");
            g.Emit(OpCodes.Stfld, field);
        }

        public static void LoadArgument(this ILGenerator g, int index)
        {
            switch (index)
            {
                case 0:
                    g.Emit(OpCodes.Ldarg_0);
                    break;

                case 1:
                    g.Emit(OpCodes.Ldarg_1);
                    break;

                case 2:
                    g.Emit(OpCodes.Ldarg_2);
                    break;

                case 3:
                    g.Emit(OpCodes.Ldarg_3);
                    break;

                default:
                    g.Emit(OpCodes.Ldarg_S, (byte) index);
                    break;
            }
        }

        public static void LoadArgumentAddress(this ILGenerator g, int index)
        {
            switch (index)
            {
                case 0:
                    g.Emit(OpCodes.Ldarga);
                    break;

                default:
                    g.Emit(OpCodes.Ldarga_S, (byte)index);
                    break;
            }
        }

        public static void Switch(this ILGenerator g, Label[] labels)
        {
            g.Emit(OpCodes.Switch, labels);
        }

        public static void LoadInt64(this ILGenerator g, long value)
        {
            g.Emit(OpCodes.Ldc_I8, value);
        }

        public static void LoadInt64(this ILGenerator g, ulong value)
        {
            g.LoadInt64((long)value);
        }

        public static void LoadDouble(this ILGenerator g, double val)
        {
            g.Emit(OpCodes.Ldc_R8, val);
        }

        public static void LoadSingle(this ILGenerator g, float val)
        {
            g.Emit(OpCodes.Ldc_R4, val);
        }

        public static void LoadInteger(this ILGenerator g, int value)
        {
            switch (value)
            {
                case -1:
                    g.Emit(OpCodes.Ldc_I4_M1);
                    break;
                case 0:
                    g.Emit(OpCodes.Ldc_I4_0);
                    break;
                case 1:
                    g.Emit(OpCodes.Ldc_I4_1);
                    break;
                case 2:
                    g.Emit(OpCodes.Ldc_I4_2);
                    break;
                case 3:
                    g.Emit(OpCodes.Ldc_I4_3);
                    break;
                case 4:
                    g.Emit(OpCodes.Ldc_I4_4);
                    break;
                case 5:
                    g.Emit(OpCodes.Ldc_I4_5);
                    break;
                case 6:
                    g.Emit(OpCodes.Ldc_I4_6);
                    break;
                case 7:
                    g.Emit(OpCodes.Ldc_I4_7);
                    break;
                case 8:
                    g.Emit(OpCodes.Ldc_I4_8);
                    break;

                default:
                    g.Emit(OpCodes.Ldc_I4, value);
                    break;
            }
        }

        public static void New(this ILGenerator g, ConstructorInfo ctor)
        {
            g.Emit(OpCodes.Newobj, ctor);
        }

        public static void CallVirt(this ILGenerator g, MethodInfo method)
        {
            g.Emit(OpCodes.Callvirt, method);
        }

        public static void Goto(this ILGenerator g, Label label)
        {
            g.Emit(OpCodes.Br, label);
        }

        public static void IfNotEqualGoto(this ILGenerator g, Label label)
        {
            g.Emit(OpCodes.Bne_Un, label);
        }

        public static void IfEqualGoto(this ILGenerator g, Label label)
        {
            g.Emit(OpCodes.Beq, label);
        }

        public static void IfTrueGoto(this ILGenerator g, Label label)
        {
            g.Emit(OpCodes.Brtrue, label);
        }

        public static void IfFalseGoto(this ILGenerator g, Label label)
        {
            g.Emit(OpCodes.Brfalse, label);
        }

        public static void StoreLocal(this ILGenerator g, LocalBuilder local)
        {
            StoreLocal(g, local.LocalIndex);
        }

        public static void StoreLocal(this ILGenerator g, int index)
        {
            switch (index)
            {
                case 0:
                    g.Emit(OpCodes.Stloc_0);
                    break;

                case 1:
                    g.Emit(OpCodes.Stloc_1);
                    break;

                case 2:
                    g.Emit(OpCodes.Stloc_2);
                    break;

                case 3:
                    g.Emit(OpCodes.Stloc_3);
                    break;

                default:
                    g.Emit(OpCodes.Stloc_S, (byte)index);
                    break;
            }
        }

        public static void LoadLocal(this ILGenerator g, LocalBuilder local)
        {
            LoadLocal(g, local.LocalIndex);
        }

        public static void LoadLocalAddress(this ILGenerator g, LocalBuilder local)
        {
            LoadLocalAddress(g, local.LocalIndex);
        }

        public static void LoadLocal(this ILGenerator g, int index)
        {
            switch (index)
            {
                case 0:
                    g.Emit(OpCodes.Ldloc_0);
                    break;

                case 1:
                    g.Emit(OpCodes.Ldloc_1);
                    break;

                case 2:
                    g.Emit(OpCodes.Ldloc_2);
                    break;

                case 3:
                    g.Emit(OpCodes.Ldloc_3);
                    break;

                default:
                    g.Emit(OpCodes.Ldloc_S, (byte)index);
                    break;
            }
        }

        public static void LoadLocalAddress(this ILGenerator g, int index)
        {
            g.Emit(OpCodes.Ldloca_S, (byte)index);
        }


        public static void LoadString(this ILGenerator g, string str)
        {
            if (str != null)
                g.Emit(OpCodes.Ldstr, str);
            else
                g.LoadNull();
        } 

        public static void LoadNull(this ILGenerator g) => g.Emit(OpCodes.Ldnull);

        public static void Duplicate(this ILGenerator g) => g.Emit(OpCodes.Dup);

        public static void Pop(this ILGenerator g) => g.Emit(OpCodes.Pop);

        public static void Call(this ILGenerator g, MethodBuilder method) => g.EmitCall(OpCodes.Call, method, null);

        public static void Call(this ILGenerator g, MethodInfo method) => g.EmitCall(OpCodes.Call, method, null);

        public static void CastClass<T>(this ILGenerator g) => g.Emit(OpCodes.Castclass, typeof(T));

        public static void StoreStaticField(this ILGenerator g, FieldBuilder field)
        {
            if (!field.IsStatic)
                throw new ClutchInternalErrorException($"Field {field.Name} is declared as instance while it is used as static");
            g.Emit(OpCodes.Stsfld, field);
        }

        public static void StoreStaticField(this ILGenerator g, FieldInfo field)
        {
            if (!field.IsStatic)
                throw new ClutchInternalErrorException($"Field {field.Name} is declared as instance while it is used as static");
            g.Emit(OpCodes.Stsfld, field);
        }

        public static void LoadStaticField(this ILGenerator g, FieldInfo field)
        {
            if (!field.IsStatic)
                throw new ClutchInternalErrorException($"Field {field.Name} is declared as instance while it is used as static");
            g.Emit(OpCodes.Ldsfld, field);
        }

        public static void LoadStaticFieldAddress(this ILGenerator g, FieldInfo field)
        {
            if (!field.IsStatic)
                throw new ClutchInternalErrorException($"Field {field.Name} is declared as instance while it is used as static");
            g.Emit(OpCodes.Ldsflda, field);
        }
    }
}
