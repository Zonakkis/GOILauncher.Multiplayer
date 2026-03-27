using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using LiteNetLib;
using System;
using System.Net;
using System.Net.Sockets;

namespace GOILauncher.Multiplayer.Core.Network
{
    public class NetworkServerListener : INetEventListener
    {
        private readonly IPacketDispatcher _dispatcher;
        private readonly IEventBus _eventBus;
        private readonly ILogger<NetworkServerListener> _logger;

        public NetworkServerListener(IPacketDispatcher dispatcher,
            IEventBus eventBus,
            ILogger<NetworkServerListener> logger)
        {
            _dispatcher = dispatcher;
            _eventBus = eventBus;
            _logger = logger;
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
            request.AcceptIfKey("GOILauncher");
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
            _logger.Info($"Client connected: {peer.EndPoint} (ID: {peer.Id})");
            _eventBus.Publish(new ClientConnectedEvent(peer.Id));
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            var reason = string.Format("({0}){1}", disconnectInfo.SocketErrorCode, disconnectInfo.Reason);
            _logger.Info($"Client disconnected: {peer.EndPoint} (ID: {peer.Id}), Reason: {reason}");
            _eventBus.Publish(new ClientDisconnectedEvent(peer.Id, reason));
        }
    }
}