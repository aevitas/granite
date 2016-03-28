using System.Runtime.CompilerServices;

namespace Granite.Core.Interfaces
{
    public interface IAwaitable<out T> : INotifyCompletion
    {
        bool IsCompleted { get; }

        T GetAwaiter();

        void GetResult();
    }
}
