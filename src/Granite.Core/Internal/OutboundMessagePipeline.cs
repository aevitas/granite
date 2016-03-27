using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Granite.Core.Interfaces;
using System.Runtime.InteropServices;
using Granite.Core.Sockets;

namespace Granite.Core.Internal
{
    internal class OutboundMessagePipeline : IMessagePipeline
    {
        private readonly IConnection _connection;
        private bool _isRunning;
        private bool _isShuttingDown;
        private TaskCompletionSource<object> _shutdownTcs; 
        private readonly CancellationToken _cancellationToken;
        private readonly ConcurrentQueue<IMessage> _messages = new ConcurrentQueue<IMessage>();
        private static readonly int HeaderSize = Marshal.SizeOf<MessageHeader>();

        public OutboundMessagePipeline(IConnection connection, CancellationToken cancellationToken)
        {
            Requires.NotNull(connection, nameof(connection));

            _connection = connection;
            _cancellationToken = cancellationToken;
        }

        public bool Enqueue(IMessage message)
        {
            Requires.NotNull(message, nameof(message));

            if (IsShuttingDown())
                return false;

            _messages.Enqueue(message);

            if (!_isRunning)
                Start();

            return true;
        }

        public void Start()
        {
            if (_isRunning)
                return;

            Task.Run(SendMessages);

            _isRunning = true;
        }

        private async Task SendMessages()
        {
            while (true)
            {
                if (ShouldBreakProcessing())
                    break;

                IMessage message;
                if (!_messages.TryDequeue(out message))
                    break;

                var buffer = PrepareMessage(message);
                var awaitable = Pools.SocketAwaitable.Take();
                try
                {
                    awaitable.EventArgs.SetBuffer(buffer, 0, buffer.Length);

                    await _connection.Socket.SendAsync(awaitable);
                }
                finally
                {
                    Pools.SocketAwaitable.Return(awaitable);
                }
            }
        }

        private byte[] PrepareMessage(IMessage message)
        {
            byte[] messageBuffer;
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                byte[] correlationBuffer = new byte[Marshal.SizeOf<Guid>()];
                unsafe
                {
                    fixed (byte* b = correlationBuffer)
                        Marshal.StructureToPtr(message.Correlation, (IntPtr)b, false);
                }

                ms.Write(correlationBuffer, 0, correlationBuffer.Length);

                var serialized = Current.Serializer.Serialize(message.Content);

                bw.Write(serialized);

                messageBuffer = ms.ToArray();
            }

            var header = new MessageHeader { Length = messageBuffer.Length };
            var headerBuffer = new byte[HeaderSize];

            unsafe
            {
                fixed (byte* b = headerBuffer)
                    Marshal.StructureToPtr(header, (IntPtr)b, false);
            }

            byte[] buffer = new byte[headerBuffer.Length + messageBuffer.Length];

            Buffer.BlockCopy(headerBuffer, 0, buffer, 0, headerBuffer.Length);
            Buffer.BlockCopy(messageBuffer, 0, buffer, headerBuffer.Length + 1, messageBuffer.Length);

            return buffer;
        }

        public async Task StopAsync(bool finishMessages)
        {
            if (!_isRunning)
                return;

            _shutdownTcs = new TaskCompletionSource<object>();

            Shutdown(finishMessages);

            await _shutdownTcs.Task;
        }

        private bool ShouldBreakProcessing()
        {
            if (_messages.Count <= 0)
                return true;

            return false;
        }

        private bool IsShuttingDown()
        {
            if (_cancellationToken.IsCancellationRequested)
                return true;

            if (_isShuttingDown)
                return true;

            return false;
        }

        private void Shutdown(bool finishMessages)
        {
            _isShuttingDown = true;

            IMessage message;
            if (!finishMessages)
                while (_messages.TryDequeue(out message)) { }

            _shutdownTcs?.SetResult(null);
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            Shutdown(false);
        }

        #endregion
    }
}
