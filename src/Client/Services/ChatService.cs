using System;
using Autofac;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class ChatService : IChatService, IStartable
    {
        private readonly INetworkClient _networkClient;
        private readonly IClientPacketDispatcher _dispatcher;
        private readonly IEventBus _eventBus;
        private readonly ILogger<ChatService> _logger;
        private readonly IPlayerService _playerService;
        private readonly List<Message> _messages = new List<Message>();
        public ReadOnlyCollection<Message> Messages { get; private set; }
        public ChatService(INetworkClient networkClient,
            IClientPacketDispatcher dispatcher,
            IEventBus eventBus,
            ILogger<ChatService> logger,
            IPlayerService playerService)
        {
            _networkClient = networkClient;
            _dispatcher = dispatcher;
            _eventBus = eventBus;
            _logger = logger;
            _playerService = playerService;
            Messages = new ReadOnlyCollection<Message>(_messages);
        }

        void IStartable.Start()
        {
            _dispatcher.RegisterStruct<S2CChatMessagePacket>(OnChatMessage);
            _eventBus.Subscribe<PlayerJoinedEvent>(OnPlayerJoined);
            _eventBus.Subscribe<PlayerLeftEvent>(OnPlayerLeft);
        }

        private void AddMessage(Message message)
        {
            if (message == null)
                return;

            _messages.Add(message);
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
            _networkClient.Send(packet, DeliveryMethod.ReliableOrdered);

            // 本地回显：服务端只把消息广播给其他玩家（排除发送者），自己的消息需要本地直接显示
            AddMessage(new Message(MessageType.Player, _playerService.LocalPlayer.Name, content, DateTime.Now));
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

        private void OnChatMessage(S2CChatMessagePacket packet, PacketSender _)
        {
            var playerId = packet.PlayerId;
            var dateTime = DateTimeUtils.FromUnixTimeSeconds(packet.Timestamp);
            if (_playerService.TryGetPlayer(playerId, out var player))
            {
                var message = new Message(MessageType.Player, player.Name, packet.Content, dateTime);
                AddMessage(message);
            }
            else
            {
                _logger.Warn("Unknown playerId: {PlayerId}, Content: {Content}", playerId, packet.Content);
                var message = new Message(MessageType.Player, $"玩家 {playerId}", packet.Content, dateTime);
                AddMessage(message);
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
