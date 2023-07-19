using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Clutch.Configuration;
using Clutch.Configuration.Issues;
using Clutch.Helpers;

namespace Clutch.Building
{
    internal static class TypeGraph
    {
        public static TypeGraphBuildContext BuildGraph(Dictionary<Type, BaseTypeBuilder> types, ClutchContextBuilder contextBuilder)
        {
            var ctx = new TypeGraphBuildContext
                          {
                              TypesDictionary = types.Values.ToDictionary(s => s.Type, s => new TypeGraphNode { Type = s.Type, IsDeclaredInContext = true, Builder = s }),
                              Builder = contextBuilder,
                              PropertyTypeHandlerFactory = new PropertyTypeHandlerFactory()
                          };

            ctx.Types = ctx.TypesDictionary.Values.ToList();

            CheckForSingleCallOnProperty(ctx.Builder.GetAnyEntityTypeOrDefault()?.GetAnyPropertyOrDefault());

            // build hierarchy (bases and derived)
            foreach (var type in ctx.Types)
            {
                var node = ProcessType(type.Type, ctx);
                // set derived entities
                node.BaseNodes.ForEach(s => ctx.TypesDictionary[s.Type].DerivedNodes.Add(node));

                if (!node.Type.IsPublic)
                    ctx.AddInaccessibleType(node.Type);
            }

            // second stage: process properties
            // ordering helps implicitly to process properties of base types first
            var orderedByHierarchy = ctx.Types.OrderBy(s => s.BaseNodes.Count);
            foreach (var node in orderedByHierarchy)
            {
                ProcessProperties(node, ctx);
            }

            Dictionary<string, TypeGraphNode> names = new Dictionary<string, TypeGraphNode>();

            // define contructors
            foreach (var node in ctx.Types)
            {
                if (node.Type.IsInterface)
                {
                    node.BaseType = typeof(object);
                    node.BaseTypeConstructor = MethodInfos.CtorOfObject;
                }
                else
                {
                    node.BaseType = node.Type;
                    node.BaseTypeConstructor = TypeInfoHelper.GetParameterlessConstructor(node.Type);

                    if (node.BaseTypeConstructor == null)
                    {
                        node.Issue(CoreIssues.ParameterlessConstructorNotFound, EmptyIssueArgs.Instance);
                    }
                }

                node.Discriminator = node.Type.Name;
                if (names.TryGetValue(node.Discriminator, out var collision))
                    node.Issue(CoreIssues.TypeNameCollisionIsNotSupported, (node.Type.FullName, collision.Type.FullName));
                else
                    names[node.Discriminator] = node;
            }

            BuildExtensions(ctx);

            return ctx;
        }

        private static void BuildExtensions(TypeGraphBuildContext ctx)
        {
            var extensions = ctx.Builder.GetExtensions().ToList();
            foreach (var extension in extensions)
            {
                extension.BuildTypeGraph(ctx);
            }
        }

        private static TypeGraphNode ProcessType(Type type, TypeGraphBuildContext ctx)
        {
            var node = GetOrCreateNode(type, ctx);

            if (node.IsProcessed)
                return node;
            node.IsProcessed = true;

            var bases = new[] { type.BaseType }.Concat(type.GetInterfaces()).Where(s => s != null).ToList();
            // recursive processing
            var processed = bases.Select(s => ProcessType(s, ctx)).ToList();

            // check for double inheritance and find main path
            var sortedByLength = processed.OrderByDescending(s => s.BaseNodesWithSelf.Count).ToList();
            var longestPath = sortedByLength.FirstOrDefault();
            if (longestPath?.BaseNodesWithSelf.Count > 0)
            {
                var other = sortedByLength.Skip(1);
                var multiInherited = other.Where(s => s.BaseNodesWithSelf.Any(r => !longestPath.BaseNodesWithSelf.Contains(r))).ToList();

                if (multiInherited.Any())
                {
                    node.Issue(CoreIssues.MultiInheritanceIsNotSupported, 
                                                              (longestPath.BaseNodesWithSelf.First().Type, multiInherited.First().BaseNodesWithSelf.First().Type));
                }
                node.BaseNodes.AddRange(longestPath.BaseNodesWithSelf);
                node.BaseNodesWithSelf.AddRange(longestPath.BaseNodesWithSelf);
            }

            if (type.IsSealed)
                node.Issue(CoreIssues.SealedClassesAreNotSupported, EmptyIssueArgs.Instance);

            if (node.IsDeclaredInContext)
            {
                node.BaseNodesWithSelf.Insert(0, node);
                var baseEntity = node.BaseNodes.FirstOrDefault();

                CheckMonotonicInheritance(node, baseEntity);

                MergeEntityOptions(node, baseEntity, ctx);
            }

            return node;
        }

