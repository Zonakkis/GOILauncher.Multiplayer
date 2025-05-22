using System;
using System.Collections;
using System.Collections.Generic;
using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Client.Interfaces;
using GOILauncher.Multiplayer.Client.Pools;
using GOILauncher.Multiplayer.Shared.Constants;
using GOILauncher.Multiplayer.Shared.Interfaces;
using UnityEngine;

namespace GOILauncher.Multiplayer.Client.Game
{
    public class GamePlayerManager : MonoBehaviour, IGamePlayerManager
    {
        public static GamePlayerManager Instance { get; private set; }

        private IClientEventListener _listener;
        public IClientEventListener Listener
        {
            set
            {
                _listener = value;
                _listener.ServerDisconnected += OnServerDisonnected;
                _listener.PlayerConnected += OnPlayerConnected;
                _listener.PlayerDisconnected += OnPlayerDisconnected;
                _listener.PlayerEntered += OnPlayerEntered;
                _listener.PlayerLeft += OnPlayerLeft;
                _listener.PlayerMoved += OnPlayerMoved;
            }
        }

        private readonly IUnitySceneManager _unitySceneManager = UnitySceneManager.Init();
        public event EventHandler<PlayerMovedEventArgs> PlayerMoved;

        private readonly Dictionary<int, ClientPlayer> _inGamePlayers = new Dictionary<int, ClientPlayer>();
        private ClientPlayer LocalPlayer { get; set; }
        private PlayerPool _playerPool;
        private float _timer;
        public GamePlayerManager()
        {
            _unitySceneManager.GameSceneEntered += OnGameSceneEntered;
            _unitySceneManager.GameSceneLeft += OnGameSceneLeft;
            _unitySceneManager.GameRestarted += OnGameRestarted;
        }

        public static GamePlayerManager Init(IClientEventListener listener, ClientPlayer localPlayer)
        {
            if (Instance != null)
                return Instance;
            var gamePlayerManager = new GameObject(nameof(GamePlayerManager), typeof(GamePlayerManager));
            DontDestroyOnLoad(gamePlayerManager);
            Instance = gamePlayerManager.GetComponent<GamePlayerManager>();
            Instance.Listener = listener;
            Instance.LocalPlayer = localPlayer;
            return Instance;
        }

        public void Start()
        {
            if (_unitySceneManager.IsInGame)
                CreatePlayerPrefab();
        }

        public void Update()
        {
            if (!_unitySceneManager.IsInGame)
                return;
            _timer += Time.deltaTime;
            if (_timer > GameConstants.FrameTime)
            {
                _timer -= GameConstants.FrameTime;
                PlayerMoved?.Invoke(this, new PlayerMovedEventArgs
                {
                    Player = LocalPlayer,
                    Move = LocalPlayer.GamePlayer.GetMove()
                });
            }
        }

        public void ResetGame()
        {
            if (_unitySceneManager.IsInGame)
                foreach (var player in _inGamePlayers.Values)
                    StartCoroutine(RemoveRemotePlayer(player));
            _inGamePlayers.Clear();
        }

        private void OnServerDisonnected(object sender, ServerDisconnectedEventArgs e)
        {
            ResetGame();
        }

        private void OnPlayerConnected(object sender, PlayerConnectedEventArgs e)
        {
            var player = e.Player;
            if (player.IsInGame)
            {
                _inGamePlayers.Add(player.Id, player);
                if (_unitySceneManager.IsInGame)
                    StartCoroutine(CreateRemotePlayer(player));
            }
        }
        private void OnPlayerDisconnected(object sender, PlayerDisconnectedEventArgs e)
        {
            var player = e.Player;
            if (_inGamePlayers.Remove(player.Id) && _unitySceneManager.IsInGame)
                StartCoroutine(RemoveRemotePlayer(player));
        }
        private void OnPlayerEntered(object sender, PlayerEnteredEventArgs e)
        {
            var player = e.Player;
            _inGamePlayers.Add(player.Id, player);
            if (_unitySceneManager.IsInGame)
                StartCoroutine(CreateRemotePlayer(player));
        }
        private void OnPlayerLeft(object sender, PlayerLeftEventArgs e)
        {
            var player = e.Player;
            var playerId = player.Id;
            if (_inGamePlayers.Remove(playerId) && _unitySceneManager.IsInGame)
                StartCoroutine(RemoveRemotePlayer(player));
        }

