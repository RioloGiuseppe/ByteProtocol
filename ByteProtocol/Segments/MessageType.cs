using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteProtocol.Segments
{
    internal enum MessageType : byte
    {
        Event = 0,
        Acknowledgement = 1,
        Response = 2
    }
}
