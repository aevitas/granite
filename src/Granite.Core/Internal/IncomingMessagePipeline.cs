using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Granite.Core.Interfaces;
using Granite.Core.Sockets;

namespace Granite.Core.Internal
{
    internal class IncomingMessagePipeline : IMessagePipeline
    {
        private readonly ILog _log = LogManager.GetLogger("inc-msg-p");
        private static readonly int HeaderSize = Marshal.SizeOf<MessageHeader>();

        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<IMessage>> _promises =
            new ConcurrentDictionary<Guid, TaskCompletionSource<IMessage>>();

        private CancellationToken _cancellationToken;
        private readonly IConnection _connection;
        private bool _isRunning;

        public IncomingMessagePipeline(IConnection connection, CancellationToken token)
        {
            Requires.NotNull(connection, nameof(connection));

            _connection = connection;
            _cancellationToken = token;
        }

        private async Task ReceiveAsync()
        {
            var headerBuffer = new byte[HeaderSize];

            SocketAwaitable awaitable = Pools.SocketAwaitable.Take();
            try
            {
                while (!_cancellationToken.IsCancellationRequested && _isRunning)
                {
                    awaitable.EventArgs.SetBuffer(headerBuffer, 0, HeaderSize);

                    if (!await ReceiveChunkAsync(awaitable))
                        break;

                    MessageHeader header;

                    unsafe
                    {
                        fixed (byte* b = awaitable.EventArgs.Buffer)
                        {
                            var h = (MessageHeader*)b;
                            header = *h;
                        }
                    }

                    if (!IsValidHeader(header))
                        continue;

                    var opCode = header.OpCode;
                    var length = header.Length;
                    var buffer = Pools.BufferManagerInternal.TakeBuffer(length);

                    try
                    {
                        awaitable.EventArgs.SetBuffer(buffer, 0, length);

                        if (!await ReceiveChunkAsync(awaitable))
                            break;

                        var message = await ReifyMessage(opCode, buffer);

                        if (CompletePromise(message.Correlation, message))
                            continue;

                        _connection.OnMessageReceived(message);
                    }
                    finally
                    {
                        Pools.BufferManagerInternal.ReturnBuffer(buffer);
                    }
                }
            }
            finally
            {
                Pools.SocketAwaitable.Return(awaitable);
            }
        }


        private async Task<bool> ReceiveChunkAsync(SocketAwaitable awaitable)
        {
            var error = SocketError.OperationAborted;

            try
            {
                error = await _connection.Socket.ReceiveAsync(awaitable);
            }
            catch (ObjectDisposedException)
            {
                _log.Error("ReceiveChunkAsync failed; socket was disposed!");
            }

            // Something else went wrong - ConnectionReset, OperationAborted, etc.
            if (error != SocketError.Success)
                return false;

            // Graceful shutdown by the remote socket.
            if (awaitable.EventArgs.BytesTransferred == 0)
                return false;

            return true;
        }

        private static Task<Message> ReifyMessage(uint opcode, byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer))
            using (var br = new BinaryReader(ms))
            {
                Guid correlation;
                var guidBuffer = br.ReadBytes(Marshal.SizeOf<Guid>());

                unsafe
                {
                    fixed (byte* b = guidBuffer)
                    {
                        var g = (Guid*)b;
                        correlation = *g;
                    }
                }

                var content = br.ReadString();

                return Task.FromResult(new Message(opcode, content, correlation));
            }
        }

        public Task<IMessage> RegisterPromise(Guid guid, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<IMessage>();
            if (!_promises.TryAdd(guid, tcs))
                throw new ArgumentException(
                    $"Can not register a promise for packet with correlation {guid} - a promise to receive this packet already exists, and hasn't been fulfilled yet.");

            cancellationToken.Register(() =>
            {
                TaskCompletionSource<IMessage> completionSource;
                if (_promises.TryRemove(guid, out completionSource))
                    completionSource.SetCanceled();
            });

            return tcs.Task;
        }

        public bool CompletePromise(Guid guid, IMessage message)
        {
            TaskCompletionSource<IMessage> tcs;
            if (!_promises.TryRemove(guid, out tcs))
                return false;

            tcs.SetResult(message);

            return true;
        }

        public void Start()
        {
            if (_isRunning)
                return;

            Task.Run(ReceiveAsync);

            _isRunning = true;
        }

        public async Task StopAsync(bool finishMessages = true)
        {
            Shutdown();
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            Shutdown();
        }

        #endregion

        private void Shutdown()
        {
            _isRunning = false;
        }

        private static bool IsValidHeader(MessageHeader header)
        {
            return header.Length > 1;
        }
    }
}
