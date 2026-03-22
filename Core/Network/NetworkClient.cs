using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using LiteNetLib;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Network
{
    public class NetworkClient : INetworkClient
    {
        public bool IsConnected { get; private set; }
        private readonly NetManager _netManager;
        private readonly NetPacketProcessor _processor;
        private readonly ILogger<NetworkClient> _logger;
        private NetPeer _server;

        public NetworkClient(
            NetManager netManager,
            NetPacketProcessor processor,
            IEventBus eventBus,
            ILogger<NetworkClient> logger)
        {
            _netManager = netManager;
            _processor = processor;
            eventBus.Subscribe<ServerDisconnectedEvent>((_) => _server = null);
            _logger = logger;
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
            if (_server == null) return;

            _logger.Info("Disconnecting from server...");
            _server.Disconnect();
        }

        public void Poll()
        {
            _netManager.PollEvents();
        }

        public void Send<T>(T packet, DeliveryMethod method) where T : class, new()
        {
            if (_server == null) return;

            var bytes = _processor.Write(packet);
            _server.Send(bytes, method);
        }

        public void Send(INetSerializable packet, DeliveryMethod method)
        {
            if (_server == null) return;

            var bytes = _processor.WriteNetSerializable(packet);
            _server.Send(bytes, method);
        }
    }
}