using ByteProtocol.Payloads;
using ByteProtocol.Segments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteProtocol
{
    public class MessageRegistry<GenericRequest, GenericResponse> : IMessageRegistry where GenericRequest : Message, new() where GenericResponse : Message, new()
    {

        private List<MessageRegistryInfo> _messages = new List<MessageRegistryInfo>();

        public IMessageRegistry RegisterEvent<EventPayload>(byte[] number, ByteProtocolEvent<EventPayload> @event, bool requreAck = false) where EventPayload : Payload
        {
            _messages.Add(new MessageRegistryInfo(MessageType.Event)
            {
                MessageNumber = number,
                RequreAck = requreAck,
                Event = (j, x) => @event?.Invoke(j, x as EventPayload),
                EventType = typeof(EventPayload)
            });
            return this;
        }


        public IMessageRegistry RegisterQuery(byte[] number, byte[] responseNumber)
        {
            _messages.Add(new MessageRegistryInfo(MessageType.Event)
            {
                MessageNumber = number,
                ResponseNumber = responseNumber
            });
            return this;
        }

        public IMessageRegistry RegisterQuery(byte number, byte responseNumber) => RegisterQuery(new byte[1] { number }, new byte[1] { responseNumber });
        internal MessageRegistryInfo GetMessageInfo(byte[] number) => _messages.Where(o => o.MessageNumber.SequenceEqual(number)).FirstOrDefault();
        public IMessageRegistry RegisterEvent<GenericPayload>(byte number, ByteProtocolEvent<GenericPayload> @event, bool requreAck = false) where GenericPayload : Payload => RegisterEvent(new byte[1] { number }, @event, requreAck);
        internal IEnumerable<MessageRegistryInfo> GetMessageInfoByResponse(byte[] number) => _messages.Where(o => Enumerable.SequenceEqual(o.ResponseNumber, number));
    }
}