        private static void CheckMonotonicInheritance(TypeGraphNode node, TypeGraphNode baseNode)
        {
            if (baseNode != null && node.Builder.BuilderName != baseNode.Builder.BuilderName)
                node.Issue(CoreIssues.CannotDeriveFromOtherTypeCharacter, (node.Builder.BuilderName, baseNode.Builder.BuilderName, baseNode.Type));
        }

        private static void MergeEntityOptions(TypeGraphNode node, TypeGraphNode baseEntity, TypeGraphBuildContext ctx)
        {
            // get base type options first
            if (baseEntity != null)
                node.Options.Merge(node.Options);

            // get any entity options
            MergeAnyEntityPropertyOptions(node.Options, ctx.Builder.GetAnyEntityTypeOrDefault());
        }

        private static void ProcessProperties(TypeGraphNode node, TypeGraphBuildContext ctx)
        {
            TypeGraphNode baseEntity = node.BaseNodes.FirstOrDefault();
            // collect runtime properties
            var rawProperties = node.Type.GetProperties().ToDictionary(s => s.Name);
            if (node.Type.IsInterface)
            {
                // for interfaces we need to collect base interfaces properties
                var allBaseProperties = node.Type.GetInterfaces().SelectMany(s => s.GetProperties());
                foreach (var property in allBaseProperties)
                {
                    // collision by name
                    if (rawProperties.TryGetValue(property.Name, out var alreadyProperty))
                    {
                        if (property.PropertyType != alreadyProperty.PropertyType)
                        {
                            // collision by name and signature
                            // We at the moment do not support that. Technically this is possible but will add extra complexity.
                            // From other hand this indicates bad design.
                            node.Issue(CoreIssues.PropertyCollisionBySignatureIsNotSupported, 
                                                                  ($"{alreadyProperty.PropertyType.Name} {alreadyProperty.DeclaringType?.Name}.{property.Name}",
                                                                      $"{property.PropertyType.Name} {property.DeclaringType?.Name}.{property.Name}"));
                        }
                        else
                        {
                             // this case we support. Just update current node property to have correct PropertyInfo
                            rawProperties[property.Name] = property;
                        }
                    }
                    else
                        rawProperties.Add(property.Name, property);
                }
            }
            
            CheckForSingleCallOnProperty(node.Builder.GetAnyPropertyOrDefault(false));
            CheckForSingleCallOnProperty(node.Builder.GetAnyPropertyOrDefault(true));

            var builders = node.Builder.GetProperties();
            var properties = rawProperties.Select(s =>
            {
                if (builders.TryGetValue(s.Key, out var builder))
                    return CreatePropertyNode(s.Value, builder, ctx); // add builder from current type
                else 
                    return CreatePropertyNode(s.Value, node.Builder.CreateGenericPropertyBuilder(s.Value.PropertyType, s.Key, false), ctx); //create builder if it was not specified by user
            }).ToDictionary(s => s.Builder.PropertyName);

            
            foreach (var builder in builders.Values)
            {
                if (!rawProperties.ContainsKey(builder.PropertyName))
                    throw new ClutchInternalErrorException($"Property Builder '{builder.PropertyName}' exists with no corresponding property on Type '{node.Type.Name}'");
            }

            var allProperties = properties.Values.ToList();

            // merge builders
            foreach (var typeGraphProperty in allProperties)
            {
                //merge properties from base type or any property
                MergePropertyOptions(typeGraphProperty, node, baseEntity, ctx);
            }

            node.MembersInfo = new MembersInfo(node);

            if (node.Type.IsInterface)
                allProperties.ForEach(p => PrepareAccessModeForInterfaceOrAbstractProperty(p, node));
            else
                allProperties.ForEach(p => PrepareAccessModeForClass(p, node));

            // check double reference to the same field
            var propertyAndField = allProperties.Select(s => new { Property = s, FieldName = s.NewBackingField ?? s.BackingField?.Name }).Where(s => s.FieldName != null);
            var groupedByField = propertyAndField.GroupBy(s => s.FieldName).Where(s => s.Count() > 1).ToList();
            groupedByField.ForEach(g => 
                                       g.First().Property.Issue(CoreIssues.FieldIsReferencedMoreThanOnce, 
                                                                (g.Skip(1).First().Property.Name, g.Key), 
                                                                g.First().Property.Options.Get(ConfigOptionDeclarations.FieldName)?.CallerInfo));

            allProperties.ForEach(p => ProcessPropertyAsGeneric(p, ctx));

            //check for any private field
            if (allProperties.Where(s => s.BackingField != null).Any(s => s.BackingField.IsPrivate || s.BackingField.IsAssembly))
                ctx.AddInaccessibleType(node.Type);

            node.Properties = properties;
        }

