using System;

namespace Granite.Core
{
    public class CanNotConnectException : Exception
    {
        public string Host { get; }

        public int Port { get; }

        public CanNotConnectException(string message) : base(message) { }

        public CanNotConnectException(string message, string host, int port) : base(message)
        {
            Host = host;
            Port = port;
        }
    }
}
