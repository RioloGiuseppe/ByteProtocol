using ByteProtocol.ProtocolStream;
using ByteProtocol.Segments;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using ByteProtocol.Payloads;

namespace ByteProtocol
{
    public abstract partial class ByteProtocolBase<GenericMessage> : IByteProtocolBase where GenericMessage : Message, new()
    {
        public bool CanMessageModifyHead { get; set; } = false;
        private MessageMaker<GenericMessage> _messageMaker;
        public delegate void ByteProtocolEvent<in EventPayload>(ByteProtocolBase<GenericMessage> sender, EventPayload obj) where EventPayload : Payload;


        private byte[] DefaultHead = new byte[0];
        protected abstract byte[] Checksum(byte[] data);
        protected abstract byte[] Stuffing(byte[] data);
        protected abstract byte MessageStart { get; }
        protected abstract byte? MessageStop { get; }
        protected abstract byte[] Unstuffing(byte[] data);
        protected virtual void OnInit() { }
        protected virtual byte[] ModifyHead(byte[] head) => new byte[0];
        private int _timeout;
        public int Timeout { get => _timeout < 200 ? 200 : _timeout; set => _timeout = value; }


        #region Constructors
        public ByteProtocolBase()
        {
            _registry = new MessageRegistry<GenericMessage>(this);
            OnInit();
            _messageMaker = new MessageMaker<GenericMessage>(MessageStart, MessageStop, Unstuffing, Checksum);
            _messageMaker.MessageArrived += ManageReceivedMessage;
        }

        public ByteProtocolBase(byte[] head) : this()
        {
            if (head != null)
                DefaultHead = head;
        }

        public ByteProtocolBase(IByteProtocolStream stream) : this()
        {
            ProtocolStream = stream;
        }

        public ByteProtocolBase(IByteProtocolStream stream, byte[] head) : this(head)
        {
            ProtocolStream = stream;
        }

        #endregion

        #region Acknowledgement
        public virtual void SendAck() { }
        public virtual Task SendAckAsync() => Task.CompletedTask;
        public virtual void SendNack() { }
        public virtual Task SendNackAsync() => Task.CompletedTask;
        #endregion

        #region Protocol

        private byte[] ComputeBaseMessage(GenericMessage s)
        {
            List<byte> message = new List<byte>();
            List<byte> innerMessage = new List<byte>();
            if (CanMessageModifyHead)
                DefaultHead = ModifyHead(DefaultHead);
            s.Head = DefaultHead;
            innerMessage.AddRange(s.ToBytes());
            var cs = Checksum(innerMessage.ToArray()) ?? new byte[0];
            innerMessage.AddRange(cs);
            message.Add(MessageStart);
            message.AddRange(Stuffing(innerMessage.ToArray()));
            if (MessageStop.HasValue) message.Add(MessageStop.Value);
            return message.ToArray();
        }
        private void SendSegment(GenericMessage c)
        {
            var m = ComputeBaseMessage(c);

#if DEBUG
            Console.ForegroundColor = ConsoleColor.Cyan;
            foreach (var item in m)
                Console.Write(String.Format("{0:X02} ", item));
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
#endif

            ProtocolStream?.Stream.Write(m, 0, m.Length);
            ProtocolStream?.Stream.Flush();
        }
        private Task SendSegmentAsync(GenericMessage c)
        {
            return Task.Factory.StartNew(() =>
             {
                 var m = ComputeBaseMessage(c);
                 ProtocolStream?.Stream.Write(m, 0, m.Length);
                 ProtocolStream?.Stream.Flush();
             });
        }

        #endregion

        #region Stream

        private CancellationTokenSource _listenerCancellationToken = new CancellationTokenSource();

        private CancellationToken _cancellationToken;

        public IByteProtocolStream ProtocolStream { get; set; }

        public async Task Connect()
        {
            RegisterMessages(Registry);
            await ProtocolStream?.Connect();
            startListener();
        }

        private void startListener()
        {
            Task.Factory.StartNew(async () =>
            {
                while (!_cancellationToken.IsCancellationRequested)
                {
                    int read;
                    var buf = new byte[1];
                    while ((read = await ProtocolStream.Stream.ReadAsync(buf, 0, 1)) != 0)
                    {
                        await _messageMaker.PushDataAsync(buf);
                    }
                    await Task.Delay(ProtocolStream.ReadingPollingTime);
                }
            }, _listenerCancellationToken.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        #endregion

        #region Message Register

        private readonly MessageRegistry<GenericMessage> _registry;
        internal MessageRegistry<GenericMessage> Registry => _registry;
        protected abstract void RegisterMessages(MessageRegistry<GenericMessage> registry);

        #endregion

        #region IDisposable

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _messageMaker.MessageArrived -= ManageReceivedMessage;
                    _messageMaker.Dispose();
                    _listenerCancellationToken.Cancel();
                }
                _disposed = true;
            }
        }

        ~ByteProtocolBase()
        {
            Dispose(false);
        }

        #endregion
    }
}