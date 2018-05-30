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

        private CancellationTokenSource listenerCancellationSource;

        public Func<ByteProtocolBase<GenericRequest, GenericResponse>, Task<bool>> ConnectionChallengeFunction { get; set; }

        public Func<ByteProtocolBase<GenericRequest, GenericResponse>, Task<bool>> HeartbeatChallengeFunction { get; set; }
        
        public event EventHandler<DateTime> Heartbeat;
        
        public event EventHandler<DateTime> ConnectionFailure;

        public TimeSpan HeartbeatDelay { get; set; }

        public TimeSpan ConnectionDelay { get; set; }

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
                    if (ConnectionChallengeFunction == null)
                        throw new ConfigurationException("Unsetted challenge function for reconnection.");
                    if (!await ConnectionChallengeFunction(this))
                    {
                        StoptListener();
                        ConnectionFailure?.Invoke(this, DateTime.UtcNow);
                        await ProtocolStream.Reconnect();
                        startListener();
                    }
                    await Task.Delay(ConnectionDelay);
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

        public void StoptListener()
        {
            if (listenerCancellationSource != null)
            {
                try
                {
                    listenerCancellationSource.Cancel();
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
                    if (HeartbeatChallengeFunction == null)
                        throw new ConfigurationException("Unsetted challenge function for heartbeat.");
                    if (await HeartbeatChallengeFunction.Invoke(this))
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