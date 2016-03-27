using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Granite.Core.Interfaces;
using Granite.Core.Internal;
using Granite.Core.Sockets;

namespace Granite.Core
{
    public abstract class TcpConnection : IConnection
    {
        private IncomingMessagePipeline _incomingPipeline;
        private OutboundMessagePipeline _outboundPipeline;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        protected IPEndPoint RemoteEndPoint { get; private set; }

        public ISocket Socket { get; protected set; }

        public Action OnConnected { get; }

        protected TcpConnection(Action onConnectedCallback = null)
        {
            Socket = new TcpSocket(AddressFamily.InterNetwork)
            {
                NoDelay = true
            };

            OnConnected = onConnectedCallback;
        }

        public virtual async Task ConnectAsync(string host, int port, int timeoutMilliseconds = 10000)
        {
            Requires.NotNullOrWhiteSpace(host, nameof(host));

            if (port <= 0)
                throw new ArgumentOutOfRangeException("Port can not be zero or negative.");
            
            var awaitable = Pools.SocketAwaitable.Take();
            awaitable.EventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Parse(host), port);

            try
            {
                var error = await Socket.ConnectAsync(awaitable);

                if (error != SocketError.Success)
                    throw new CanNotConnectException("Could not connect to the specified host!", host, port);
            }
            finally
            {
                Pools.SocketAwaitable.Return(awaitable);
            }

            _incomingPipeline = new IncomingMessagePipeline(this, _cts.Token);
            _outboundPipeline = new OutboundMessagePipeline(this, _cts.Token);

            _incomingPipeline.Start();

            RemoteEndPoint = Socket.RemoteEndPoint;

            OnConnected?.Invoke();
        }

        public virtual async Task DisconnectAsync()
        {
            var awaitable = Pools.SocketAwaitable.Take();

            try
            {
                if (!Socket.Connected)
                    return;

                await Socket.DisconnectAsync(awaitable);
            }
            finally
            {
                Pools.SocketAwaitable.Return(awaitable);
            }
        }

        public virtual Task SendMessageAsync(IMessage message)
        {
            Requires.NotNull(message, nameof(message));

            _outboundPipeline.Enqueue(message);

            return Task.CompletedTask;
        }

        public virtual Task ReceiveMessageAsync(Guid correlation)
        {
            return _incomingPipeline.RegisterPromise(correlation, _cts.Token);
        }

        public abstract Task OnMessageReceived(IMessage message);

        public void SetSocket(ISocket socket)
        {
            Requires.NotNull(socket, nameof(socket));

            Socket?.Dispose();

            Socket = socket;
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
            Socket?.Dispose();
        }

        #endregion
    }
}
