using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Granite.Core.Interfaces;

namespace Granite.Core
{
    // Originally written by Janiels - https://github.com/Janiels

    public class TaskQueue : ITaskQueue
    {
        private readonly Action<Task, object> _executeAction;
        private readonly object _lock = new object();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private Task _previousTask = Task.CompletedTask;

        public TaskQueue()
        {
            _executeAction = ExecuteAction;
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            _cts?.Dispose();
        }

        #endregion

        public bool Enqueue(Action<object, CancellationToken> action, object state)
        {
            return EnqueueInternal(new StatefulAction(action, state));
        }

        private bool EnqueueInternal(object state)
        {
            lock (_lock)
            {
                // If previous is null, we've been stopped.
                if (_previousTask == null)
                    return false;

                _previousTask.ContinueWith(_executeAction, state);

                return true;
            }
        }

        public Task StopAsync(bool finishQueue)
        {
            Task last;
            lock (_lock)
            {
                last = _previousTask;
                _previousTask = null;
            }

            if (last == null)
                throw new InvalidOperationException("Can not stop a TaskQueue that was already stopped previously!");

            return StopAsyncInternal(last, finishQueue);
        }

        private async Task StopAsyncInternal(Task last, bool finishQueue)
        {
            // If we're not finishing the queue, signal cancellation to all tasks queued through ExecuteAction.
            if (!finishQueue)
                _cts.Cancel();

            try
            {
                await last;
            }
            catch (OperationCanceledException)
            { }

            _cts.Dispose();
        }

        private void ExecuteAction(Task previous, object state)
        {
            Debug.Assert(state is StatefulAction);

            var s = (StatefulAction) state;

            s.Action(s.State, _cts.Token);
        }

        private class StatefulAction
        {
            public StatefulAction(Action<object, CancellationToken> action, object state)
            {
                Action = action;
                State = state;
            }

            public Action<object, CancellationToken> Action { get; }
            public object State { get; }
        }
    }
}