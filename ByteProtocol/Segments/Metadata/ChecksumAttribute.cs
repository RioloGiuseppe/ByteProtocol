using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteProtocol.Segments.Metadata
{
    public class ChecksumAttribute : SegmentAttribute
    {
        public ChecksumAttribute()
        {
            Position = int.MaxValue;
            Length = 0;
        }
    }
}
