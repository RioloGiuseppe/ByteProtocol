using System;
using System.Collections.Generic;
using System.Text;

namespace ByteProtocol.Segments
{
    public interface IMessage
    {
        byte[] Head { get; }
        byte[] Number { get; }
        byte[] Data { get; }
        byte ChecksumLength { get; }
    }
}
