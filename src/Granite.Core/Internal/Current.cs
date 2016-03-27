using Granite.Core.Serialization;

namespace Granite.Core.Internal
{
    internal static class Current
    {
        public static ISerializer Serializer => JsonSerializer.Instance;
    }
}
