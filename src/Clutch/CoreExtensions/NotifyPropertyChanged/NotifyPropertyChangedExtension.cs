using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Clutch.Building;
using Clutch.Configuration;
using Clutch.Configuration.Issues;
using Clutch.Extensibility;
using Clutch.Helpers;

namespace Clutch.CoreExtensions.NotifyPropertyChanged
{
    class NotifyPropertyChangedExtension : IExtension
    {
        public const string RaisePropertyChangedMethodName = "RaisePropertyChanged";

        public static ConfigOptionDeclaration<NotifyPropertyChangedBehavior> NotifyPropertyChangedExtensionBehavior { get; } = 
            new ConfigOptionDeclaration<NotifyPropertyChangedBehavior>(NotifyPropertyChangedBehavior.ImplementOnAllEntities);

        public static ConfigOptionDeclaration<bool> NotifyPropertyChangedEnabled { get; } = new ConfigOptionDeclaration<bool>(true);

        public void BuildTypeGraph(TypeGraphBuildContext ctx)
        {
            var behaviorCall = ctx.Options.Get(NotifyPropertyChangedExtensionBehavior);

            var allSet = ctx.Types.SelectMany(s => s.Properties.Values, (ent, pro) => new { ent, pro, val = pro.Options.Get(NotifyPropertyChangedEnabled) }).
                Where(s => s.val != null).ToList();

            if (behaviorCall == null && allSet.Any())
            {
                ctx.Issue(CoreIssues.ExtensionIsNotEnabledExplicitely, 
                          (nameof(NotifyPropertyChangedExtensionMethods.EnableNotifyPropertyChanged), 
                           nameof(NotifyPropertyChangedExtensionMethods.UseNotifyPropertyChanged)));
            }

            var behavior = behaviorCall?.Value ?? NotifyPropertyChangedExtensionBehavior.DefaultValue;

            if (behavior == NotifyPropertyChangedBehavior.Disable)
            {
                if (allSet.Any())
                    ctx.Issue(CoreIssues.ExtensionIsUsedWhileDisabled,
                              (nameof(NotifyPropertyChangedExtensionMethods.EnableNotifyPropertyChanged),
                               nameof(NotifyPropertyChangedExtensionMethods.UseNotifyPropertyChanged)));
            }
            else if (behavior == NotifyPropertyChangedBehavior.ImplementOnAllEntities)
            {
                allSet.Where(s => s.val.Value).ForEach(s => s.pro.Issue(CoreIssues.CallIsRedundantOnProperty, nameof(NotifyPropertyChangedExtensionMethods.EnableNotifyPropertyChanged), s.val.CallerInfo));
                ctx.Types.ForEach(s => BuildEntity(s, true));
                InstallInfluencers(ctx);
            }
            else if (behavior == NotifyPropertyChangedBehavior.ImplementOnlyOnEntitiesWithEnabledProperties)
            {
                allSet.Where(s => !s.val.Value).ForEach(s => s.pro.Issue(CoreIssues.CallIsRedundantOnProperty, nameof(NotifyPropertyChangedExtensionMethods.EnableNotifyPropertyChanged), s.val.CallerInfo));
                allSet.Where(s => s.val.Value).Select(s => s.ent).Distinct().ForEach(s => BuildEntity(s, false));
                InstallInfluencers(ctx);
            }
            else
                throw new ClutchInternalErrorException($"Unexpected value of {nameof(NotifyPropertyChangedExtensionBehavior)} ({behaviorCall?.Value})");
        }

        private static void InstallInfluencers(TypeGraphBuildContext ctx)
        {
            var generator = new NotifyPropertyChangedGenerator();
            ctx.ProxyInfluencers.AddTypeInfluencer(generator);
            ctx.ProxyInfluencers.AddPropertySetterInfluencer(generator);
        }

        private void BuildEntity(TypeGraphNode node, bool defaultValue)
        {
            var info = GetInfo(node);
            if (info != null)
            {
                node.Extensions.SetData<NotifyPropertyChangedExtension>(info);

                foreach (var property in node.Properties.Values)
                {
                    var calculated = defaultValue;
                    var set = property.Options.Get(NotifyPropertyChangedEnabled);
                    if (set != null)
                        calculated = set.Value;

                    if (calculated)
                        property.Extensions.SetData<NotifyPropertyChangedExtension>(true);
                }
            }
        }

        private NotifyPropertyChangedEntityInfo GetInfo(TypeGraphNode node)
        {
            NotifyPropertyChangedEntityInfo result = new NotifyPropertyChangedEntityInfo();
            var type = node.Type;

            if (type.GetInterfaces().Contains(typeof(INotifyPropertyChanged)))
            {
                if (!type.IsInterface)
                {
                    // the class already implemented interface
                    var raiser = type.GetMethod(RaisePropertyChangedMethodName, 
                                                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, 
                                                null, CallingConventions.Any, new []{typeof(string)}, null);
                    if (raiser == null)
                    {
                        node.Issue(NotifyPropertyChangedIssues.RaisePropertyChangedNotFound, EmptyIssueArgs.Instance);
                        return null;
                    }

                    result.RaisePropertyChangedMethodInfo = raiser;
                }
            }
            else
            {
                if (type.GetEvent(nameof(INotifyPropertyChanged.PropertyChanged), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static) != null)
                {
                    node.Issue(NotifyPropertyChangedIssues.ContainsPropertyChangedEvent, EmptyIssueArgs.Instance);
                    return null;
                }

                result.AddInterface = true;
            }

            return result;
        }
    }
}
