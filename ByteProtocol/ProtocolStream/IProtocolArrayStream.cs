using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteProtocol.ProtocolStream
{
    public interface IByteProtocolStream: IDisposable
    {
        TimeSpan ReadingPollingTime { get; }
        System.IO.Stream Stream { get; }
        Task<bool> Connect();
        bool Disconnect();
        Task<bool> Reconnect();
    }
}
