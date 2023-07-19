using System;
using Clutch.Configuration.Issues;

namespace Clutch
{
    public static class CoreIssues
    {
        public static readonly IssueDeclaration<IssueSourceType, string> PropertyNotFound =
            IssueDeclaration.Error<IssueSourceType, string>((source, args) => 
            $"Property '{args}' not found on type '{source.Type}'");

        public static readonly IssueDeclaration<IssueSourceType, (Type type1, Type type2)> MultiInheritanceIsNotSupported =
            IssueDeclaration.Error<IssueSourceType, (Type type1, Type type2)>((source, args) => 
            $"Multiinheritance is not supported by Clutch. '{source}' has at least to parents: '{args.type1}' and '{args.type2}'");

        public static readonly IssueDeclaration<IssueSourceType, EmptyIssueArgs> ParameterlessConstructorNotFound =
            IssueDeclaration.Error<IssueSourceType, EmptyIssueArgs>((source, args) => 
            $"Parameterless constructor not found on '{source}'");

        public static readonly IssueDeclaration<IssueSourceType, (string property1, string property2)> PropertyCollisionBySignatureIsNotSupported =
            IssueDeclaration.Error<IssueSourceType, (string property1, string property2)>((source, args) => 
            $"Property collision by signature is not supported by Clutch. Property '{args.property1}' collide with '{args.property2}'");

        public static readonly IssueDeclaration<IssueSourceType, (string typeName1, string typeName2)> TypeNameCollisionIsNotSupported =
            IssueDeclaration.Error<IssueSourceType, (string typeName1, string typeName2)>((source, args) =>
            $"Type name collision is not supported by Clutch. Type '{args.typeName1}' collide with '{args.typeName2}'");

        public static readonly IssueDeclaration<IssueSourceProperty, EmptyIssueArgs> PropertyMustBeDeclaredAsVirtual =
            IssueDeclaration.Error<IssueSourceProperty, EmptyIssueArgs>((source, args) => 
            $"Property '{source}' must be declared as virtual");

        public static readonly IssueDeclaration<IssueSourceProperty, PropertyAccessMode> PropertyMustHaveGetter =
            IssueDeclaration.Error<IssueSourceProperty, PropertyAccessMode>((source, args) => 
            $"Property '{source}' must have getter as access mode is '{args}'");

        public static readonly IssueDeclaration<IssueSourceProperty, PropertyAccessMode> PropertyMustHaveSetter =
            IssueDeclaration.Error<IssueSourceProperty, PropertyAccessMode>((source, args) => 
            $"Property '{source}' must have getter as access mode is '{args}'");

        public static readonly IssueDeclaration<IssueSourceType, EmptyIssueArgs> SealedClassesAreNotSupported =
            IssueDeclaration.Error<IssueSourceType, EmptyIssueArgs>((source, args) =>
            $"Class '{source}' is 'sealed' and not supported");
        

        public static readonly IssueDeclaration<IssueSourceProperty, (string property2, string field)> FieldIsReferencedMoreThanOnce =
            IssueDeclaration.Warning<IssueSourceProperty, (string property2, string field)>((source, args) => 
            $"Field '{args.field}' is referenced by at least two properties ('{source.PropertyName}' and '{args.property2}')");

        public static readonly IssueDeclaration<IssueSourceProperty, string> FieldNotFound =
            IssueDeclaration.Error<IssueSourceProperty, string>((source, args) => 
            $"Field '{args}' not found");

        public static readonly IssueDeclaration<IssueSourceProperty, EmptyIssueArgs> BackingFieldNotFound =
            IssueDeclaration.Error<IssueSourceProperty, EmptyIssueArgs>((source, args) => 
            $"Backing field not found for property '{source}'");

        public static readonly IssueDeclaration<IssueSourceProperty, PropertyAccessMode> OnlyFieldAccessModeIsAllowedOnInterfaceOrAbstractProperty =
            IssueDeclaration.Warning<IssueSourceProperty, PropertyAccessMode>((source, args) => 
            $"Only '{nameof(PropertyAccessMode.Field)}' property access is allowed on interface or abstract property. '{args}' is used on property '{source}' and it will be ignored.");

