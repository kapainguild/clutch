
namespace Clutch.Configuration
{
    public interface IConfigOptionDeclaration
    {
        string Name { get; }

        bool IsPassThrough { get; }
    }
}
