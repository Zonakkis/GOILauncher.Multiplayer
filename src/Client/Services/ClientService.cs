using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Network;
using LiteNetLib;

namespace GOILauncher.Multiplayer.Client.Services
{
    public class ClientService : IClientService
    {
        private readonly INetworkClient _networkClient;
        private readonly IEventBus _eventBus;
        public bool IsConnected => _networkClient.IsConnected;

        public ClientService(INetworkClient networkClient,
            IPacketDispatcher dispatcher,
            IEventBus eventBus)
        {
            _networkClient = networkClient;
            _eventBus = eventBus;
            dispatcher.RegisterStruct<S2CServerHandShakePacket>(OnServerHandshake);
        }

        public void Dispose()
        {
            _networkClient.Dispose();
        }

        public void Connect(string host, int port)
        {
            _networkClient.Connect(host, port);
        }

        public void Disconnect()
        {
            _networkClient.Disconnect();
        }

        public void Poll()
        {
            _networkClient.Poll();
        }

        private void OnServerHandshake(S2CServerHandShakePacket packet, NetPeer _)
        {
            var playerId = packet.PlayerId;
            _eventBus.Publish(new ServerHandshakeEvent(playerId));
        }
    }
}
