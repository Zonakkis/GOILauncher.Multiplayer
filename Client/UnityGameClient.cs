using System;
using System.Collections.Generic;
using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Client.Interfaces;
using GOILauncher.Multiplayer.Shared.Constants;
using GOILauncher.Multiplayer.Shared.Extensions;
using GOILauncher.Multiplayer.Shared.Game;
using GOILauncher.Multiplayer.Shared.Interfaces;
using GOILauncher.Multiplayer.Shared.Net;
using GOILauncher.Multiplayer.Shared.Packets;
using LiteNetLib;
using UnityEngine;

namespace GOILauncher.Multiplayer.Client
{
    public class UnityGameClient : MonoBehaviour, IGameClient, IClientEventProcessor
    {
        public static UnityGameClient Instance { get; private set; }
        private readonly INetClient _client = new NetClient();
        private readonly IUnitySceneManager _unitySceneManager = UnitySceneManager.Init();
        private IGamePlayerManager _gamePlayerManager;
        public event EventHandler<ServerConnectedEventArgs> ServerConnected;
        public event EventHandler<ServerDisconnectedEventArgs> ServerDisconnected;
        public event EventHandler<PlayerConnectedEventArgs> PlayerConnected;
        public event EventHandler<PlayerDisconnectedEventArgs> PlayerDisconnected;
        public event EventHandler<PlayerEnteredEventArgs> PlayerEntered;
        public event EventHandler<PlayerLeftEventArgs> PlayerLeft;
        public event EventHandler<PlayerMovedEventArgs> PlayerMoved;
        public event EventHandler<ChatMessageReceivedEventArgs> ChatMessageReceived;
        public Dictionary<int, ClientPlayer> Players { get; } = new Dictionary<int, ClientPlayer>();
        public int Ping => _client.Ping;

        private readonly ClientPlayer _localPlayer = new ClientPlayer
        {
            Platform = Application.platform
        };
        private string _host;
        private int _port;
        private int _retryCount;
        public static UnityGameClient Init()
        {
            if (Instance != null)
                return Instance;
            var unityGameClient = new GameObject(nameof(UnityGameClient), typeof(UnityGameClient));
            DontDestroyOnLoad(unityGameClient);
            return Instance = unityGameClient.GetComponent<UnityGameClient>();
        }

        public void Awake()
        {
            _gamePlayerManager = GamePlayerManager.Init(this, _localPlayer);
            _gamePlayerManager.PlayerMoved += OnPlayerMoved;
            var listener = _client.Listener;
            _client.ServerDisconnected += OnServerDisconnected;
            listener.NetworkReceiveEvent += OnNetworkReceived;
            _unitySceneManager.GameSceneEntered += OnGameSceneEntered;
            _unitySceneManager.GameSceneLeft += OnGameSceneLeft;
        }

        public void Update()
        {
            _client.Poll();
        }

        public void Start(string host, int port, string playerName)
        {
            if (_client.IsRunning)
                return;
            _host = host;
            _port = port;
            _localPlayer.Name = playerName;
            _localPlayer.IsInGame = _unitySceneManager.IsInGame;
            _client.Start(host, port);
        }

        public void Reset()
        {
            Players.Clear();
        }

        public void Stop()
        {
            if (_client.IsRunning)
            {
                _client.Stop();
                ServerDisconnected?.Invoke(this, new ServerDisconnectedEventArgs
                {
                    Reason = "主动断开连接"
                });
            }
            Reset();
        }

        public void SendChatMessage(string message)
        {
            if (!_client.IsRunning)
                return;
            var chatMessagePacket = new ChatMessagePacket
            {
                PlayerId = _localPlayer.Id,
                Message = message
            };
            _client.Send(chatMessagePacket, DeliveryMethod.ReliableOrdered);
        }


        public void OnNetworkReceived(NetPeer server, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            var packetType = reader.GetPacketType();
            switch (packetType)
            {
                case PacketType.ServerConnected:
                    var serverConnectedPacket = reader.Get<ServerConnectedPacket>();
                    OnServerConnected(server, serverConnectedPacket);
                    break;
                case PacketType.PlayerConnected:
                    var playerConnectedPacket = reader.Get<PlayerConnectedPacket>();
                    OnPlayerConnected(server, playerConnectedPacket);
                    break;
                case PacketType.PlayerDisconnected:
                    var playerDisconnectedPacket = reader.Get<PlayerDisconnectedPacket>();
                    OnPlayerDisconnected(server, playerDisconnectedPacket);
                    break;
                case PacketType.PlayerEntered:
                    var playerEnteredPacket = reader.Get<PlayerEnteredPacket>();
                    OnPlayerEntered(server, playerEnteredPacket);
                    break;
                case PacketType.PlayerLeft:
                    var playerLeftPacket = reader.Get<PlayerLeftPacket>();
                    OnPlayerLeft(server, playerLeftPacket);
                    break;
                case PacketType.PlayerMove:
                    var playerMovePacket = reader.Get<PlayerMovePacket>();
                    OnPlayerMoved(server, playerMovePacket);
                    break;
                case PacketType.ChatMessage:
                    var chatMessagePacket = reader.Get<ChatMessagePacket>();
                    OnChatMessageReceived(server, chatMessagePacket);
                    break;
                default:
                    Console.WriteLine("Unknown packet type");
                    break;
            }
            reader.Recycle();
        }


