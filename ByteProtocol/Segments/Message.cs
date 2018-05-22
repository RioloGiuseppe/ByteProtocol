using ByteProtocol.Exceptions;
using ByteProtocol.Payloads;
using ByteProtocol.Segments.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ByteProtocol.Segments
{
    public interface IMessage
    {
        byte[] Head { get; set; }
        byte[] Number { get; set; }
        byte[] Data { get; set; }
        byte ChecksumLength { get; }
    }

    public abstract class Message : IMessage
    {
        public Message()
        {
            foreach (var p in GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(o => o.PropertyType == typeof(Byte[])))
                p.SetValue(this, new byte[0]);
        }

        public abstract byte[] Head { get; set; }

        public abstract byte[] Number { get; set; }
        [SegmentInfo(0, 1, true)]

        internal protected byte Length { get; set; }

        public abstract byte[] Data { get; set; }

        [Checksum()]
        internal byte[] Checksum { get; set; }

        public abstract byte ChecksumLength { get; }

        internal Dictionary<PropertyInfo, SegmentAttribute> GetPropertyInfo()
        {
            Dictionary<PropertyInfo, SegmentAttribute> _serializationData = new Dictionary<PropertyInfo, SegmentAttribute>();
            foreach (var prop in GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(prop => Attribute.IsDefined(prop, typeof(SegmentAttribute))))
            {
                var SegmentInfo = prop.GetCustomAttributes(true)
                    .OfType<SegmentAttribute>()
                    .FirstOrDefault();
                if (SegmentInfo == null) throw new SerializationException();
                _serializationData.Add(prop, SegmentInfo);
            }
            _serializationData = _serializationData.OrderBy(o => o.Value.Position).ToDictionary(o => o.Key, o => o.Value);
            return _serializationData;
        }

        internal byte[] ToBytes()
        {
            OnPreSerilize();
            List<byte> buffer = new List<byte>();
            Dictionary<PropertyInfo, SegmentAttribute> _serializationData = GetPropertyInfo()
                .Where(o => o.Key.Name != "Checksum")
                .ToDictionary(o => o.Key, o => o.Value);
            foreach (var _desObj in _serializationData)
            {
                var info = _desObj.Value;
                if (_desObj.Key.PropertyType == typeof(byte[]) && _desObj.Key.Name != "Data")
                    if (((byte[])_desObj.Key.GetValue(this)).Length == _desObj.Value.Length)
                        buffer.AddRange((byte[])_desObj.Key.GetValue(this));
                //else
                //    throw new SerializationException();
                if (Length > 0 && _desObj.Key.PropertyType == typeof(byte[]) && _desObj.Key.Name == "Data")
                    if (((byte[])_desObj.Key.GetValue(this)).Length == PayloadLength())
                        buffer.AddRange((byte[])_desObj.Key.GetValue(this));
                    else throw new SerializationException();
                if (_desObj.Key.PropertyType == typeof(byte))
                    if (_desObj.Value.Length == 1)
                        buffer.Add((byte)_desObj.Key.GetValue(this));
                    else throw new SerializationException();
            }
            OnPostSerialize();
            return buffer.ToArray();
        }

        public abstract byte PayloadLength();

        #region Callbacks

        protected virtual void OnPreSerilize() { }

        protected virtual void OnPostSerialize() { }

        #endregion
    }
}
