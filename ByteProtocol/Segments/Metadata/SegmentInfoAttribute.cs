using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteProtocol.Segments.Metadata
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class SegmentInfoAttribute : SegmentAttribute
    {
        public SegmentInfoAttribute(int position, int lenght, bool includeInLength)
        {
            Position = position;
            Length = Convert.ToByte(lenght);
            IncludeInLength = includeInLength;
        }
    }
}
