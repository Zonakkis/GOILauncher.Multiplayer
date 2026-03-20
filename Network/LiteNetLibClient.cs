using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Network.Converters;
using GOILauncher.Multiplayer.Network.Enums;
using LiteNetLib;
using System;

namespace GOILauncher.Multiplayer.Network
{
    public class LiteNetLibClient : INetworkClient
    {
        public bool IsConnected { get; private set; }
        public event Action Connected;
        public event Action Disconnected;
        public event Action<byte[]> DataReceived;

        private readonly NetManager _netManager;
        private readonly EventBasedNetListener _listener;
        private readonly ILogger<LiteNetLibClient> _logger;
        private readonly INetworkConverter _converter;
        private readonly NetPeer _server;

        public LiteNetLibClient(
            NetManager netManager,
            EventBasedNetListener listener,
            ILogger<LiteNetLibClient> logger,
            INetworkConverter converter)
        {
            _netManager = netManager;
            _listener = listener;
            _logger = logger;
            _converter = converter;
            _listener.PeerConnectedEvent += OnServerConnected;
            _listener.PeerDisconnectedEvent += OnServerDisconnected;
            _listener.NetworkReceiveEvent += OnNetworkReceived;
        }

        public void Connect(string host, int port)
        {
            _netManager.Start();
            _netManager.Connect(host, port, "GOILauncher");
            _logger.Info($"Connecting to {host}:{port}...");
        }

        public void Disconnect()
        {
            _netManager.Stop();
        }

        public void Poll()
        {
            _netManager.PollEvents();
        }

        public void Send(byte[] data, SendMode mode)
        {
            if(_server == null) return;

            var deliveryMethod = _converter.Convert<SendMode, DeliveryMethod>(mode);
            _server.Send(data, deliveryMethod);
        }


        private void OnServerConnected(NetPeer peer)
        {
            _logger.Info("Connected to server.");
            Connected?.Invoke();
        }

        private void OnServerDisconnected(NetPeer peer, DisconnectInfo disconnectinfo)
        {
            _logger.Info("Disconnected from server.");
            Disconnected?.Invoke();
        }

        private void OnNetworkReceived(
            NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            DataReceived?.Invoke(reader.GetRemainingBytes());
            reader.Recycle();
        }
    }
}