using System;

namespace Granite.Core.Interfaces
{
    public interface IMessage
    {
        Guid Correlation { get; }

        uint OpCode { get; }

        string Content { get; }
    }
}
