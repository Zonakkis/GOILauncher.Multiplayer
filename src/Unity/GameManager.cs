using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Unity.Events;
using GOILauncher.Multiplayer.Unity.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace GOILauncher.Multiplayer.Unity
{

    public class GameManager : MonoBehaviour, IGameManager
    {
        public IEventBus EventBus { get; set; }
        public ILogger<GameManager> Logger { get; set; }
        private string _currentSceneName;
        public bool IsInGame => _currentSceneName == GOIScene.Mian.ToString();
        public GameObject Player { get; private set; }
        public GameObject PlayerPrefab { get; private set; }

        public void Awake()
        {
            // 存场景名字符串而非 Scene 结构体：Scene 在场景卸载后 name 会失效（返回空串），导致 IsInGame 误判
            _currentSceneName = UnitySceneManager.GetActiveScene().name;
            UnitySceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void Start()
        {
            // 插件可能在 Mian 已经加载完成后才初始化，此时收不到当前场景的 sceneLoaded 事件。
            if (IsInGame && Player == null)
            {
                OnGameStarted();
                EventBus.Publish(new GameStartedEvent());
            }

            // Start 在属性注入（InjectProperties）之后执行，Logger 此时可用；Awake 里打日志会因 Logger 未注入而抛异常
            Logger.Info("GameManager initialized, current scene: {SceneName}, isInGame: {IsInGame}", _currentSceneName, IsInGame);
        }

        public void OnDestroy()
        {
            // 检测 GameManager 是否被销毁重建（不依赖注入，用 Debug.Log）
            Debug.Log("[GOILauncher.Multiplayer] GameManager destroyed.");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var wasInGame = IsInGame;
            _currentSceneName = scene.name;
            Logger.Info("Scene loaded: {SceneName}, wasInGame: {WasInGame}, isInGame: {IsInGame}", scene.name, wasInGame, IsInGame);
            if (IsInGame)
            {
                if (!wasInGame)
                {
                    OnGameStarted();
                    EventBus.Publish(new GameStartedEvent());
                }
                else
                {
                    OnGameRestarted();
                    EventBus.Publish(new GameRestartedEvent());
                }
            }
            else if (wasInGame)
            {
                OnGameQuit();
                EventBus.Publish(new GameQuitEvent());
            }
        }

        private void OnGameStarted()
        {
            RefreshGameResources();
        }

        private void OnGameRestarted()
        {
            RefreshGameResources();
        }

        private void OnGameQuit()
        {
            Player = null;
        }

        private void RefreshGameResources()
        {
            Player = GameObject.Find("Player");
            PlayerPrefab = CreatePlayerPrefab();
        }

        private GameObject CreatePlayerPrefab()
        {
            var player = GameObject.Find("Player");
            if (player == null)
            {
                Logger.Warn("Player GameObject not found in the scene.");
                return null;
            }

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
            return playerPrefab;
        }
    }
}
