using ByteProtocol.ProtocolStream;
using ByteProtocol.Segments;
using System;
using System.Collections.Generic;
using System.Text;

namespace ByteProtocol
{
    public abstract class ByteProtocolBase<T> : ByteProtocolBase<T, T> where T : Message, new()
    {
        public ByteProtocolBase() : base() { }
        public ByteProtocolBase(byte[] head) : base(head) { }
        public ByteProtocolBase(IByteProtocolStream byteProtocolStream) : base(byteProtocolStream) { }
        public ByteProtocolBase(IByteProtocolStream byteProtocolStream, byte[] head) : base(byteProtocolStream, head) { }
    }
}
