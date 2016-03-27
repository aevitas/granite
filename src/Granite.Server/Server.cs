using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Granite.Core.Interfaces;
using Granite.Core.Internal;
using Granite.Core.Sockets;
using Granite.Server.Interfaces;

namespace Granite.Server
{
    public class Server : IServer
    {
        private readonly ILog _log = LogManager.GetLogger("Server");
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly List<IConnection> _clients = new List<IConnection>();
        private readonly Func<IConnection> _clientFactory; 

        private Task _listenerTask;
        private ISocket _listenerSocket;
        private bool _isDisposed;

        public Server(Func<IConnection> clientFactory)
        {
            Requires.NotNull(clientFactory, nameof(clientFactory));

            _clientFactory = clientFactory;
        }

        public void Listen(EndPoint localEndPoint)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (!CanListen((IPEndPoint) localEndPoint))
                throw new InvalidOperationException("Can not start a server that was previously disposed!");

            _listenerSocket = new TcpSocket(localEndPoint.AddressFamily);
            _listenerSocket.Bind(localEndPoint);
            _listenerSocket.Listen(5000);

            _listenerTask = AcceptClientAsync();

            _log.Info($"Listening for connections on {localEndPoint}");
        }

        private bool CanListen(IPEndPoint endPoint)
        {
            if (endPoint.Port <= 0)
                throw new ArgumentOutOfRangeException("Port can not be zero or negative!");

            if (_listenerSocket != null)
                return false;

            return true;
        }

        private async Task AcceptClientAsync()
        {
            SocketAwaitable awaitable = Pools.SocketAwaitable.Take();

            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    var error = await _listenerSocket.AcceptAsync(awaitable);

                    if (error == SocketError.ConnectionReset)
                        continue;

                    if (error == SocketError.OperationAborted)
                    {
                        _log.Error($"{error} returned by AcceptClientAsync - requesting cancellation and shutting down.");
                        _cts.Cancel();
                        break;
                    }

                    if (error != SocketError.Success)
                    {
                        _log.Error($"AcceptAsync failed with error: {error}");
                        continue;
                    }

                    var socket = new TcpSocket(awaitable.EventArgs.AcceptSocket);
                    socket.NoDelay = true;

                    // Null it now so we don't dispose of it in the finally clause.
                    awaitable.EventArgs.AcceptSocket = null;

                    var client = _clientFactory();
                    var ep = socket.RemoteEndPoint;
                    client.SetSocket(socket);

                    _lock.EnterWriteLock();
                    try
                    {
                        _clients.Add(client);
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                        _log.Debug($"Client from {ep} successfully connected.");
                    }
                }
            }
            finally
            {
                awaitable.EventArgs.AcceptSocket?.Dispose();
                awaitable.EventArgs.AcceptSocket = null;

                Pools.SocketAwaitable.Return(awaitable);
            }
        }

        public async Task StopAsync()
        {
            if (_listenerTask == null)
                return;

            if (_listenerSocket == null)
                return;

            _cts.Cancel();
            _listenerSocket.Dispose();

            await _listenerTask;

            _cts.Dispose();
            Dispose();
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _listenerSocket?.Dispose();

            _cts.Dispose();

            _isDisposed = true;
        }

        #endregion
    }
}
