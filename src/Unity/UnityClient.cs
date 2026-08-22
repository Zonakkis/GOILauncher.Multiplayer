using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Client.Models;
using GOILauncher.Multiplayer.Client.Services;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Unity.Config;
using GOILauncher.Multiplayer.Unity.Events;
using GOILauncher.Multiplayer.Unity.Extensions;
using GOILauncher.Multiplayer.Unity.Models;
using GOILauncher.Multiplayer.Unity.Player;
using UnityEngine;

namespace GOILauncher.Multiplayer.Unity
{

    public class UnityClient : MonoBehaviour, IUnityClient
    {
        private bool _initialized;

        public bool IsConnected => IsMultiplayerEnabled && ClientService.IsConnected;
        public ReadOnlyCollection<Message> ChatMessages => ChatService.Messages;
        public IClientService ClientService { get; set; }
        public IPlayerService PlayerService { get; set; }
        public IChatService ChatService { get; set; }
        public IEventBus EventBus { get; set; }
        public IGameManager GameManager { get; set; }
        public IPlayerManager PlayerManager { get; set; }
        public IMultiplayerState MultiplayerState { get; set; }

        public event Action Connected;
        public event Action<string> Disconnected;
        public event Action<Message> ChatMessageReceived;
        public event Action PlayerListUpdated;

        /// <summary>
        /// 每次枚举都现场造 PlayerView，不留副本：名单的唯一所有者是 PlayerService。
        /// </summary>
        public IEnumerable<PlayerView> Players
        {
            get
            {
                foreach (PlayerInfo info in PlayerService.Players)
                    yield return new PlayerView(info.Id, PlayerService, PlayerManager);
            }
        }

        public bool TryGetPlayer(int playerId, out PlayerView player)
        {
            PlayerInfo info;
            if (!PlayerService.TryGetPlayer(playerId, out info))
            {
                player = default(PlayerView);
                return false;
            }

            player = new PlayerView(info.Id, PlayerService, PlayerManager);
            return true;
        }

        public void Init()
        {
            if (_initialized)
                return;

            _initialized = true;
            EventBus.Subscribe<GameStartedEvent>(OnGameStartedEvent);
            EventBus.Subscribe<GameQuitEvent>(OnGameQuitEvent);
            EventBus.Subscribe<PlayerListUpdatedEvent>(OnPlayerListUpdatedEvent);
            // 门面把下层事件转成自己的事件，UI 不需要认识 EventBus 上的事件类型，
            // 也不会看到服务端角色的事件（两个角色共用一条总线）。
            EventBus.Subscribe<ServerConnectedEvent>(OnServerConnectedEvent);
            EventBus.Subscribe<ServerDisconnectedEvent>(OnServerDisconnectedEvent);
            EventBus.Subscribe<ChatMessageEvent>(OnChatMessageEvent);
        }

        private void Update()
        {
            ClientService?.Poll();
        }

        public void Connect(string host, int port, string playerName)
        {
            if (!IsMultiplayerEnabled)
                return;

            var playerInfo = new PlayerInfo(0, playerName, Application.platform.ToPlatform(), GameManager.IsInGame);

            PlayerService.SetLocalPlayerInfo(playerInfo);
            ClientService.Connect(host, port);
        }

        public void Disconnect()
        {
            ClientService.Disconnect();
        }

        public void SendMessage(MessageType type, string message)
        {
            if (!IsMultiplayerEnabled)
                return;

            ChatService.SendMessage(type, message);
        }

        /// <summary>
        /// 门面只做取件：拿到本地玩家和目标实例两个对象，具体怎么搬在 LocalPlayer.TeleportTo。
        /// </summary>
        public void TeleportTo(int playerId)
        {
            if (!IsMultiplayerEnabled)
                return;

            // 本地不在游戏里时 PlayerManager.LocalPlayer 为 null；
            // 传自己的 Id 时 GetPlayer 返回的是 LocalPlayer，as RemotePlayer 自然落空，
            // 所以"能不能传"这两种情况都不用单独判。
            LocalPlayer local = PlayerManager.LocalPlayer;
            RemotePlayer target = PlayerManager.GetPlayer(playerId) as RemotePlayer;
            if (local == null || target == null)
                return;

            local.TeleportTo(target.transform);
        }

        private bool IsMultiplayerEnabled
        {
            get { return MultiplayerState == null || MultiplayerState.Enabled; }
        }

        private void OnGameStartedEvent(GameStartedEvent @event)
        {
            PlayerService.SetIsInGame(true);
        }

        private void OnGameQuitEvent(GameQuitEvent @event)
        {
            PlayerService.SetIsInGame(false);
        }

        private void OnPlayerListUpdatedEvent(PlayerListUpdatedEvent @event)
        {
            PlayerListUpdated?.Invoke();
        }

        private void OnServerConnectedEvent(ServerConnectedEvent @event)
        {
            Connected?.Invoke();
        }

        private void OnServerDisconnectedEvent(ServerDisconnectedEvent @event)
        {
            Disconnected?.Invoke(@event.Reason);
        }

        private void OnChatMessageEvent(ChatMessageEvent @event)
        {
            ChatMessageReceived?.Invoke(@event.Message);
        }
    }
}
