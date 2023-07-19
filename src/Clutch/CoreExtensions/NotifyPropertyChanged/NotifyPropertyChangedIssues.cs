using System.ComponentModel;
using Clutch.Configuration.Issues;

namespace Clutch.CoreExtensions.NotifyPropertyChanged
{
    public static class NotifyPropertyChangedIssues
    {
        public static readonly IssueDeclaration<IssueSourceType, EmptyIssueArgs> RaisePropertyChangedNotFound =
            IssueDeclaration.Error<IssueSourceType, EmptyIssueArgs>((source, args) => $"Instance method 'void {NotifyPropertyChangedExtension.RaisePropertyChangedMethodName}(string)' not found on {source}");

        public static readonly IssueDeclaration<IssueSourceType, EmptyIssueArgs> ContainsPropertyChangedEvent =
            IssueDeclaration.Error<IssueSourceType, EmptyIssueArgs>((source, args) => $"Type {source} should not contain '{nameof(INotifyPropertyChanged.PropertyChanged)}' event");
    }
}
