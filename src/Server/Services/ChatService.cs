using System.Linq;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Network;
using LiteNetLib;

namespace GOILauncher.Multiplayer.Server.Services
{
    public class ChatService
    {
        private readonly IPlayerService _playerService;
        private readonly INetworkServer _networkServer;

        public ChatService(IPlayerService playerService,
            INetworkServer networkServer,
            IPacketDispatcher dispatcher)
        {
            _playerService = playerService;
            _networkServer = networkServer;
            dispatcher.RegisterStruct<C2SChatMessagePacket>(OnChatMessage);
        }

        private void OnChatMessage(C2SChatMessagePacket packet, NetPeer peer)
        {
            var playerId = peer.Id;
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