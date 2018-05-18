using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteProtocol.Payloads
{
    public class SerializationInfoAttribute : Attribute
    {
        public int Position { get; set; }
        public int? Size { get; set; }
        public new byte? Match { get; set; }

        public SerializationInfoAttribute(int position) { Position = position; Size = null; Match = null; }

        public SerializationInfoAttribute(int position, int size) : this(position) { Size = size; Match = null; }

        public SerializationInfoAttribute(int position, byte match) : this(position) { Size = null; Match = match; }

    }
}