        private void OnPlayerMoved(object sender, PlayerMovedEventArgs e)
        {
            if (_unitySceneManager.IsInGame && _inGamePlayers.TryGetValue(e.Player.Id, out var player))
                player.GamePlayer?.SetNextMove(e.Move);
        }



        private void OnGameSceneEntered(object sender, EventArgs e)
        {
            CreatePlayerPrefab();
            foreach (var player in _inGamePlayers.Values)
                StartCoroutine(CreateRemotePlayer(player));
        }

        private void OnGameSceneLeft(object sender, EventArgs e)
        {

        }

        private void OnGameRestarted(object sender, EventArgs e)
        {
            CreatePlayerPrefab();
            foreach (var player in _inGamePlayers.Values)
                StartCoroutine(CreateRemotePlayer(player));
        }

        private void CreatePlayerPrefab()
        {
            //开启交互碰撞箱的方法：player,tip,sides的layer设为Terrain
            Time.timeScale = 0;
            var player = GameObject.Find("Player");
            var playerPrefab = Instantiate(player);
            playerPrefab.name = "PlayerPrefab";
            Destroy(playerPrefab.GetComponent<Saviour>());
            Destroy(playerPrefab.GetComponent<Screener>());
            Destroy(playerPrefab.GetComponent<MipmapBias>());
            Destroy(playerPrefab.GetComponent<PlayerControl>());
            Destroy(playerPrefab.GetComponentInChildren<PotSounds>());
            Destroy(playerPrefab.GetComponentInChildren<PlayerSounds>());
            Destroy(playerPrefab.GetComponentInChildren<HammerCollisions>());
            Destroy(playerPrefab.transform.Find("PotCollider/Sensor").gameObject);
            foreach (var camera in playerPrefab.GetComponentsInChildren<Camera>())
                Destroy(camera);
            foreach (var rigidBody2D in playerPrefab.GetComponentsInChildren<Rigidbody2D>())
                rigidBody2D.isKinematic = true;
            foreach (var collider in playerPrefab.GetComponentsInChildren<Collider2D>())
                Destroy(collider);
            playerPrefab.SetActive(false);
            _playerPool = new PlayerPool(playerPrefab);
            LocalPlayer.GamePlayer = player.AddComponent<LocalPlayer>();
        }

        private IEnumerator CreateRemotePlayer(ClientPlayer player)
        {
            if (player == null)
                yield break;
            yield return null;
            var remotePlayerGameObject = _playerPool.Get();
            remotePlayerGameObject.name = $"[{player.Id}][{player.Platform}]{player.Name}";
            var remotePlayer = remotePlayerGameObject.AddComponent<RemotePlayer>();
            remotePlayer.Id = player.Id;
            remotePlayer.Name = player.Name;
            remotePlayer.Platform = player.Platform;
            remotePlayer.LocalPlayer = LocalPlayer.GamePlayer.Player;
            remotePlayer.ApplyMove(player.Move);
            player.GamePlayer = remotePlayer;
            Debug.Log($"生成远程玩家：[{player.Id}][{player.Platform}]{player.Name}");
        }

        private IEnumerator RemoveRemotePlayer(ClientPlayer player)
        {
            if (player?.GamePlayer != null)
            {
                _playerPool.Release(player.GamePlayer.Player.gameObject);
                player.GamePlayer = null;
            }
            yield break;
        }
    }
}