        private static TypeGraphProperty CreatePropertyNode(PropertyInfo propertyInfo, PropertyBuilder builder, TypeGraphBuildContext ctx)
        {
            var graphProperty = new TypeGraphProperty(propertyInfo, builder);
            var initializer = new GenericPropertyInitializer(graphProperty, ctx);
            builder.CallGenericProcessor(initializer);
            return graphProperty;
        }

        private static void ProcessPropertyAsGeneric(TypeGraphProperty property, TypeGraphBuildContext ctx)
        {
            var genericProcessor = new GenericPropertyBuilder(property, ctx);
            property.Builder.CallGenericProcessor(genericProcessor);
        }

        private static void PrepareAccessModeForClass(TypeGraphProperty property, TypeGraphNode node)
        {
            // complexity: 4 modes, abstract/implemented, name specified/not, contains getter/setter

            var getter = property.PropertyInfo.GetMethod;
            var setter = property.PropertyInfo.SetMethod;
            var anyMethod = getter ?? setter;

            if (anyMethod.IsAbstract)
            {
                PrepareAccessModeForInterfaceOrAbstractProperty(property, node);
            }
            else if (!anyMethod.IsVirtual)
            {
                property.Issue(CoreIssues.PropertyMustBeDeclaredAsVirtual, EmptyIssueArgs.Instance);
            }
            else
            {
                var propertyAccess = property.Options.Get(ConfigOptionDeclarations.UsePropertyAccessMode);
                var mode = propertyAccess?.Value ?? ConfigOptionDeclarations.UsePropertyAccessMode.DefaultValue;
                var fieldRequired = true;
                var setterRequired = true;
                var getterRequired = true;
                switch (mode)
                {
                    case PropertyAccessMode.Field:
                        getterRequired = false;
                        setterRequired = false;
                        break;
                    case PropertyAccessMode.FieldForGetterAndInitializationPropertyForSetter:
                        getterRequired = false;
                        break;
                    case PropertyAccessMode.FieldForGetterPropertyForSetter:
                        getterRequired = false;
                        break;
                    case PropertyAccessMode.Property:
                        fieldRequired = false;
                        break;
                    default:
                        throw new ClutchInternalErrorException($"Unknown access mode '{mode}'");
                }

                if (getterRequired && getter == null)
                    property.Issue(CoreIssues.PropertyMustHaveGetter, mode, propertyAccess?.CallerInfo);

                if (setterRequired && setter == null)
                    property.Issue(CoreIssues.PropertyMustHaveSetter, mode, propertyAccess?.CallerInfo);

                if (fieldRequired)
                {
                    if (property.Options.TryGet(ConfigOptionDeclarations.FieldName, out var fieldName))
                    {
                        var field = node.MembersInfo.GetField(fieldName.Value);
                        if (field == null)
                            property.Issue(CoreIssues.FieldNotFound, fieldName.Value, fieldName.CallerInfo);
                        else
                            property.BackingField = field;
                    }
                    else
                    {
                        var field = node.MembersInfo.FindBackingField(property.Name);
                        if (field == null)
                            property.Issue(CoreIssues.BackingFieldNotFound, EmptyIssueArgs.Instance);
                        else
                            property.BackingField = field;
                    }
                    if (property.BackingField != null)
                        if (property.BackingField.FieldType != property.ReturnType)
                            property.Issue(CoreIssues.PropertyAndFieldTypeMismatch, (property.BackingField.FieldType, property.BackingField.Name));
                }
                else if (property.Options.TryGet(ConfigOptionDeclarations.FieldName, out var fieldNameRedundant))
                    property.Issue(CoreIssues.CallIsRedundantOnProperty, nameof(PropertyApi.HasField), fieldNameRedundant.CallerInfo);

                property.PropertyAccessMode = mode;
            }
        }

