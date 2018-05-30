using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteProtocol.Segments
{
    public abstract class Acknowledge : Message
    {
        public override byte[] Data { get => new byte[0];  set { } }
    }
}
