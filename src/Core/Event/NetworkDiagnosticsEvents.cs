using System.Net;
using System.Net.Sockets;

namespace GOILauncher.Multiplayer.Core.Event
{
    /// <summary>
    /// LiteNetLib reports per-peer latency on the Poll thread while statistics are enabled.
    /// </summary>
    public sealed class NetworkLatencyUpdatedEvent
    {
        public int ClientId { get; }
        public int LatencyMilliseconds { get; }

        public NetworkLatencyUpdatedEvent(int clientId, int latencyMilliseconds)
        {
            ClientId = clientId;
            LatencyMilliseconds = latencyMilliseconds;
        }
    }

    /// <summary>
    /// Socket-level error surfaced by LiteNetLib. Previously dropped silently by the listener;
    /// published so the observation facade can count it and the log can show it.
    /// </summary>
    public sealed class NetworkErrorEvent
    {
        public string EndPoint { get; }
        public SocketError SocketError { get; }

        public NetworkErrorEvent(IPEndPoint endPoint, SocketError socketError)
        {
            EndPoint = endPoint == null ? string.Empty : endPoint.ToString();
            SocketError = socketError;
        }
    }
}
