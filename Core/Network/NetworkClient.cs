using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using LiteNetLib;

namespace GOILauncher.Multiplayer.Network
{
    public class NetworkClient : INetworkClient
    {
        public bool IsConnected { get; private set; }
        private readonly NetManager _netManager;
        private readonly EventBasedNetListener _listener;
        private readonly IPacketDispatcher _dispatcher;
        private readonly IEventBus _eventBus;
        private readonly ILogger<NetworkClient> _logger;
        private NetPeer _server;

        public NetworkClient(
            NetManager netManager,
            EventBasedNetListener listener,
            IPacketDispatcher dispatcher,
            IEventBus eventBus,
            ILogger<NetworkClient> logger)
        {
            _netManager = netManager;
            _listener = listener;
            _dispatcher = dispatcher;
            _eventBus = eventBus;
            _logger = logger;
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

        public void Send(byte[] data, DeliveryMethod method)
        {
            if (_server == null) return;

            _server.Send(data, method);
        }


        private void OnServerConnected(NetPeer peer)
        {
            _logger.Info("Connected to server.");
            _server = peer;
            _eventBus.Publish(new ServerConnectedEvent());
        }

        private void OnServerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            _logger.Info("Disconnected from server.");
            _server = null;
            _eventBus.Publish(new ServerDisconnectedEvent(
                string.Format("({0}){1}", disconnectInfo.SocketErrorCode, disconnectInfo.Reason)));
        }

        private void OnNetworkReceived(
            NetPeer peer, NetPacketReader reader, DeliveryMethod method)
        {
            _dispatcher.Dispatch(peer, reader);
            reader.Recycle();
        }
    }
}