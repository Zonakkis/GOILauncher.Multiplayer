using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Packets.C2S;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Network;
using GOILauncher.Multiplayer.Server.Events;
using LiteNetLib;

namespace GOILauncher.Multiplayer.Server.Services
{
    public class ServerService
    {
        private readonly INetworkServer _networkServer;
        private readonly IEventBus _eventBus;

        public ServerService(INetworkServer networkServer,
            IPacketDispatcher dispatcher,
            IEventBus eventBus)
        {
            _networkServer = networkServer;
            _eventBus = eventBus;
            dispatcher.RegisterStruct<ClientHandShakePacket>(OnServerHandshake);
        }

        public void Start(int port)
        {
            _networkServer.Start(port);
        }

        public void OnServerHandshake(ClientHandShakePacket packet, NetPeer peer)
        {
            _eventBus.Publish(new ClientHandshakeEvent(packet.PlayerName));
        }
    }
}
