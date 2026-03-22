using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using LiteNetLib;
using System.Net;
using System.Net.Sockets;

namespace GOILauncher.Multiplayer.Core.Network
{
    public class NetworkClientListener : INetEventListener
    {
        private readonly IPacketDispatcher _dispatcher;
        private readonly IEventBus _eventBus;
        private readonly ILogger<NetworkClientListener> _logger;

        public NetworkClientListener(IPacketDispatcher dispatcher,
            IEventBus eventBus,
            ILogger<NetworkClientListener> logger)
        {
            _dispatcher = dispatcher;
            _eventBus = eventBus;
            _logger = logger;
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {

        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {

        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {

        }

        public void OnNetworkReceive(NetPeer peer,
            NetPacketReader reader,
            DeliveryMethod deliveryMethod)
        {
            _dispatcher.Dispatch(peer, reader);
            reader.Recycle();
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {

        }

        public void OnPeerConnected(NetPeer peer)
        {
            _logger.Info("Connected to server.");
            _eventBus.Publish(new ServerConnectedEvent());
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            var reason = string.Format("({0}){1}", disconnectInfo.SocketErrorCode, disconnectInfo.Reason);
            _logger.Info("Disconnected from server: " + reason);
            _eventBus.Publish(new ServerDisconnectedEvent(reason));
        }
    }
}