        public void OnServerConnected(NetPeer peer, ServerConnectedPacket packet)
        {
            var playerId = packet.PlayerId;
            _localPlayer.Id = playerId;
            Players.Add(playerId, _localPlayer);
            var playerConnectedPacket = new PlayerConnectedPacket
            {
                PlayerId = playerId,
                PlayerName = _localPlayer.Name,
                Platform = _localPlayer.Platform,
                IsInGame = _unitySceneManager.IsInGame,
                InitMove = _unitySceneManager.IsInGame ? _localPlayer.Player.GetMove() : new Move()
            };
            _client.Send(playerConnectedPacket, DeliveryMethod.ReliableOrdered);
            ServerConnected?.Invoke(this, new ServerConnectedEventArgs
            {
                PlayerId = playerId
            });
            Debug.Log($"成功连接到服务器:{peer.EndPoint.Address}:{peer.EndPoint.Port}");
        }

        private void OnServerDisconnected(object sender, GOILauncher.Multiplayer.Shared.Events.ServerDisconnectedEventArgs e)
        {
            Stop();
            if (_retryCount < ConnectionConstants.MaxRetries)
            {
                _retryCount++;
                _client.Start(_host, _port);
                Debug.LogError($"与服务器断开连接：{e.Reason}，第{_retryCount}次重连...");
            }
            else
                Debug.LogError($"与服务器断开连接：{e.Reason}");
            ServerDisconnected?.Invoke(this, new ServerDisconnectedEventArgs
            {
                Reason = e.Reason
            });
        }

        private void OnPlayerConnected(NetPeer _, PlayerConnectedPacket packet)
        {
            var playerId = packet.PlayerId;
            var player = new ClientPlayer
            {
                Id = playerId,
                Name = packet.PlayerName,
                Platform = packet.Platform,
                IsInGame = packet.IsInGame,
                InitMove = packet.InitMove
            };
            Players.Add(playerId, player);
            PlayerConnected?.Invoke(this, new PlayerConnectedEventArgs
            {
                Player = player
            });
            Debug.Log($"[{playerId}][{packet.Platform}]{packet.PlayerName}加入房间。");
        }
        private void OnPlayerDisconnected(NetPeer _, PlayerDisconnectedPacket packet)
        {
            var playerId = packet.PlayerId;
            if (Players.TryGetValue(playerId, out var player))
            {
                Players.Remove(playerId);
                PlayerDisconnected?.Invoke(this, new PlayerDisconnectedEventArgs
                {
                    Player = player
                });
            }
        }
        private void OnPlayerEntered(NetPeer _, PlayerEnteredPacket packet)
        {
            var playerId = packet.PlayerId;
            var player = Players[playerId];
            player.IsInGame = true;
            player.InitMove = packet.InitMove;
            PlayerEntered?.Invoke(this, new PlayerEnteredEventArgs
            {
                Player = player
            });
            Debug.Log($"[{player.Name}]进入游戏。");
        }


        private void OnPlayerLeft(NetPeer _, PlayerLeftPacket playerLeftPacket)
        {
            var playerId = playerLeftPacket.PlayerId;
            var player = Players[playerId];
            player.IsInGame = false;
            PlayerLeft?.Invoke(this, new PlayerLeftEventArgs
            {
                Player = player
            });
            Debug.Log($"[{player.Name}]离开游戏。");
        }

        private void OnPlayerMoved(NetPeer _, PlayerMovePacket playerMovePacket)
        {
            var playerId = playerMovePacket.PlayerId;
            var player = Players[playerId];
            PlayerMoved?.Invoke(this, new PlayerMovedEventArgs
            {
                Player = player,
                Move = playerMovePacket.Move
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
            Debug.Log($"[{playerId}][{player.Name}]: {message}");
        }

        private void OnPlayerMoved(object sender, PlayerMovedEventArgs e)
        {
            var playerMovedPacket = new PlayerMovePacket
            {
                PlayerId = _localPlayer.Id,
                Move = e.Move
            };
            _client.Send(playerMovedPacket, DeliveryMethod.Unreliable);
        }


        private void OnGameSceneEntered(object sender, EventArgs e)
        {
            var playerEnteredPacket = new PlayerEnteredPacket
            {
                PlayerId = _localPlayer.Id,
                InitMove = _localPlayer.Player.GetMove()
            };
            _client.Send(playerEnteredPacket, DeliveryMethod.ReliableOrdered);
        }
        private void OnGameSceneLeft(object sender, EventArgs e)
        {
            var playerLeftPacket = new PlayerLeftPacket
            {
                PlayerId = _localPlayer.Id
            };
            _client.Send(playerLeftPacket, DeliveryMethod.ReliableOrdered);
        }

    }
}
