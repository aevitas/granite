using System.ServiceModel.Channels;
using Granite.Core.Sockets;

namespace Granite.Core.Internal
{
    internal static class Pools
    {
        public static IPool<SocketAwaitable> SocketAwaitable
            => new ObjectPool<SocketAwaitable>(() => new SocketAwaitable());

        public static BufferManager BufferManagerInternal
            => BufferManager.CreateBufferManager(100*1024*1024, 3*1024*1024);
    }
}
