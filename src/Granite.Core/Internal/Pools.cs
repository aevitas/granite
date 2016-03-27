using System.ServiceModel.Channels;
using Granite.Core.Sockets;

namespace Granite.Core.Internal
{
    internal static class Pools
    {
        private const int MaximumBufferPoolSize = 100*1024*1024;
        private const int MaximumBufferSize = 3*1024*1024;

        public static IPool<SocketAwaitable> SocketAwaitable
            => new ObjectPool<SocketAwaitable>(() => new SocketAwaitable());

        public static BufferManager BufferManagerInternal
            => BufferManager.CreateBufferManager(MaximumBufferPoolSize, MaximumBufferSize);
    }
}
