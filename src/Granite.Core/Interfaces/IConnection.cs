using System;
using System.Threading;
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

        Task ReceiveMessageAsync(Guid correlation);

        Task OnMessageReceived(IMessage message);
    }
}
