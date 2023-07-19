

namespace Clutch.Configuration
{
    public static class ConfigOptionDeclarations
    {
        // property level
        public static readonly ConfigOptionDeclaration<PropertyAccessMode> UsePropertyAccessMode = 
            new ConfigOptionDeclaration<PropertyAccessMode>(PropertyAccessMode.FieldForGetterAndInitializationPropertyForSetter);

        public static readonly ConfigOptionDeclaration<string> FieldName = new ConfigOptionDeclaration<string>(null);

        public static readonly ConfigOptionDeclaration<PropertySetterMode> UsePropertySetterMode = 
            new ConfigOptionDeclaration<PropertySetterMode>(PropertySetterMode.Set, true);

        public static readonly ConfigOptionDeclaration<DefaultValueHandling> UseDefaultValueHandling = 
            new ConfigOptionDeclaration<DefaultValueHandling>(DefaultValueHandling.Include, true);

        public static readonly ConfigOptionDeclarationOpenType HasDefaultValue = new ConfigOptionDeclarationOpenType();


        public static readonly ConfigOptionDeclaration<CollectionDefaultValue> HasCollectionDefaultValue =
            new ConfigOptionDeclaration<CollectionDefaultValue>(CollectionDefaultValue.Empty, true);

        // type level


        // context level
    }
}
