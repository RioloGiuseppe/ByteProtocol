using ByteProtocol.Payloads;
using ByteProtocol.Segments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteProtocol
{
    internal class MessageRegistryInfo<GenericMessage> where GenericMessage : Message, new()
    {
        public MessageRegistryInfo(MessageType type)
        {
            Type = type;
        }
        public MessageType Type { get; private set; }
        public bool WaitForAck { get; set; }

        #region Query
        public byte[] MessageNumber { get; set; }
        public byte[] ResponseNumber { get; set; } = new byte[0];
        #endregion

        #region Events
        public bool RequreAck { get; set; }
        public Type EventType { get; set; }
        public ByteProtocolBase<GenericMessage>.ByteProtocolEvent<Payload> Event { get; set; }
        #endregion
    }
}
