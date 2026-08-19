using System.Linq;
using Autofac;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Network;
using LiteNetLib;

namespace GOILauncher.Multiplayer.Server.Services
{
    public class ChatService : IStartable
    {
        private readonly IPlayerService _playerService;
        private readonly INetworkServer _networkServer;
        private readonly IServerPacketDispatcher _dispatcher;

        public ChatService(IPlayerService playerService,
            INetworkServer networkServer,
            IServerPacketDispatcher dispatcher)
        {
            _playerService = playerService;
            _networkServer = networkServer;
            _dispatcher = dispatcher;
        }

        void IStartable.Start()
        {
            _dispatcher.RegisterStruct<C2SChatMessagePacket>(OnChatMessage);
        }

        private void OnChatMessage(C2SChatMessagePacket packet, PacketSender sender)
        {
            var playerId = sender.Id;
            var content = packet.Content;
            var timestamp = packet.Timestamp;
            var chatPacket = new S2CChatMessagePacket
            {
                PlayerId = playerId,
                Content = content,
                Timestamp = timestamp
            };
            var otherPlayerIds = _playerService.Players.Keys.Where(id => id != playerId);
            _networkServer.Multicast(otherPlayerIds, chatPacket, DeliveryMethod.ReliableOrdered);
        }
    }
}