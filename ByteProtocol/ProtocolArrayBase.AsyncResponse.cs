using ByteProtocol.Payloads;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ByteProtocol
{
    public abstract partial class ByteProtocolBase<GenericRequest, GenericResponse>
    {
        private ConcurrentDictionary<Guid, RequestTaskInfo> _lockedTasks = new ConcurrentDictionary<Guid, RequestTaskInfo>();

        protected Task<RequestPayload> StartRequest<RequestPayload>(byte[] request, Payload inputdata) where RequestPayload : Payload, new()
        {
            var locker = new AutoResetEvent(false);
            var guid = Guid.NewGuid();
            var adata = new RequestTaskInfo(request, locker);
            _lockedTasks.TryAdd(guid, adata);

            return Task<RequestPayload>.Factory.StartNew(() =>
            {
                var message = new GenericRequest();
                message.Data = inputdata.Serialize();
                message.Number = request;
                message.Length = (byte)(message.Data.Length + message.Number.Length);
                SendSegment(message);
                if (locker.WaitOne(Timeout))
                {
                    _lockedTasks.TryRemove(guid, out adata);
                    var ret = new RequestPayload();
                    ret.Deserialize(adata.Data);
                    return ret;
                }
                else
                {
                    throw new ByteProtocol.Exceptions.TimeoutException();
                }
            });
        }

        protected Task<bool> StartCommand(byte[] command, Payload inputdata)
        {
            return Task<bool>.Factory.StartNew(() =>
            {
                try
                {
                    var message = new GenericRequest();
                    message.Data = inputdata.Serialize();
                    message.Number = command;
                    message.Length = ComputeLength(message);
                    SendSegment(message);
                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }
            });
        }

        protected abstract byte ComputeLength(GenericRequest message);

        internal void ManageReceivedMessage(GenericResponse e)
        {
            var messageinfo = Registry.GetMessageInfo(e?.Number);
            if (messageinfo?.Type == Segments.MessageType.Event)
            {
                if (messageinfo.RequreAck) SendAck();
                var payload = Activator.CreateInstance(messageinfo.EventType) as Payload;
                payload.Deserialize(e.Data);
                messageinfo.Event?.Invoke(this, payload);
                return;
            }
            if (messageinfo?.Type == Segments.MessageType.Response || messageinfo == null)
            {
                var infos = Registry.GetMessageInfoByResponse(e.Number);
                var kv = _lockedTasks
                    .Join(infos, o => o.Value.Number, i => i.MessageNumber, (o, g) => o.Value, new BytesComparer())
                    .OrderBy(o => o.Timestamp)
                    .FirstOrDefault();
                if (kv != null)
                {
                    kv.Data = e.Data;
                    kv.Are.Set();
                }
                return;
            }
            if (messageinfo?.Type == Segments.MessageType.Acknowledgement)
            {

                return;
            }
        }
    }
}
