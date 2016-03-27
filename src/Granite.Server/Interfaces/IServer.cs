using System;
using System.Net;

namespace Granite.Server.Interfaces
{
    public interface IServer : IDisposable
    {
        void Listen(EndPoint localEndPoint);
    }
}
