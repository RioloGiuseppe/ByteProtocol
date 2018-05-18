using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteProtocol.Exceptions
{
    public class SerializationException : ByteProtocolGenericException
    {
        public SerializationException() :
            base("Unexpected error occurred during serialization!")
        { }

        public SerializationException(int l, int a) : base($"Unexpected string length! expected {l} arrived {a}")
        { }

        public SerializationException(string pname) :
            base($"Invalid data position for property {pname}")
        { }

    }
}
