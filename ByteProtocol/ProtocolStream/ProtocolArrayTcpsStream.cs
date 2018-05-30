using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Threading.Tasks;
using ByteProtocol.Exceptions;

namespace ByteProtocol.ProtocolStream
{
    public class ByteProtocolTcpsStream : IByteProtocolStream
    {
        private TcpClient _tcp;

        public string Hostname { get; private set; }

        public int Port { get; private set; }

        public TimeSpan ReadingPollingTime { get; private set; }

        public System.IO.Stream Stream { get; private set; }

        public string MachineName { get; set; }

        public string ServerName { get; set; }

        public ByteProtocolTcpsStream(string hostname, int port)
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
                Stream = CreateSecureStream(MachineName, ServerName);
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

        private SslStream CreateSecureStream(string machineName, string serverName)
        {
            SslStream sslStream = new SslStream(_tcp.GetStream(), false, (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) =>
            {
                if (sslPolicyErrors == SslPolicyErrors.None)
                    return true;
                Console.WriteLine("Certificate error: {0}", sslPolicyErrors);
                return false;
            }, null);
            try
            {
                sslStream.AuthenticateAsClient(serverName);
            }
            catch (AuthenticationException e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
                if (e.InnerException != null)
                {
                    Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                }
                Console.WriteLine("Authentication failed - closing the connection.");
                _tcp.Close();
                return null;
            }
            return sslStream;
        }
    }
}