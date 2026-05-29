using System.Linq;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Network;
using GOILauncher.Multiplayer.Server.Events;
using LiteNetLib;

namespace GOILauncher.Multiplayer.Server.Services
{
    public class ChatService : IChatService
    {
        private readonly IPlayerService _playerService;
        private readonly INetworkServer _networkServer;
        private readonly IEventBus _eventBus;

        public ChatService(IPlayerService playerService,
            INetworkServer networkServer,
            IPacketDispatcher dispatcher,
            IEventBus eventBus)
        {
            _playerService = playerService;
            _networkServer = networkServer;
            _eventBus = eventBus;
            dispatcher.RegisterStruct<C2SChatMessagePacket>(OnChatMessage);
        }

        public void SendMessage(string message)
        {
            // Broadcast the message to all players
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
            _networkServer.Multicast(otherPlayerIds, chatPacket, DeliveryMethod.ReliableUnordered);
            _eventBus.Publish(new ChatMessageEvent(playerId, content, timestamp));
        }
    }
}