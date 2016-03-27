namespace Granite.Core.Interfaces
{
    public interface IAwaiter<T>
    {
        bool IsCompleted { get; }

        T GetAwaiter();

        void GetResult();
    }
}
