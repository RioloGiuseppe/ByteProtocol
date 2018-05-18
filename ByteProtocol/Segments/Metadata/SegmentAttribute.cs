using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteProtocol.Segments.Metadata
{
    public abstract class SegmentAttribute : Attribute
    {
        public int Position { get; protected set; }
        public int Length { get; protected set; }
        public bool IncludeInLength { get; protected set; }
    }
}
