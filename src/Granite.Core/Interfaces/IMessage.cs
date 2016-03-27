using System;

namespace Granite.Core.Interfaces
{
    public interface IMessage
    {
        Guid Correlation { get; }

        object Content { get; }
    }
}