        private static void PrepareAccessModeForInterfaceOrAbstractProperty(TypeGraphProperty property, TypeGraphNode node)
        {
            string result = null;
            if (property.Options.TryGet(ConfigOptionDeclarations.UsePropertyAccessMode, out var propertyAccess) &&
                propertyAccess.Value != PropertyAccessMode.Field)
                property.Issue(CoreIssues.OnlyFieldAccessModeIsAllowedOnInterfaceOrAbstractProperty, propertyAccess.Value, propertyAccess.CallerInfo);

            property.PropertyAccessMode = PropertyAccessMode.Field;

            if (property.Options.TryGet(ConfigOptionDeclarations.FieldName, out var fieldName))
            {
                var requestedFieldName = fieldName.Value;
                if (node.MembersInfo.Contains(requestedFieldName, out var desc))
                {
                    if (node.Type.IsInterface)
                        property.Issue(CoreIssues.MemberWithNameIsAlreadyDeclared, (requestedFieldName, desc), fieldName.CallerInfo);
                    else
                    {
                        // this is abstract property
                        var field = node.MembersInfo.GetField(requestedFieldName);
                        if (field == null)
                            property.Issue(CoreIssues.MemberWithNameIsAlreadyDeclared, (requestedFieldName, desc), fieldName.CallerInfo);
                        else
                        {
                            if (field.FieldType == property.ReturnType)
                                property.BackingField = field;
                            else
                                property.Issue(CoreIssues.PropertyAndFieldTypeMismatch, (field.FieldType, field.Name), fieldName.CallerInfo);
                        }
                    }
                }
                else
                    result = requestedFieldName;
            }
            else
                result = node.MembersInfo.GenerateUniqueFieldName(property.Name);

            if (result != null)
            {
                property.NewBackingField = result;
                node.MembersInfo.AddNewMember(result, MemberDeclarationSource.BackingField);
            }
        }

        private static void MergePropertyOptions(TypeGraphProperty typeGraphProperty, TypeGraphNode node, TypeGraphNode parent, TypeGraphBuildContext ctx)
        {
            var options = typeGraphProperty.Options;

            CheckForSingleCallOnProperty(typeGraphProperty.Builder);

            if (typeGraphProperty.PropertyInfo.DeclaringType == node.Type)
                MergeAnyEntityPropertyOptions(options, node.Builder.GetAnyPropertyOrDefault(false));
            MergeAnyEntityPropertyOptions(options, node.Builder.GetAnyPropertyOrDefault(true));

            // add or merge builders from parent type
            if (parent != null)
            {
                if (parent.Properties.TryGetValue(typeGraphProperty.Name, out TypeGraphProperty parentProperty))
                    options.Merge(parentProperty.Options);
            }
            MergeAnyEntityPropertyOptions(options, ctx.Builder.GetAnyEntityTypeOrDefault()?.GetAnyPropertyOrDefault());
        }

        private static void MergeAnyEntityPropertyOptions<TParent, TSource>(ConfigOptionBag options, BaseBuilder<TParent, TSource> anyOrDefault) where TParent : IInternalBuilder
        {
            if (anyOrDefault != null)
                options.Merge(anyOrDefault.ToInternal().Options);
        }

        private static void CheckForSingleCallOnProperty(IInternalBuilder<IssueSourceProperty> builder)
        {
            if (builder != null)
            {
                foreach (var option in builder.Options.GetAll())
                {
                    var (optionName, callerInfo, value) = option.GetLastCallIfMoreThanOnce();
                    if (optionName != null)
                        builder.IssueSourceContext.Issue(CoreIssues.OptionIsSetMoreThanOnceOnProperty, (optionName, value), callerInfo);
                }
            }
        }

        private static TypeGraphNode GetOrCreateNode(Type type, TypeGraphBuildContext ctx)
        {
            if (!ctx.TypesDictionary.TryGetValue(type, out TypeGraphNode node))
            {
                node = new TypeGraphNode { Type = type };
                ctx.TypesDictionary.Add(type, node);
            }

            return node;
        }
    }
}
