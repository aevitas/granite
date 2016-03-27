using System;
using Granite.Core.Interfaces;
using Granite.Core.Internal;

namespace Granite.Core
{
    public class Message : IMessage
    {
        public Guid Correlation { get; }

        public object Content { get;}

        public Message(object content) : this(content, Guid.NewGuid())
        {}

        internal Message(object content, Guid correlation)
        {
            Requires.NotNull(content, nameof(content));

            Content = content;
            Correlation = correlation;
        }
    }
}
