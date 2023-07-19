using Clutch.Building;

namespace Clutch.Extensibility
{
    public interface IExtension
    {
        void BuildTypeGraph(TypeGraphBuildContext ctx);
    }
}
