using ByteProtocol.Payloads;
using ByteProtocol.Segments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteProtocol
{
    public class MessageRegistry<GenericMessage>  where GenericMessage : Message, new()
    {
        private ByteProtocolBase<GenericMessage> byteProtocolBase;
        private List<MessageRegistryInfo<GenericMessage>> _messages = new List<MessageRegistryInfo<GenericMessage>>();

        public MessageRegistry(ByteProtocolBase<GenericMessage> byteProtocolBase)
        {
            this.byteProtocolBase = byteProtocolBase;
        }
        public MessageRegistry<GenericMessage> RegisterEvent<EventPayload>(byte[] number, ByteProtocolBase<GenericMessage>.ByteProtocolEvent<EventPayload> @event, bool requreAck = false) where EventPayload : Payload
        {
            _messages.Add(new MessageRegistryInfo<GenericMessage>(MessageType.Event)
            {
                MessageNumber = number,
                RequreAck = requreAck,
                Event = (j, x) => @event?.Invoke(j, x as EventPayload),
                EventType = typeof(EventPayload)
            });
            return this;
        }
        public MessageRegistry<GenericMessage> RegisterEvent<GenericPayload>(byte number, ByteProtocolBase<GenericMessage>.ByteProtocolEvent<GenericPayload> @event, bool requreAck = false) where GenericPayload : Payload =>
            RegisterEvent(new byte[1] { number }, @event, requreAck);
        public MessageRegistry<GenericMessage> RegisterQuery(byte[] number, byte[] responseNumber)
        {
            _messages.Add(new MessageRegistryInfo<GenericMessage>(MessageType.Event)
            {
                MessageNumber = number,
                ResponseNumber = responseNumber
            });
            return this;
        }
        public MessageRegistry<GenericMessage> RegisterQuery(byte number, byte responseNumber) =>
            RegisterQuery(new byte[1] { number }, new byte[1] { responseNumber });

        internal MessageRegistryInfo<GenericMessage> GetMessageInfo(byte[] number)
        {
            return _messages.Where(o => o.MessageNumber.SequenceEqual(number)).FirstOrDefault();
        }

        internal IEnumerable<MessageRegistryInfo<GenericMessage>> GetMessageInfoByResponse(byte[] number) =>
             _messages
                .Where(o => Enumerable.SequenceEqual(o.ResponseNumber, number));

    }
}