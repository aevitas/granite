using System;
using Granite.Core.Interfaces;
using Granite.Core.Internal;

namespace Granite.Core
{
    public class Message : IMessage
    {
        public Guid Correlation { get; }

        public uint OpCode { get; }

        public object Content { get;}

        public Message(uint opCode, object content) : this(opCode, content, Guid.NewGuid())
        {}

        internal Message(uint opCode, object content, Guid correlation)
        {
            Requires.NotNull(content, nameof(content));

            OpCode = opCode;
            Content = content;
            Correlation = correlation;
        }
    }
}
