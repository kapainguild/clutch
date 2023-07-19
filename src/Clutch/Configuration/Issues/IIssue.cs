
namespace Clutch.Configuration.Issues
{
    public interface IIssue
    {
        string Message { get; }

        string DeclarationName { get; }

        IssueSeverity Severity { get; }
    }
}
