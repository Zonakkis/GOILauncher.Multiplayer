using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Server.Events;
using LiteNetLib;

namespace GOILauncher.Multiplayer.Server.Services
{
    public class ChatService : IChatService
    {
        private readonly IServerService _serverService;
        private readonly IEventBus _eventBus;

        public ChatService(IServerService serverService, IPacketDispatcher dispatcher, IEventBus eventBus)
        {
            _serverService = serverService;
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
            _serverService.Broadcast(chatPacket);
            _eventBus.Publish(new ChatMessageEvent(playerId, content, timestamp));
        }
    }
}