        public static readonly IssueDeclaration<IssueSourceProperty, (string memberName, string declaration)> MemberWithNameIsAlreadyDeclared =
            IssueDeclaration.Error<IssueSourceProperty, (string memberName, string declaration)>((source, args) => 
            $"Member '{args.memberName}' is already declared as {args.declaration} on {source.Type.Name}");

        public static readonly IssueDeclaration<IssueSourceProperty, (Type fieldType, string fieldName)> PropertyAndFieldTypeMismatch =
            IssueDeclaration.Error<IssueSourceProperty, (Type fieldType, string fieldName)>((source, args) => 
            $"Field type '{args.fieldType}' of '{args.fieldName}' does not match type of '{source}' property");

        public static readonly IssueDeclaration<IssueSourceProperty, (string optionName, object lastValue)> OptionIsSetMoreThanOnceOnProperty =
            IssueDeclaration.Warning<IssueSourceProperty, (string optionName, object lastValue)>((source, args) => 
            $"Option '{args.optionName}' is set more than once on property '{source}'. Last value '{args.lastValue}' will be considered");

        public static readonly IssueDeclaration<IssueSourceType, string> OptionIsSetMoreThanOnceOnType =
            IssueDeclaration.Warning<IssueSourceType, string>((source, args) => 
            $"Option '{args}' is set more than once on type '{source}'");

        public static readonly IssueDeclaration<IssueSource, string> OptionIsSetMoreThanOnceOnContext =
            IssueDeclaration.Warning<IssueSource, string>((source, args) => 
            $"Option '{args}' is set more than once on '{source}'");

        public static readonly IssueDeclaration<IssueSource, (string extensionOptionCall, string enableExtensionMethod)> ExtensionIsNotEnabledExplicitely =
            IssueDeclaration.Warning<IssueSource, (string extensionOptionCall, string enableExtensionMethod)>((source, args) =>
            $"{args.extensionOptionCall} is used on a property but extention is not enabled. Please call '{args.enableExtensionMethod}' on '{source}'");

        public static readonly IssueDeclaration<IssueSource, (string extensionOptionCall, string enableExtensionMethod)> ExtensionIsUsedWhileDisabled =
            IssueDeclaration.Warning<IssueSource, (string extensionOptionCall, string enableExtensionMethod)>((source, args) =>
            $"{args.extensionOptionCall} is used on a property but extention was disabled by '{args.enableExtensionMethod}' call");

        public static readonly IssueDeclaration<IssueSourceProperty, string> CallIsRedundantOnProperty =
            IssueDeclaration.Info<IssueSourceProperty, string>((source, args) =>
            $"The call '{args}' on property '{source}' is redundant");

        public static readonly IssueDeclaration<IssueSourceProperty, string> DefaultValueIsRedundant =
            IssueDeclaration.Info<IssueSourceProperty, string>((source, args) =>
            $"Specifing default value '{args}' on property '{source}' is redundant as it equals to system default (default(T))");

        public static readonly IssueDeclaration<IssueSourceProperty, Type> TypeIsNotSupported =
            IssueDeclaration.Error<IssueSourceProperty, Type>((source, args) =>
                                                                   $"Type '{args}' cannot be handled by Clutch (Property '{source}')");

        public static readonly IssueDeclaration<IssueSourceType, string> CannotChangeTypeCharacter =
            IssueDeclaration.Error<IssueSourceType, string>((source, args) =>
                                                                  $"Type '{source}' is already declared as '{args}'. It cannot be changed");

        public static readonly IssueDeclaration<IssueSourceType, (string characterCurrent, string characterBase, Type baseType)> CannotDeriveFromOtherTypeCharacter =
            IssueDeclaration.Error<IssueSourceType, (string characterCurrent, string characterBase, Type baseType)>((source, args) =>
                                                                $"Type '{source}:{args.characterCurrent}' is derived from '{args.baseType}:{args.characterBase}'. '{args.characterCurrent}' cannot derive from '{args.characterBase}'");

    }
}
