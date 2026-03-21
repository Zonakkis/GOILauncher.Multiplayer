using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Packets.S2C;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Network;
using LiteNetLib;

namespace GOILauncher.Multiplayer.Client.Services
{
    public class ClientService
    {
        private readonly INetworkClient _networkClient;
        private readonly IEventBus _eventBus;

        public ClientService(INetworkClient networkClient, 
        IPacketDispatcher dispatcher,
         IEventBus eventBus)
        {
            _networkClient = networkClient;
            _eventBus = eventBus;
            dispatcher.RegisterStruct<ServerHandShakePacket>(OnServerHandshake);
        }

        public void Connect(string host, int port)
        {
            _networkClient.Connect(host, port);
        }

        public void OnServerHandshake(ServerHandShakePacket packet, NetPeer peer)
        {
            _eventBus.Publish(new ServerHandshakeEvent(packet.PlayerId));
        }
    }
}
