using System.Collections.Generic;
using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Client.Models;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Core.Utils;
using GOILauncher.Multiplayer.Network;
using LiteNetLib;

namespace GOILauncher.Multiplayer.Client.Services
{
    public class ChatService : IChatService
    {
        private readonly INetworkClient _networkClient;
        private readonly IEventBus _eventBus;
        private readonly ILogger<ChatService> _logger;
        private readonly IPlayerService _playerService;
        public List<Message> Messages { get; } = new List<Message>();
        public ChatService(INetworkClient networkClient,
            IPacketDispatcher dispatcher,
            IEventBus eventBus,
            ILogger<ChatService> logger,
            IPlayerService playerService)
        {
            _networkClient = networkClient;
            _eventBus = eventBus;
            _logger = logger;
            _playerService = playerService;
            dispatcher.RegisterStruct<S2CChatMessagePacket>(OnChatMessage);
            eventBus.Subscribe<PlayerJoinedEvent>(OnPlayerJoined);
            eventBus.Subscribe<PlayerLeftEvent>(OnPlayerLeft);
        }

        private void AddMessage(Message message)
        {
            if (message == null)
                return;

            Messages.Add(message);
            _eventBus.Publish(new ChatMessagesUpdatedEvent(Messages, message));
        }

        private void AddChatMessage(Message message)
        {
            if (message == null)
                return;

            AddMessage(message);
            _eventBus.Publish(new ChatMessageEvent(message));
        }

        private void SendSystemMessage(string content)
        {
            AddMessage(new Message(MessageType.System, "系统", content));
        }

        private void SendChatMessage(string content)
        {
            if (!_networkClient.IsConnected)
            {
                SendSystemMessage("尚未连接到服务器。");
                return;
            }

            var packet = new C2SChatMessagePacket(content);
            _networkClient.Send(packet, DeliveryMethod.ReliableUnordered);
        }

        public void SendMessage(MessageType type, string content)
        {
            switch (type)
            {
                case MessageType.System:
                    SendSystemMessage(content);
                    break;
                case MessageType.Player:
                    SendChatMessage(content);
                    break;
                case MessageType.Server:
                    AddMessage(new Message(MessageType.Server, "服务器", content));
                    break;
                default:
                    _logger.Warn("Unknown message type: {MessageType}, Content: {Content}", type, content);
                    break;
            }
        }

        private void OnChatMessage(S2CChatMessagePacket packet, NetPeer _)
        {
            var playerId = packet.PlayerId;
            var dateTime = DateTimeUtils.FromUnixTimeSeconds(packet.Timestamp);
            if (_playerService.Players.TryGetValue(playerId, out var player))
            {
                var message = new Message(MessageType.Player, player.Name, packet.Content, dateTime);
                AddChatMessage(message);
            }
            else
            {
                _logger.Warn("Unknown playerId: {PlayerId}, Content: {Content}", playerId, packet.Content);
                var message = new Message(MessageType.Player, $"玩家 {playerId}", packet.Content, dateTime);
                AddChatMessage(message);
            }
        }
        private void OnPlayerJoined(PlayerJoinedEvent e)
        {
            var message = new Message(MessageType.Server, "服务器", $"玩家 {e.PlayerName} 加入了游戏。");
            AddMessage(message);
        }
        private void OnPlayerLeft(PlayerLeftEvent e)
        {
            AddMessage(new Message(MessageType.Server, "服务器", $"玩家 {e.PlayerName} 离开了游戏。"));
        }
    }
}
