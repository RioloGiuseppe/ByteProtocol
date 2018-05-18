using ByteProtocol.Segments.Metadata;
using ByteProtocol.Segments;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ByteProtocol
{
    internal class MessageMaker<T> where T : Message, new()
    {
        private BlockingCollection<byte> _inputBuffer = new BlockingCollection<byte>();
        private CancellationTokenSource _messageCreatorCancellationSource = new CancellationTokenSource();
        private CancellationToken _messageCreatorCancellationToken;
        private byte _start;
        private byte? _stop;
        private Func<byte[], byte[]> _unstuffData;
        private Func<byte[], byte[]> _checksum;

        internal event Action<T> MessageArrived;

        public MessageMaker(byte start, byte? stop, Func<byte[], byte[]> unstuff, Func<byte[], byte[]> checksum)
        {
            _start = start;
            _stop = stop;
            _unstuffData = unstuff;
            _checksum = checksum;
            MakeMessage();
        }

        public Task PushDataAsync(byte[] stream)
        {
            return Task.Factory.StartNew(() =>
            {
                foreach (var b in stream)
                    _inputBuffer.Add(b);
            });
        }

        private void MakeMessage()
        {
            Task.Factory.StartNew(() =>
            {
                List<byte> _message = new List<byte>();
                while (!_messageCreatorCancellationToken.IsCancellationRequested)
                {
                    _message.Clear();
                    bool _isSetLen = false;
                    while (_inputBuffer.Take() != _start) ;
                    var message = new T();
                    List<byte> _ = new List<byte>();

                    var props = message.GetPropertyInfo();

                    foreach (var prop in props)
                    {
                        byte len = 0;
                        _.Clear();
                        if (prop.Value.GetType() == typeof(SegmentInfoAttribute))
                            len = (_isSetLen && message.Length == 0 && prop.Value.IncludeInLength) ? (byte)0 : Convert.ToByte(prop.Value.Length);
                        if (prop.Value.GetType() == typeof(ChecksumAttribute))
                            len = Convert.ToByte(prop.Value.Length);
                        if (prop.Value.GetType() == typeof(PayloadAttribute))
                            len = message.PayloadLength();
                        while (_unstuffData(_.ToArray()).Length < len)
                            _.Add(_inputBuffer.Take());
                        if (prop.Key.PropertyType == typeof(byte[]))
                            prop.Key.SetValue(message, _unstuffData(_.ToArray()));
                        if (prop.Key.PropertyType == typeof(byte))
                        {
                            prop.Key.SetValue(message, _unstuffData(_.ToArray())[0]);
                            _isSetLen = true;
                        }
                    }
#if DEBUG
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(String.Format("{0:X02} ", _start));
                    Console.Write(String.Format("{0:X02} ", message.Length));
                    foreach (var item in message.Head)
                        Console.Write(String.Format("{0:X02} ", item));
                    foreach (var item in message.Number)
                        Console.Write(String.Format("{0:X02} ", item));
                    foreach (var item in message.Data)
                        Console.Write(String.Format("{0:X02} ", item));
                    foreach (var item in message.Checksum)
                        Console.Write(String.Format("{0:X02} ", item));
                    Console.Write(String.Format("{0:X02} ", _stop));
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Gray;
#endif
                    if (verifyChecksum(message))
                        MessageArrived?.Invoke(message);
                    if (_stop.HasValue)
                        while (_inputBuffer.Take() != _stop.Value) ;
                }
            }, _messageCreatorCancellationSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        #region IDisposable

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _messageCreatorCancellationSource.Cancel();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MessageMaker()
        {
            Dispose(false);
        }

        #endregion

        #region ProtocolVerification
        private bool verifyChecksum(Message segment)
        {
            List<byte> message = new List<byte>();
            var pps = segment.GetPropertyInfo().Where(o => o.Key.Name != "Checksum");
            foreach (var p in pps)
            {
                if (p.Key.PropertyType == typeof(byte[]))
                    message.AddRange(((byte[])p.Key.GetValue(segment)));
                if (p.Key.PropertyType == typeof(byte))
                    message.Add((byte)p.Key.GetValue(segment));
            }
            var cks = _checksum(message.ToArray());
            return Enumerable.SequenceEqual(cks, segment.Checksum);
        }
        #endregion

    }
}




