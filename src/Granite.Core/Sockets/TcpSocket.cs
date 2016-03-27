using System.Net;
using System.Net.Sockets;
using Granite.Core.Internal;

namespace Granite.Core.Sockets
{
    public class TcpSocket : ISocket
    {
        public TcpSocket(AddressFamily addressFamily)
        {
            Socket = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                Blocking = false
            };
        }

        public TcpSocket(Socket socket)
        {
            Requires.NotNull(socket, nameof(socket));

            Socket = socket;
        }

        public Socket Socket { get; }

        #region Implementation of IDisposable

        public void Dispose()
        {
            Socket?.Dispose();
        }

        #endregion

        public bool ReceiveAsync(SocketAsyncEventArgs args)
        {
            return Socket.ReceiveAsync(args);
        }

        public bool SendAsync(SocketAsyncEventArgs args)
        {
            return Socket.SendAsync(args);
        }

        public bool AcceptAsync(SocketAsyncEventArgs args)
        {
            return Socket.AcceptAsync(args);
        }

        public bool ConnectAsync(SocketAsyncEventArgs args)
        {
            return Socket.ConnectAsync(args);
        }

        public bool DisconnectAsync(SocketAsyncEventArgs args)
        {
            return Socket.DisconnectAsync(args);
        }

        public void Bind(EndPoint endPoint)
        {
            Socket.Bind(endPoint);
        }

        public void Listen(int backlog)
        {
            Socket.Listen(backlog);
        }

        public void Shutdown(SocketShutdown how)
        {
            Socket.Shutdown(how);
        }

        public void Close()
        {
            Socket.Close();
        }

        public bool Blocking
        {
            get { return Socket.Blocking; }
            set { Socket.Blocking = value; }
        }

        public bool NoDelay
        {
            get { return Socket.NoDelay; }
            set { Socket.NoDelay = value; }
        }

        public bool Connected => Socket.Connected;

        public IPEndPoint RemoteEndPoint => (IPEndPoint) Socket.RemoteEndPoint;

        public IPEndPoint LocalEndPoint => (IPEndPoint) Socket.LocalEndPoint;
    }
}