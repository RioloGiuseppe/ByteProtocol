using System;
using System.Collections.Generic;
using System.Text;

namespace ByteProtocol.Exceptions
{
    public class TimeoutException : ByteProtocolGenericException
    {
        public TimeoutException() : base("Timeout expired") { }
    }
}
