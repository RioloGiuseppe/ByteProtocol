using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ByteProtocol
{
    internal class RequestTaskInfo
    {
        //private byte _message;

        public RequestTaskInfo(byte[] number, AutoResetEvent are)
        {
            Number = number;
            Are = are;
            Timestamp = DateTime.Now;
        }

        public AutoResetEvent Are { get; set; }

        public byte[] Number { get; set; }

        public byte[] Data { get; set; } = null;

        public DateTime Timestamp { get; private set; }

        public byte Status { get; set; }
    }
}
