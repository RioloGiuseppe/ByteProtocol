using ByteProtocol.Segments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteProtocol.Segments.Metadata
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PayloadAttribute : SegmentAttribute
    {
        public PayloadAttribute(int position)
        {
            Position = position;
        }
    }
}
