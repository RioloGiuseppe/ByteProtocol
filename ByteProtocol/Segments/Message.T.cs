using ByteProtocol.Payloads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteProtocol.Segments
{
    public abstract class Message<GenericPayload> : Message where GenericPayload : Payload, new()
    {
        private GenericPayload _data = new GenericPayload();
        public override byte[] Data { get => _data.Serialize(); set => _data.Deserialize(value); }
        public GenericPayload Payload { get => _data; }
    }
}
