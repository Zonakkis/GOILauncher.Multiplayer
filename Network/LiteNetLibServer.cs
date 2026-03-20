using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Network.Converters;
using GOILauncher.Multiplayer.Network.Enums;
using LiteNetLib;
using System;

namespace GOILauncher.Multiplayer.Network
{
    public class LiteNetLibServer : INetworkServer
    {
        private readonly NetManager _netManager;
        private readonly EventBasedNetListener _listener;
        private readonly ILogger<LiteNetLibServer> _logger;
        private readonly INetworkConverter _converter;

        public event Action<int> Connected;
        public event Action<int> Disconnected;
        public event Action<byte[]> DataReceived;

        public LiteNetLibServer(
            NetManager netManager,
            EventBasedNetListener listener,
            ILogger<LiteNetLibServer> logger,
            INetworkConverter converter)
        {
            _netManager = netManager;
            _listener = listener;
            _logger = logger;
            _converter = converter;
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

        public void Send(int clientId, byte[] data, SendMode mode)
        {
            var peer = _netManager.GetPeerById(clientId);
            if(peer == null) return;

            var deliveryMethod = _converter.Convert<SendMode, DeliveryMethod>(mode);
            peer.Send(data, deliveryMethod);
        }

        private void OnClientConnected(NetPeer peer)
        {
            _logger.Info($"Client connected: {peer.EndPoint} (ID: {peer.Id})");
            Connected?.Invoke(peer.Id);
        }

        private void OnClientDisconnected(NetPeer peer, DisconnectInfo disconnectinfo)
        {
            _logger.Info($"Client disconnected: {peer.EndPoint} (ID: {peer.Id}), Reason: {disconnectinfo.Reason}");
            Disconnected?.Invoke(peer.Id);
        }

        private void OnNetworkReceived(
            NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            DataReceived?.Invoke(reader.GetRemainingBytes());
            reader.Recycle();
        }
    }
}
