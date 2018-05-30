using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using ByteProtocol.Exceptions;

namespace ByteProtocol.ProtocolStream
{
    public class ByteProtocolTcpStream : IByteProtocolStream
    {
        private TcpClient _tcp;
        public string Hostname { get; private set; }
        public int Port { get; private set; }
        public TimeSpan ReadingPollingTime { get; private set; }
        public System.IO.Stream Stream { get; private set; }
        public ByteProtocolTcpStream(string hostname, int port)
        {
            Hostname = hostname;
            Port = port;
        }
        public async Task<bool> Connect()
        {
            if (_tcp == null)
                _tcp = new TcpClient(Hostname, Port);
            try
            {
                if (!_tcp.Connected)
                    if (!string.IsNullOrEmpty(Hostname))
                        await _tcp.ConnectAsync(Hostname, Port);
                    else throw new ConfigurationException("invalid endpoint");
                Stream = _tcp.GetStream();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool Disconnect()
        {
            try
            {
                if (_tcp.Connected)
                {
                    try { _tcp.Close(); _tcp = null; }
                    catch (Exception) { }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> Reconnect()
        {
            Disconnect();
            return await Connect();
        }
        public void Dispose()
        {
            Disconnect();
        }
    }
}
