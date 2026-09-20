using System.Net;

namespace GOILauncher.Multiplayer.Core.Event
{
    public class ClientConnectedEvent
    {
        public int ClientId { get; }
        /// <summary>Remote endpoint captured at accept time; identity facts never stored on PlayerInfo.</summary>
        public IPEndPoint RemoteEndPoint { get; }

        public ClientConnectedEvent(int clientId, IPEndPoint remoteEndPoint)
        {
            ClientId = clientId;
            RemoteEndPoint = remoteEndPoint;
        }
    }
}
