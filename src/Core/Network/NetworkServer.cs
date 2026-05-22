using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using LiteNetLib;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Network
{
    public class NetworkServer : INetworkServer
    {
        public bool IsRunning => _netManager.IsRunning;
        private readonly NetManager _netManager;
        private readonly NetPacketProcessor _processor;
        private readonly IEventBus _eventBus;
        private readonly ILogger<NetworkServer> _logger;

        public NetworkServer(
            NetManager netManager,
            NetPacketProcessor processor,
            IEventBus eventBus,
            ILogger<NetworkServer> logger)
        {
            _netManager = netManager;
            _processor = processor;
            _eventBus = eventBus;
            _logger = logger;
        }

        public void Dispose()
        {
            Stop();
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

            var bytes = NetSerializablePacketWriter.Write(packet);
            peer.Send(bytes, method);
        }
    }
}
