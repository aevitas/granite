using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Granite.Core.Internal;

namespace Granite.Core.Sockets
{
    public static class SocketExtensions
    {
        public static async Task<SocketError> ConnectAsync(this ISocket socket, SocketAwaitable awaitable)
        {
            Requires.NotNull(awaitable, nameof(awaitable));

            try
            {
                awaitable.Reset();

                if (!socket.ConnectAsync(awaitable.EventArgs))
                    awaitable.CompleteSynchronously();

                await awaitable;
                return awaitable.EventArgs.SocketError;
            }
            catch (ObjectDisposedException)
            {
                return SocketError.ConnectionAborted;
            }
        }

        public static async Task<SocketError> DisconnectAsync(this ISocket socket, SocketAwaitable awaitable)
        {
            Requires.NotNull(awaitable, nameof(awaitable));

            try
            {
                awaitable.Reset();

                if (!socket.DisconnectAsync(awaitable.EventArgs))
                    awaitable.CompleteSynchronously();

                await awaitable;
                return awaitable.EventArgs.SocketError;

            }
            catch (ObjectDisposedException)
            {
                return SocketError.ConnectionAborted;
            }
        }

        public static async Task<SocketError> AcceptAsync(this ISocket socket, SocketAwaitable awaitable)
        {
            Requires.NotNull(awaitable, nameof(awaitable));

            try
            {
                awaitable.Reset();

                if (!socket.AcceptAsync(awaitable.EventArgs))
                    awaitable.CompleteSynchronously();

                await awaitable;
                return awaitable.EventArgs.SocketError;

            }
            catch (ObjectDisposedException)
            {
                return SocketError.ConnectionAborted;
            }
        }

        public static async Task<SocketError> ReceiveAsync(this ISocket socket, SocketAwaitable awaitable)
        {
            try
            {
                var received = 0;
                var count = awaitable.EventArgs.Count;

                while (received < count)
                {
                    awaitable.Reset();
                    awaitable.EventArgs.SetBuffer(received, count - received);

                    if (!socket.ReceiveAsync(awaitable.EventArgs))
                        awaitable.CompleteSynchronously();

                    await awaitable;

                    if (awaitable.EventArgs.SocketError != SocketError.Success)
                        return awaitable.EventArgs.SocketError;

                    var bytesTransferred = awaitable.EventArgs.BytesTransferred;
                    if (bytesTransferred == 0)
                        return SocketError.ConnectionReset;

                    received += bytesTransferred;
                }

                return SocketError.Success;
            }
            catch (ObjectDisposedException)
            {
                return SocketError.ConnectionAborted;
            }
        }

        public static async Task<SocketError> SendAsync(this ISocket socket, SocketAwaitable awaitable)
        {
            Requires.NotNull(awaitable, nameof(awaitable));

            try
            {
                var sent = 0;
                var count = awaitable.EventArgs.Count;

                while (sent < count)
                {
                    awaitable.Reset();
                    awaitable.EventArgs.SetBuffer(sent, count - sent);

                    if (!socket.SendAsync(awaitable.EventArgs))
                        awaitable.CompleteSynchronously();

                    await awaitable;

                    if (awaitable.EventArgs.SocketError != SocketError.Success)
                        return awaitable.EventArgs.SocketError;

                    sent += awaitable.EventArgs.BytesTransferred;
                }

                return SocketError.Success;
            }
            catch (ObjectDisposedException)
            {
                return SocketError.ConnectionAborted;
            }
        }
    }
}
