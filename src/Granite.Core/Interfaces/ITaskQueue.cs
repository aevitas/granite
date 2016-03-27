using System;
using System.Threading;

namespace Granite.Core.Interfaces
{
    public interface ITaskQueue : IDisposable
    {
        /// <summary>
        ///     Enqueues the specified action supplied with the specified state onto the task
        ///     queue.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        bool Enqueue(Action<object, CancellationToken> action, object state);
    }
}