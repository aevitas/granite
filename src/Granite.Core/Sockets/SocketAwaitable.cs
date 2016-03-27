using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Granite.Core.Interfaces;
using Granite.Core.Internal;

namespace Granite.Core.Sockets
{
    public sealed class SocketAwaitable : INotifyCompletion, IDisposable, IAwaiter<SocketAwaitable>
    {
        private bool _isDisposed;
        private readonly Action _emptyContinuation = () => { };
        private Action _continuation;

        public SocketAwaitable(SocketAsyncEventArgs eventArgs)
        {
            Requires.NotNull(eventArgs, nameof(eventArgs));

            EventArgs = eventArgs;

            eventArgs.Completed += (sender, args) =>

            {
                var previous = _continuation ?? Interlocked.CompareExchange(ref _continuation, _emptyContinuation, null);

                previous?.Invoke();
            };
        }

        public SocketAwaitable() : this(new SocketAsyncEventArgs()) { }

        public SocketAsyncEventArgs EventArgs { get; private set; }

        public SocketAwaitable GetAwaiter()
        {
            return this;
        }

        public void GetResult()
        {
        }

        public bool IsCompleted { get; private set; }

        public void CompleteSynchronously()
        {
            IsCompleted = true;
        }

        public void Reset()
        {
            _continuation = null;
            IsCompleted = false;
        }

        #region Implementation of INotifyCompletion

        public void OnCompleted(Action continuation)
        {
            if (_continuation == _emptyContinuation ||
                Interlocked.CompareExchange(ref _continuation, continuation, null) == _emptyContinuation)
            {
                Task.Run(continuation);
            }
        }

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (_isDisposed)
                return;

            EventArgs?.Dispose();
            EventArgs = null;

            _isDisposed = true;
        }

        #endregion
    }
}
