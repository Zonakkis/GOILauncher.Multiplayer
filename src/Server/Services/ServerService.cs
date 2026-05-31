using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Network;
using LiteNetLib;

namespace GOILauncher.Multiplayer.Server.Services
{
    public class ServerService : IServerService
    {
        public bool IsRunning => _networkServer.IsRunning;
        private readonly INetworkServer _networkServer;

        public ServerService(INetworkServer networkServer,
            IEventBus eventBus)
        {
            _networkServer = networkServer;
            eventBus.Subscribe<ClientConnectedEvent>(OnClientConnected);
        }

        public void Dispose()
        {
            _networkServer.Dispose();
        }

        public void Start(int port)
        {
            _networkServer.Start(port);
        }

        public void Stop()
        {
            _networkServer.Stop();
        }

        public void Poll()
        {
            _networkServer.Poll();
        }

        private void OnClientConnected(ClientConnectedEvent e)
        {
            var packet = new S2CServerHandShakePacket { PlayerId = e.ClientId };
            _networkServer.Send(e.ClientId, packet, DeliveryMethod.ReliableUnordered);
        }
    }
}
