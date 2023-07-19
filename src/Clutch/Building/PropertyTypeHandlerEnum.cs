using System;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text.Json;
using Clutch.Building.ProxySupport;

namespace Clutch.Building
{
    class PropertyTypeHandlerEnum
    {
        internal delegate object EnumSwitcherDelegate(ref ReadOnlySpan<byte> reader);

        internal static PropertyTypeHandler Build<T>(PropertyTypeHandlerFactory propertyTypeHandlerFactory, PropertyTypeHandlerBuildContext ctx)
        {
            var type = typeof(T);
            var underlyingType = Enum.GetUnderlyingType(type);
            var underlyingTypeHandler = propertyTypeHandlerFactory.GetUnderlyingTypeHandler(underlyingType, ctx);

            return Build<T>(type, underlyingType, underlyingTypeHandler, propertyTypeHandlerFactory);
        }

        public static PropertyTypeHandler Build<T>(Type type, Type underlyingType, PropertyTypeHandler underlyingTypeHandler, PropertyTypeHandlerFactory propertyTypeHandlerFactory)
        {
            return new PropertyTypeHandler
                   {
                       CompareAndGotoLabel = underlyingTypeHandler.CompareAndGotoLabel,
                       Serialize = ctx => EnumSerializer(ctx, type, underlyingType, underlyingTypeHandler),
                       Deserialize = ctx => EnumDeserializer(ctx, type, underlyingType, underlyingTypeHandler),
                       LoadDefaultValue = PrimitiveTypeHandlers.LoadDefaultStruct<T>(),
                       IsSystemDefaultValue = underlyingTypeHandler.IsSystemDefaultValue
                   };
        }

        private static PropertyTypeHandlerEnumCacheEntry BuildCacheEntry(GeneratorPropertyContext ctx, Type type, Type underlyingType)
        {
            var proxyAssemblyContext = ctx.Property.AssemblyContext;

            var entries = GetEntries(type, underlyingType, proxyAssemblyContext);
            return new PropertyTypeHandlerEnumCacheEntry
                   {
                       DeserializeSwitcher = BuildDeserializeSwitcher(entries, type, underlyingType, proxyAssemblyContext),
                       SerializeSwitcher = BuildSerializeSwitcher(entries, type, proxyAssemblyContext)
                   };
        }

        private static PropertyTypeHandlerEnumCacheEntry GetOrCreateCacheEntry(GeneratorPropertyContext ctx, Type type, Type underlyingType)
        {
            var propertyTypeHandlerFactory = ctx.Property.AssemblyContext.PropertyTypeHandlerFactory;
            return propertyTypeHandlerFactory.GetOrCreateEnumEntry(type, () => BuildCacheEntry(ctx, type, underlyingType));
        }


        private static EnumEntry[] GetEntries(Type type, Type underlyingType, ProxyAssemblyContext proxyAssemblyContext)
        {
            bool underlyingTypeIsUInt64 = underlyingType == typeof(ulong);

            var values = Enum.GetValues(type);
            var names = Enum.GetNames(type);
            var utf8 = names.Select(s => proxyAssemblyContext.MemoryManager.Allocate(s)).ToArray();
            return values.OfType<object>().Select((o, i) => new EnumEntry
                                                                   {
                                                                       ULongValue = underlyingTypeIsUInt64? Convert.ToUInt64(o) : unchecked((ulong)Convert.ToInt64(o)),
                                                                       Utf8Name = proxyAssemblyContext.StaticFields.Add("EnumSwitcher" + type.Name + names[i], () => utf8[i].EncodedText),
                                                                       StringHash = ProxyHelperFunctions.GetFastHashCode(JsonEncodedText.Encode(names[i]).EncodedUtf8Bytes)
                                                                   }).ToArray();
        }


        private static void EnumSerializer(SerializePropertyContext ctx, Type type, Type underlyingType, PropertyTypeHandler underlyingTypeHandler)
        {
            var enumEntry = GetOrCreateCacheEntry(ctx, type, underlyingType);

            var g = ctx.Generator;
            var end = g.DefineLabel();

            ctx.GenerateGetPropertyValue(false);
            g.Emit(OpCodes.Conv_U8);

            g.LoadArgument(1);
            g.LoadStaticField(ctx.Property.EscapedPropertyNameField.FieldBuilder);

            g.Call(enumEntry.SerializeSwitcher);
            g.IfTrueGoto(end);
            //serialize as number
            underlyingTypeHandler.Serialize(ctx);

            g.MarkLabel(end);
        }


