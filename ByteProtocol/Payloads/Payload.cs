using ByteProtocol.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ByteProtocol.Payloads
{
    public abstract class Payload
    {
        public Payload() : base() { }

        public Payload(byte[] data, bool IsAscii = false)
        {
            FromBinary(data);
        }

        private Dictionary<PropertyInfo, SerializationInfoAttribute> GetPropertyInfo()
        {
            Dictionary<PropertyInfo, SerializationInfoAttribute> _serializationData = new Dictionary<PropertyInfo, SerializationInfoAttribute>();
            foreach (var prop in GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(prop => Attribute.IsDefined(prop, typeof(SerializationInfoAttribute))))
            {
                var SerializationInfo = prop.GetCustomAttributes(true)
                    .OfType<SerializationInfoAttribute>()
                    .FirstOrDefault();
                if (SerializationInfo == null) throw new SerializationException();
                _serializationData.Add(prop, SerializationInfo);
            }
            _serializationData.OrderBy(o => o.Value.Position);
            return _serializationData;
        }

        internal Payload Deserialize(byte[] data)
        {
            return FromBinary(data);
        }

        internal byte[] Serialize()
        {
            return ToBinary();
        }

        #region Callbacks

        protected virtual void OnPreSerilize() { }

        protected virtual void OnPostSerialize() { }

        protected virtual void OnPreDeserialize() { }

        protected virtual void OnPostDeserialize() { }

        #endregion

        #region Binary

        internal Payload FromBinary(byte[] data)
        {
            OnPreDeserialize();
            Dictionary<PropertyInfo, SerializationInfoAttribute> _serializationData = GetPropertyInfo();
            if (data != null)
            {
                foreach (var _desObj in _serializationData)
                {
                    var info = _desObj.Value;
                    // length 1 (non-aggregated bit)
                    if (!info.Size.HasValue && !info.Match.HasValue)
                        _desObj.Key.SetValue(this, data[info.Position]);
                    // // length n (string)
                    if (info.Size.HasValue)
                    {
                        if (info.Size.Value == int.MaxValue) _desObj.Key.SetValue(this, data.SubArray(info.Position, data.Length));
                        else _desObj.Key.SetValue(this, data.SubArray(info.Position, info.Size.Value));
                    }

                    // length 0 (aggregated bit)
                    if (info.Match.HasValue)
                    {
                        if (data.Length < _desObj.Value.Position + 1)
                            throw new SerializationException(_desObj.Key.Name);
                        if ((data[_desObj.Value.Position] & _desObj.Value.Match) == 0)
                            _desObj.Key.SetValue(this, false);
                        else
                            _desObj.Key.SetValue(this, true);
                    }
                }
            }
            OnPostDeserialize();
            return this;
        }

        internal byte[] ToBinary()
        {
            OnPreSerilize();
            List<byte> buffer = new List<byte>();
            Dictionary<PropertyInfo, SerializationInfoAttribute> _serializationData = GetPropertyInfo();
            foreach (var _desObj in _serializationData)
            {
                var info = _desObj.Value;
                // length 1 (non-aggregated bit)
                if (!info.Size.HasValue && !info.Match.HasValue)
                    buffer.Add((byte)_desObj.Key.GetValue(this));
                // // length n (string)
                if (info.Size.HasValue)
                    buffer.AddRange((byte[])_desObj.Key.GetValue(this));
                // length 0 (aggregated bit)
                if (info.Match.HasValue)
                {
                    if (buffer.Count <= info.Position + 1)
                        buffer.Add(0x00);
                    bool isSet = (bool)_desObj.Key.GetValue(this);
                    buffer[info.Position] |= isSet ? info.Match.Value : (byte)0x00;
                }
            }
            OnPostSerialize();
            return buffer.ToArray();
        }

        #endregion

        #region Helper

        protected byte[] __string(string str, int size)
        {
            var b = Encoding.ASCII.GetBytes(str).ToList();
            if (b.Count > size) throw new SerializationException(size, b.Count);
            while (b.Count < size)
                b.Add(0x00);
            return b.ToArray();
        }

        protected string __string(byte[] str, int size = int.MaxValue)
        {
            var b = Encoding.ASCII.GetString(str);
            if (b.Length > size && size < int.MaxValue)
                throw new SerializationException(size, b.Length);
            return b.Replace("\0", string.Empty);
        }

        protected byte __int(int b)
        {
            return (byte)b;
        }

        protected int __int(byte b)
        {
            return (int)b;
        }

        #endregion

    }
}
