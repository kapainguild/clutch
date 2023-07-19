
namespace Clutch
{
    interface ITypeGraphPropertyGenericProcessor
    {
        void Process<T>();

        void ProcessCollection<T, TElement>();
    }
}