        private static MethodBuilder BuildSerializeSwitcher(EnumEntry[] entries, Type type, ProxyAssemblyContext proxyAssemblyContext)
        {
            entries = entries.OrderBy(s => s.ULongValue).ToArray();

            var switcher = proxyAssemblyContext.StaticFields.DeclareMethod("Switcher" + type.Name, typeof(Func<ulong, Utf8JsonWriter, JsonEncodedText, bool>));
            var gen = switcher.MethodBuilder.GetILGenerator();
            TreeSwitchBuilder.GenerateTreeBool(gen, entries,
                                               (g, entry, label) => TreeSwitchBuilder.ULongGoIfGreater(g, entry.ULongValue, label, () => g.LoadArgument(0)),
                                               (g, entry, label) => TreeSwitchBuilder.ULongGoIfNotEqual(g, entry.ULongValue, label, () => g.LoadArgument(0)),
                                               (g, entry, returnTrue, returnFalse) =>
                                               {
                                                   //load writer
                                                   g.LoadArgument(1);
                                                   // load propertyName
                                                   g.LoadArgument(2);
                                                   // load value
                                                   g.LoadStaticField(entry.Utf8Name.FieldBuilder);
                                                   //serialize
                                                   g.Emit(OpCodes.Call, MethodInfos.WriteJsonEncodedText);
                                                   g.Goto(returnTrue);
                                               });
            gen.Return();

            return switcher.MethodBuilder;
        }


        private static void EnumDeserializer(DeserializePropertyContext ctx, Type type, Type underlyingType, PropertyTypeHandler underlyingTypeHandler)
        {
            var enumEntry = GetOrCreateCacheEntry(ctx, type, underlyingType);
            var g = ctx.Generator;
            var parseAsString = g.DefineLabel();
            var end = g.DefineLabel();


            ctx.ReaderLoader();
            g.Call(MethodInfos.Utf8JsonReaderTokenType.Value);
            g.LoadInteger((int)JsonTokenType.Number);
            g.IfNotEqualGoto(parseAsString);

            underlyingTypeHandler.Deserialize(ctx);
            g.Goto(end);

            g.MarkLabel(parseAsString);

            var str = g.DeclareLocal(typeof(ReadOnlySpan<byte>));

            //var str = ProxyHelperFunctions.GetStringAsReadOnlySpan(ref reader);
            ctx.ReaderLoader();
            g.Call(MethodInfos.HelperGetStringAsReadOnlySpan);
            g.StoreLocal(str);

            // switcher( ref str)
            g.LoadLocalAddress(str);
            g.Call(enumEntry.DeserializeSwitcher);

            //conv u8 to
            switch (Marshal.SizeOf(underlyingType))
            {
                case 1:
                    g.Emit(OpCodes.Conv_I1);
                    break;
                case 2:
                    g.Emit(OpCodes.Conv_I2);
                    break;
                case 4:
                    g.Emit(OpCodes.Conv_I4);
                    break;
            }

            g.MarkLabel(end);
        }

        private static MethodBuilder BuildDeserializeSwitcher(EnumEntry[] entries, Type type, Type underlyingType, ProxyAssemblyContext proxyAssemblyContext)
        {
            var groups = entries.GroupBy(s => s.StringHash).Select(s => new
                                                                        {
                                                                            s.Key,
                                                                            Entries = s.ToArray()
                                                                        }).OrderBy(s => s.Key).ToArray();

            var switcher = proxyAssemblyContext.StaticFields.DeclareMethod("Switcher" + type.Name, typeof(EnumSwitcherDelegate), typeof(ulong), new []{ MethodInfos.ReadOnlySpanByteRef });
            var gen = switcher.MethodBuilder.GetILGenerator();

            var codeLocal = gen.DeclareLocal(typeof(ulong));

            gen.LoadArgument(0);
            gen.Call(MethodInfos.HelperGetFastHashCodeRef);
            gen.StoreLocal(codeLocal);

            TreeSwitchBuilder.GenerateTree(gen, groups,
                                           (g, entry, label) => TreeSwitchBuilder.ULongGoIfGreater(g, entry.Key, label, () => g.LoadLocal(codeLocal)),
                                           (g, entry, label) => TreeSwitchBuilder.ULongGoIfNotEqual(g, entry.Key, label, () => g.LoadLocal(codeLocal)),
                                           (g, entry, returnTrue, returnFalse) =>
                                           {
                                               foreach (var val in entry.Entries)
                                               {
                                                   var next = g.DefineLabel();

                                                   // compare type names
                                                   g.LoadArgument(0);
                                                   g.LoadStaticFieldAddress(val.Utf8Name.FieldBuilder);
                                                   g.Call(MethodInfos.JsonEncodedTextToReadOnlySpan);
                                                   g.Call(MethodInfos.HelperSequenceEquals);

                                                   g.IfFalseGoto(next);

                                                   // create object
                                                   g.LoadInt64(val.ULongValue);
                                                   g.Goto(returnTrue);

                                                   g.MarkLabel(next);
                                               }
                                               g.Goto(returnFalse);
                                           },
                                           g => { },
                                           g => FallbackParsing(g, type));    
            gen.Return();

            return switcher.MethodBuilder;
        }

        private static void FallbackParsing(ILGenerator g, Type type)
        {
            var parseEnumGeneric = MethodInfos.HelperParseEnum.Value.MakeGenericMethod(type);
            g.LoadArgument(0);
            g.Call(parseEnumGeneric);
            g.Emit(OpCodes.Conv_U8);
        }

        sealed class EnumEntry
        {
            public ulong ULongValue { get; set; }

            public ulong StringHash { get; set; }

            public StaticFieldDeclaration Utf8Name { get; set; }
        }
    }

    public class PropertyTypeHandlerEnumCacheEntry
    {
        public MethodBuilder SerializeSwitcher { get; set; }

        public MethodBuilder DeserializeSwitcher { get; set; }
    }
}
