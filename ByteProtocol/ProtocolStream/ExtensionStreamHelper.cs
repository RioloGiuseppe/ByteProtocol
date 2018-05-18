
namespace ByteProtocol.ProtocolStream
{
    public static partial class ExtensionStreamHelper
    {
        public static IByteProtocolBase CreateTcpConnection(this IByteProtocolBase protocol, string hostname, int port)
        {
            ByteProtocolTcpStream _stream = new ByteProtocolTcpStream(hostname, port);
            protocol.ProtocolStream = _stream;
            return protocol;
        }       
    }
}
