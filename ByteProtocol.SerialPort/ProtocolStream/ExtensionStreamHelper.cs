using System.IO.Ports;

namespace ByteProtocol.ProtocolStream
{
    public static class ExtensionStreamHelper
    {
        public static IByteProtocolBase CreateTcpConnection(this IByteProtocolBase protocol, string hostname, int port)
        {
            ByteProtocolTcpStream _stream = new ByteProtocolTcpStream(hostname, port);
            protocol.ProtocolStream = _stream;
            return protocol;
        }

        public static IByteProtocolBase CreateSerial(this IByteProtocolBase protocol, string name, int baud)
        {
            ByteProtocolSerialStream _stream = new ByteProtocolSerialStream(name, baud);
            _stream.Baud = baud;
            _stream.DataBits = 8;
            _stream.Parity = Parity.None;
            _stream.StopBits = StopBits.One;
            _stream.Handshake = Handshake.None;
            protocol.ProtocolStream = _stream;
            return protocol;
        }

        public static IByteProtocolBase CreateSerial(this IByteProtocolBase protocol, int baud, string name, int databits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One, Handshake handshake = Handshake.None)
        {
            ByteProtocolSerialStream _stream = new ByteProtocolSerialStream(name, baud);
            _stream.DataBits = databits;
            _stream.Parity = parity;
            _stream.StopBits = stopBits;
            _stream.Handshake = handshake;
            protocol.ProtocolStream = _stream;
            return protocol;
        }
    }
}
