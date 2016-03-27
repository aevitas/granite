namespace Granite.Core.Internal
{
    public interface IPool<T>
    {
        T Take();

        void Return(T item);
    }
}
