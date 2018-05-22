using ByteProtocol.ProtocolStream;
using ByteProtocol.Segments;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using ByteProtocol.Payloads;
using ByteProtocol.Exceptions;

namespace ByteProtocol
{
    public abstract partial class ByteProtocolBase<GenericRequest, GenericResponse>
    {
        private CancellationTokenSource reconnectCancellationSource;
        private CancellationTokenSource heartbeatCancellationSource;
        public Func<ByteProtocolBase<GenericRequest, GenericResponse>, Task<bool>> connectionChallengeFunction { get; set; }
        public Func<ByteProtocolBase<GenericRequest, GenericResponse>, Task<bool>> heartbeatChallengeFunction { get; set; }
        public event EventHandler<DateTime> Heartbeat;
        public event EventHandler<DateTime> ConnectionFailure;
        public TimeSpan HeartbeatDelay { get; set; }
        public Task StartReconnection()
        {
            reconnectCancellationSource = new CancellationTokenSource();
            return Task.Factory.StartNew(async () =>
            {
                reconnectCancellationSource.Token.ThrowIfCancellationRequested();
                while (true)
                {
                    if (reconnectCancellationSource.Token.IsCancellationRequested)
                        reconnectCancellationSource.Token.ThrowIfCancellationRequested();
                    if (connectionChallengeFunction == null)
                        throw new ConfigurationException("Unsetted challenge function for reconnection.");
                    if (!await connectionChallengeFunction(this))
                    {
                        ConnectionFailure?.Invoke(this, DateTime.UtcNow);
                        await ProtocolStream.Reconnect();
                    }
                    await Task.Delay(new TimeSpan(0, 1, 0));
                }
            }, reconnectCancellationSource.Token);
        }

        public void StoptReconnection()
        {
            if (reconnectCancellationSource != null)
            {
                try
                {
                    reconnectCancellationSource.Cancel();
                }
                catch (Exception) { }
            }
        }

        public Task StartHeartbeat()
        {
            heartbeatCancellationSource = new CancellationTokenSource();
            return Task.Factory.StartNew(async () =>
            {
                heartbeatCancellationSource.Token.ThrowIfCancellationRequested();
                while (true)
                {
                    if (heartbeatChallengeFunction == null)
                        throw new ConfigurationException("Unsetted challenge function for heartbeat.");
                    if (await heartbeatChallengeFunction.Invoke(this))
                        Heartbeat?.Invoke(this, DateTime.UtcNow);
                    await Task.Delay(HeartbeatDelay);
                    if (heartbeatCancellationSource.Token.IsCancellationRequested)
                        heartbeatCancellationSource.Token.ThrowIfCancellationRequested();
                }
            }, heartbeatCancellationSource.Token);
        }

        public void StoptHeartbeat()
        {
            if (heartbeatCancellationSource != null)
            {
                try
                {
                    heartbeatCancellationSource.Cancel();
                }
                catch (Exception) { }
            }
        }
    }
}