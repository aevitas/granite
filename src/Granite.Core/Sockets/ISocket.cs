using System;
using System.Net;
using System.Net.Sockets;

namespace Granite.Core.Sockets
{
    public interface ISocket : IDisposable
    {
        bool ReceiveAsync(SocketAsyncEventArgs args);
        bool SendAsync(SocketAsyncEventArgs args);
        bool AcceptAsync(SocketAsyncEventArgs args);
        bool ConnectAsync(SocketAsyncEventArgs args);
        bool DisconnectAsync(SocketAsyncEventArgs args);

        void Bind(EndPoint endPoint);
        void Listen(int backlog);

        void Shutdown(SocketShutdown how);

        void Close();

        bool Blocking { get; set; }
        bool NoDelay { get; set; }

        bool Connected { get; }
        IPEndPoint RemoteEndPoint { get; }
        IPEndPoint LocalEndPoint { get; }
    }
}
