using System;
using Granite.Core.Interfaces;
using Granite.Core.Internal;

namespace Granite.Core
{
    public class Message : IMessage
    {
        public Guid Correlation { get; }

        public uint OpCode { get; }

        public string Content { get;}

        public Message(uint opCode, string content) : this(opCode, content, Guid.NewGuid())
        {}

        internal Message(uint opCode, string content, Guid correlation)
        {
            Requires.NotNull(content, nameof(content));

            OpCode = opCode;
            Content = content;
            Correlation = correlation;
        }
    }
}
