using System.Threading.Tasks;

namespace bridgefield.FoundationalBits
{
    // ReSharper disable once TypeParameterCanBeVariant
    public interface IHandle<T>
    {
        void Handle(T message);
    }

    // ReSharper disable once TypeParameterCanBeVariant
    public interface IHandleAsync<T>
    {
        Task Handle(T message);
    }
}