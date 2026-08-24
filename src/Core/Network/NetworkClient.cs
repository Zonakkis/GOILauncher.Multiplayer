using Autofac;
using GOILauncher.Multiplayer.Core.Data.Constants;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using LiteNetLib;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Network
{
    public class NetworkClient : INetworkClient, IStartable
    {
        public bool IsConnected => _server != null && _server.ConnectionState == ConnectionState.Connected;
        private readonly NetManager _netManager;
        private readonly IEventBus _eventBus;
        private readonly ILogger<NetworkClient> _logger;
        private NetPeer _server;

        public NetworkClient(
            NetManager netManager,
            IEventBus eventBus,
            ILogger<NetworkClient> logger)
        {
            _netManager = netManager;
            _eventBus = eventBus;
            _logger = logger;
        }

        void IStartable.Start()
        {
            _eventBus.Subscribe<ServerDisconnectedEvent>((_) => _server = null);
            _netManager.Start();
        }

        public void Dispose()
        {
            _netManager.Stop();
        }

        public void Connect(string host, int port)
        {
            _logger.Info($"Connecting to {host}:{port}...");
            _server = _netManager.Connect(host, port, "GOILauncher");
        }

        public void Disconnect()
        {
            // An outgoing peer is not connected yet, but it still needs to be cancelled.
            if (_server == null) return;

            _logger.Info("Disconnecting from server...");
            _server.Disconnect();
        }

        public void Poll()
        {
            _netManager.PollEvents();
        }

        public void Send(INetSerializable packet, DeliveryMethod method)
        {
            Send(packet, NetworkChannels.Default, method);
        }

        public void Send(INetSerializable packet, byte channel, DeliveryMethod method)
        {
            if (!IsConnected) return;

            var bytes = NetSerializablePacketWriter.Write(packet);
            _server.Send(bytes, channel, method);
        }
    }
}
