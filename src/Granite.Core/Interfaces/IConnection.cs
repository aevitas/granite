using System;
using System.Threading.Tasks;
using Granite.Core.Sockets;

namespace Granite.Core.Interfaces
{
    public interface IConnection : IDisposable
    {
        ISocket Socket { get; }

        Task ConnectAsync(string host, int port, int timeoutMilliseconds);

        Task DisconnectAsync();

        Task SendMessageAsync(IMessage message);

        Task<IMessage> ReceiveMessageAsync(Guid correlation);

        Task<IMessage> SendAndAwaitResponseAsync(IMessage message);

        Task OnMessageReceived(IMessage message);

        void SetSocket(ISocket socket);
    }
}
