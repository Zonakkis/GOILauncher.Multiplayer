using System;
using System.Collections.Generic;
using GOILauncher.Multiplayer.Server.Events;
using GOILauncher.Multiplayer.Server.Interfaces;
using GOILauncher.Multiplayer.Shared.Events;
using GOILauncher.Multiplayer.Shared.Extensions;
using GOILauncher.Multiplayer.Shared.Net;
using GOILauncher.Multiplayer.Shared.Packets;
using LiteNetLib;
using UnityEngine;

namespace GOILauncher.Multiplayer.Server
{
    public class UnityGameServer : MonoBehaviour, IGameServer, IServerEventProcessor
    {
        public static UnityGameServer Instance { get; private set; }
        private readonly INetServer _server = new NetServer();
        private ServerPlayerManager _serverPlayerManager;
        public event EventHandler<PlayerConnectedEventArgs> PlayerConnected;
        public event EventHandler<ChatMessageReceivedEventArgs> ChatMessageReceived;
        public Dictionary<int, ServerPlayer> Players { get; } = new Dictionary<int, ServerPlayer>();
        public bool IsRunning => _server.IsRunning;

        public static UnityGameServer Init()
        {
            var gameServer = new GameObject(nameof(UnityGameServer), typeof(UnityGameServer));
            DontDestroyOnLoad(gameServer);
            return Instance = gameServer.GetComponent<UnityGameServer>();
        }

        public void Awake()
        {
            _server.ClientConnected += OnClientConnected;
            _serverPlayerManager = new ServerPlayerManager(this);
            var listener = _server.Listener;
            listener.NetworkReceiveEvent += OnNetworkReceived;
        }

        public void Update()
        {
            _server.Poll();
        }

        public void Start(int port)
        {
            _server.Start(port);
        }

        public void Stop()
        {
            _server.Stop();
        }

        public void OnClientConnected(object sender, ClientConnectedEventArgs e)
        {
            var playerId = e.ClientId;
            var peer = e.Peer;
            var writer = new NetPacketWriter(new ServerConnectedPacket
            {
                PlayerId = playerId
            });
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        public void OnNetworkReceived(NetPeer player, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            var packetType = reader.GetPacketType();
            switch (packetType)
            {
                case PacketType.PlayerConnected:
                    var playerConnectedPacket = reader.Get<PlayerConnectedPacket>();
                    OnPlayerConnected(player, playerConnectedPacket);
                    break;
                case PacketType.ChatMessage:
                    var chatMessagePacket = reader.Get<ChatMessagePacket>();
                    OnChatMessageReceived(player, chatMessagePacket);
                    break;
                case PacketType.ServerConnected:
                default:
                    Debug.LogWarning($"未定义的数据包类型：{packetType}");
                    break;
            }
            reader.Recycle();
        }

        public void OnPlayerConnected(NetPeer peer, PlayerConnectedPacket packet)
        {
            var playerId = packet.PlayerId;
            var player = new ServerPlayer
            {
                Peer = peer,
                Id = packet.PlayerId,
                Name = packet.PlayerName,
                Platform = packet.Platform,
                IsInGame = packet.IsInGame
            };
            Players.Add(playerId, player);
            PlayerConnected?.Invoke(this, new PlayerConnectedEventArgs
            {
                Player = player
            });
        }

        public void OnChatMessageReceived(NetPeer peer, ChatMessagePacket packet)
        {
            var playerId = packet.PlayerId;
            var player = Players[playerId];
            var message = packet.Message;
            ChatMessageReceived?.Invoke(this, new ChatMessageReceivedEventArgs
            {
                Player = player,
                Message = message
            });
        }
    }
}
