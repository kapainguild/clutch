using System.Collections.Generic;
using Clutch.Configuration;
using Clutch.Helpers;

namespace Clutch
{
    internal static class PropertyApi
    {
        public static T UsePropertyAccessMode<T>(this T propertyBuilder, PropertyAccessMode mode) where T : IBaseApi =>
            propertyBuilder.SetOption(ConfigOptionDeclarations.UsePropertyAccessMode, mode);

        public static T UsePropertySetterMode<T>(this T propertyBuilder, PropertySetterMode mode) where T : IBaseApi =>
            propertyBuilder.SetOption(ConfigOptionDeclarations.UsePropertySetterMode, mode);

        public static T UseDefaultValueHandling<T>(this T propertyBuilder, DefaultValueHandling handling) where T : IBaseApi =>
            propertyBuilder.SetOption(ConfigOptionDeclarations.UseDefaultValueHandling, handling);

        public static T HasField<T>(this T propertyBuilder, string value) where T : IBaseApi =>
            propertyBuilder.SetOption(ConfigOptionDeclarations.FieldName, value);

        public static T HasDefaultValue<T, TValue>(this T propertyBuilder, TValue value) where T : IBaseApi =>
            propertyBuilder.SetOption(ConfigOptionDeclarations.HasDefaultValue, value);

        public static T HasDefaultValue<T>(this T propertyBuilder, CollectionDefaultValue value) where T : IBaseApi =>
            propertyBuilder.SetOption(ConfigOptionDeclarations.HasCollectionDefaultValue, value);
    }
}
