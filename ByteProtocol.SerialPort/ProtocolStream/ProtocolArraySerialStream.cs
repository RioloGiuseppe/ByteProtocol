using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ByteProtocol.ProtocolStream
{
    public class ByteProtocolSerialStream : IByteProtocolStream
    {
        public System.IO.Stream Stream { get; private set; }
        public TimeSpan ReadingPollingTime { get; set; } = new TimeSpan(0, 0, 0, 0, 100);
        private System.IO.Ports.SerialPort _serial = null;
        public string Name { get; set; }
        public int Baud { get; set; }
        public Parity Parity { get; set; }
        public int DataBits { get; set; }
        public StopBits StopBits { get; set; }
        public ByteProtocolSerialStream() { }
        public Handshake Handshake { get; set; }
        public ByteProtocolSerialStream(string name) : this() { Name = name; }
        public ByteProtocolSerialStream(string name, int baud) : this(name) { Baud = baud; }
        public ByteProtocolSerialStream(string name, int baud, Parity parity) : this(name, baud) { Parity = parity; }
        public ByteProtocolSerialStream(string name, int baud, Parity parity, int dataBits) : this(name, baud, parity) { DataBits = dataBits; }
        public ByteProtocolSerialStream(string name, int baud, Parity parity, int dataBits, StopBits stopBits) : this(name, baud, parity, dataBits) { StopBits = stopBits; }
        public async Task<bool> Connect()
        {
            if (_serial == null)
            {
                _serial = new SerialPort();
                _serial.PortName = Name;
                _serial.BaudRate = Baud;
                _serial.Parity = Parity;
                _serial.DataBits = DataBits;
                _serial.StopBits = StopBits;
                _serial.Handshake = Handshake;
            }
            try
            {
                if (!_serial.IsOpen)
                    await Task.Factory.StartNew(() => _serial.Open());
                Stream = _serial.BaseStream;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> Disconnect()
        {
            try
            {
                if (_serial.IsOpen)
                    await Task.Factory.StartNew(() => _serial.Close());
                Stream.Flush();
                Stream.Close();
                Stream.Dispose();
                Stream = null;
                _serial = null;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> Reconnect()
        {
            await Disconnect();
            return await Connect();
        }
        public void Dispose()
        {
            Disconnect().Wait();
        }
    }
}
