using ByteProtocol.Payloads;
using ByteProtocol.Segments;

namespace ByteProtocol
{
    public interface IMessageRegistry
    {
        IMessageRegistry RegisterEvent<EventPayload>(byte[] number, ByteProtocolEvent<EventPayload> @event, bool requreAck = false) where EventPayload : Payload;
        IMessageRegistry RegisterEvent<GenericPayload>(byte number, ByteProtocolEvent<GenericPayload> @event, bool requreAck = false) where GenericPayload : Payload;
        IMessageRegistry RegisterQuery(byte number, byte responseNumber);
        IMessageRegistry RegisterQuery(byte[] number, byte[] responseNumber);
    }
}