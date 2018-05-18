using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteProtocol.Exceptions
{
    public abstract class ByteProtocolGenericException : Exception
    {
        public ByteProtocolGenericException() : base() { }
        public ByteProtocolGenericException(string message) : base(message) { }
        public ByteProtocolGenericException(string message, Exception innerException) : base(message, innerException) { }
    }
}
