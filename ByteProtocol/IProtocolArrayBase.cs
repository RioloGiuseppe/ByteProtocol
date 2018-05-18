using ByteProtocol.ProtocolStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteProtocol
{
    public interface IByteProtocolBase : IDisposable
    {
        IByteProtocolStream ProtocolStream { get; set; }
    }
}
