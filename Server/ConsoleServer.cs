using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GOILauncher.Multiplayer.Server.Events;
using GOILauncher.Multiplayer.Server.Interfaces;
using GOILauncher.Multiplayer.Shared.Configuration;
using GOILauncher.Multiplayer.Shared.Events;
using GOILauncher.Multiplayer.Shared.Extensions;
using GOILauncher.Multiplayer.Shared.Net;
using GOILauncher.Multiplayer.Shared.Packets;
using LiteNetLib;
using NLog;

namespace GOILauncher.Multiplayer.Server
{
    public class ConsoleServer : IGameServer, IServerEventProcessor
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly INetServer _server = new NetServer();
        private readonly ServerPlayerManager _serverPlayerManager;

        public event EventHandler<PlayerConnectedEventArgs> PlayerConnected;
        public event EventHandler<PlayerDisconnectedEventArgs> PlayerDisconnected;
        public event EventHandler<ChatMessageReceivedEventArgs> ChatMessageReceived;

        public Dictionary<int, ServerPlayer> Players { get; } = new Dictionary<int, ServerPlayer>();
        public bool IsRunning => _server.IsRunning;

        public ConsoleServer()
        {
            NLogConfiguration.EnableNLog();
            _server.ClientConnected += OnClientConnected;
            _server.ClientDisconnected += OnPlayerDisconnected;
            var listener = _server.Listener;
            listener.NetworkReceiveEvent += OnNetworkReceived;
            _serverPlayerManager = new ServerPlayerManager(this);
        }

        public void Start(int port)
        {
            if (_server.IsRunning)
                return;
            _server.Start(port);
            var pollThread = new Thread(() =>
            {
                while (_server.IsRunning)
                {
                    _server.Poll();
                    Thread.Sleep(1);
                }
            })
            {
                IsBackground = true
            };
            var infoThread = new Thread(() =>
            {
                while (_server.IsRunning)
                {
                    if(Players.Count > 0)
                    {
                        _logger.Info("----------------统计数据----------------");
                        _logger.Info($"接收到的数据：{_server.BytesReceived.FormatBytes()}");
                        _logger.Info($"发送的数据：  {_server.BytesSent.FormatBytes()}");
                        _logger.Info($"丢包率：      {_server.PacketLossPercentage}%");
                        foreach (var player in Players.Values)
                            _logger.Info($"[{player.Id}][{player.Platform}]{player.Name}：延迟：{player.Peer.Ping}ms");
                    }
                    Thread.Sleep(5000);
                }
            })
            {
                IsBackground = true
            };
            pollThread.Start();
            infoThread.Start();
            _logger?.Info($"服务器已启动，端口：{port}");
        }

        public void Stop()
        {
            _server.Stop();
        }

        public void OnClientConnected(object sender, ClientConnectedEventArgs e)
        {
            var serverConnectedPacket = new ServerConnectedPacket()
            {
                PlayerId = e.ClientId
            };
            var writer = new NetPacketWriter(serverConnectedPacket);
            var peer = e.Peer;
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
                case PacketType.PlayerEntered:
                    var playerEnteredPacket = reader.Get<PlayerEnteredPacket>();
                    OnPlayerEntered(player, playerEnteredPacket);
                    break;
                case PacketType.PlayerLeft:
                    var playerLeftPacket = reader.Get<PlayerLeftPacket>();
                    OnPlayerLeft(player, playerLeftPacket);
                    break;
                case PacketType.PlayerMove:
                    var playerMovePacket = reader.Get<PlayerMovePacket>();
                    OnPlayerMoved(player, playerMovePacket);
                    break;
                case PacketType.ChatMessage:
                    var chatMessagePacket = reader.Get<ChatMessagePacket>();
                    OnChatMessageReceived(player, chatMessagePacket);
                    break;
                case PacketType.ServerConnected:
                default:
                    Console.WriteLine("Unknown packet type");
                    break;
            }
            reader.Recycle();
        }


        public void OnPlayerConnected(NetPeer peer, PlayerConnectedPacket packet)
        {
            var playerId = packet.PlayerId;
            var serverPlayer = new ServerPlayer
            {
                Id = playerId,
                Peer = peer,
                Name = packet.PlayerName,
                Platform = packet.Platform,
                IsInGame = packet.IsInGame,
                Move = packet.InitMove
            };
            foreach (var player in Players.Values)
            {
                var playerConnectedPacket = new PlayerConnectedPacket
                {
                    PlayerId = player.Id,
                    PlayerName = player.Name,
                    Platform = player.Platform,
                    IsInGame = player.IsInGame,
                    InitMove = player.Move
                };
                peer.Send(playerConnectedPacket, DeliveryMethod.ReliableOrdered);
                player.Peer.Send(packet, DeliveryMethod.ReliableOrdered);
            }
            Players.Add(playerId, serverPlayer);
            PlayerConnected?.Invoke(this, new PlayerConnectedEventArgs
            {
                Player = serverPlayer
            });
            _logger?.Info($"玩家[{playerId}][{packet.Platform}]{packet.PlayerName}连接到服务器。");
        }

        private void OnPlayerDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            var playerId = e.ClientId;
            if (Players.TryGetValue(playerId, out var player))
            {
                Players.Remove(playerId);
                var playerDisconnectedPacket = new PlayerDisconnectedPacket
                {
                    PlayerId = playerId
                };
                _server.SendToAll(playerDisconnectedPacket, DeliveryMethod.ReliableOrdered);
                PlayerDisconnected?.Invoke(this, new PlayerDisconnectedEventArgs
                {
                    Player = player,
                    Reason = e.Reason
                });
                _logger.Info($"玩家[{playerId}]{player.Name}断开连接:{e.Reason}。");
            }
        }

        private void OnPlayerEntered(NetPeer _, PlayerEnteredPacket playerEnteredPacket)
        {
            var playerId = playerEnteredPacket.PlayerId;
            var player = Players[playerId];
            player.Move = playerEnteredPacket.InitMove;
            player.IsInGame = true;
            _server.SendToAllExcept(playerId, playerEnteredPacket, DeliveryMethod.ReliableOrdered);
            _logger?.Info($"[{playerId}]{player.Name}进入游戏。");
        }
        private void OnPlayerLeft(NetPeer _, PlayerLeftPacket playerLeftPacket)
        {
            var playerId = playerLeftPacket.PlayerId;
            var player = Players[playerId];
            player.IsInGame = false;
            _server.SendToAllExcept(playerId, playerLeftPacket, DeliveryMethod.ReliableOrdered);
            _logger?.Info($"[{playerId}]{player.Name}离开游戏。");
        }

        private void OnPlayerMoved(NetPeer _, PlayerMovePacket packet)
        {
            var playerId = packet.PlayerId;
            var player = Players[playerId];
            player.Move = packet.Move;
            foreach (var serverPlayer in Players.Values
                .Where(serverPlayer => player.IsInGame && serverPlayer != player))
            {
                serverPlayer.Peer.Send(packet, DeliveryMethod.Unreliable);
            }
        }

        public void OnChatMessageReceived(NetPeer peer, ChatMessagePacket packet)
        {
            var id = packet.PlayerId;
            var player = Players[id];
            var message = packet.Message;
            _server.SendToAllExcept(id, packet, DeliveryMethod.ReliableOrdered);
            ChatMessageReceived?.Invoke(this, new ChatMessageReceivedEventArgs
            {
                Player = player,
                Message = message
            });
            _logger?.Info($"[{id}]{player.Name}：{message}");
        }

    }
}
