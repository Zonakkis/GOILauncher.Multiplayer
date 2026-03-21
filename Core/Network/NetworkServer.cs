using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using LiteNetLib;
using LiteNetLib.Utils;
using System;

namespace GOILauncher.Multiplayer.Network
{
    public class NetworkServer : INetworkServer
    {
        private readonly NetManager _netManager;
        private readonly EventBasedNetListener _listener;
        private readonly NetPacketProcessor _processor;
        private readonly IPacketDispatcher _dispatcher;
        private readonly IEventBus _eventBus;
        private readonly ILogger<NetworkServer> _logger;

        public NetworkServer(
            NetManager netManager,
            EventBasedNetListener listener,
            NetPacketProcessor processor,
            IPacketDispatcher dispatcher,
            IEventBus eventBus,
            ILogger<NetworkServer> logger)
        {
            _netManager = netManager;
            _listener = listener;
            _processor = processor;
            _dispatcher = dispatcher;
            _eventBus = eventBus;
            _logger = logger;
            _listener.PeerConnectedEvent += OnClientConnected;
            _listener.PeerDisconnectedEvent += OnClientDisconnected;
            _listener.NetworkReceiveEvent += OnNetworkReceived;
        }

        public void Start(int port)
        {
            _netManager.Start(port);
            _logger.Info($"Server started on {port}.");
        }

        public void Stop()
        {
            _netManager.Stop();
            _logger.Info("Server stopped.");
        }

        public void Poll()
        {
            _netManager.PollEvents();
        }

        public void Send<T>(int clientId, T packet, DeliveryMethod method) where T : class, new()
        {
            var peer = _netManager.GetPeerById(clientId);
            if (peer == null) return;

            var bytes = _processor.Write(packet);
            peer.Send(bytes, method);
        }

        public void Send(int clientId, INetSerializable packet, DeliveryMethod method)
        {
            var peer = _netManager.GetPeerById(clientId);
            if (peer == null) return;

            var bytes = _processor.WriteNetSerializable(packet);
            peer.Send(bytes, method);
        }

        private void OnClientConnected(NetPeer peer)
        {
            _logger.Info($"Client connected: {peer.EndPoint} (ID: {peer.Id})");
            _eventBus.Publish(new ClientConnectedEvent(peer.Id));
        }

        private void OnClientDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            _logger.Info($"Client disconnected: {peer.EndPoint} (ID: {peer.Id}), Reason: {disconnectInfo.Reason}");
            _eventBus.Publish(new ClientDisconnectedEvent(
                peer.Id, string.Format("({0}){1}", disconnectInfo.SocketErrorCode, disconnectInfo.Reason)));
        }

        private void OnNetworkReceived(
            NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            _dispatcher.Dispatch(peer, reader);
            reader.Recycle();
        }
    }
}
