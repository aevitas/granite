namespace Granite.Core.Internal
{
    internal struct MessageHeader
    {
        public int Length { get; set; }

        public uint OpCode { get; set; }
    }
}
