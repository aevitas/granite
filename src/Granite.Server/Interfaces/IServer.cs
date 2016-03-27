using System;
using System.Net;
using System.Threading.Tasks;

namespace Granite.Server.Interfaces
{
    public interface IServer : IDisposable
    {
        void Listen(EndPoint localEndPoint);

        Task StopAsync();
    }
}
