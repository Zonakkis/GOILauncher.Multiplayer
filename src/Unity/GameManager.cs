using System.Collections;
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
        public IClientEventBus EventBus { get; set; }
        public ILogger<GameManager> Logger { get; set; }
        private string _currentSceneName;
        public bool IsInGame => _currentSceneName == GOIScene.Mian.ToString();
        public GameObject Player { get; private set; }
        public GameObject PlayerPrefab { get; private set; }
        public GameObject Cursor { get; private set; }
        private Coroutine _refreshRoutine;

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
                BeginRefreshGameResources(false);
            }

            // Start 在属性注入（InjectProperties）之后执行，Logger 此时可用；Awake 里打日志会因 Logger 未注入而抛异常
            Logger.Info("GameManager initialized, current scene: {SceneName}, isInGame: {IsInGame}", _currentSceneName, IsInGame);
        }

        public void OnDestroy()
        {
            // sceneLoaded 是静态事件，不会因为本组件被销毁就自己松开：不退订的话，
            // 下一次加载场景就会打在已销毁的组件上。关掉联机走的就是这条路。
            UnitySceneManager.sceneLoaded -= OnSceneLoaded;
            StopRefreshGameResources();

            // PlayerPrefab 是本类 Instantiate 出来的场景克隆，不在 _core 底下，得自己收掉。
            if (PlayerPrefab != null)
                Destroy(PlayerPrefab);

            PlayerPrefab = null;
            Player = null;
            Cursor = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var wasInGame = IsInGame;
            _currentSceneName = scene.name;
            Logger.Info("Scene loaded: {SceneName}, wasInGame: {WasInGame}, isInGame: {IsInGame}", scene.name, wasInGame, IsInGame);
            if (IsInGame)
            {
                BeginRefreshGameResources(wasInGame);
            }
            else if (wasInGame)
            {
                StopRefreshGameResources();
                OnGameQuit();
                EventBus.Publish(new GameQuitEvent());
            }
        }

        private void OnGameQuit()
        {
            Player = null;
        }

        /// <summary>
        /// 刷新游戏资源，并把 PlayerPrefab 的公布和进入游戏事件推迟一帧。
        /// Destroy 组件是延迟到本帧末才真正生效的，同一帧就把模板交出去的话，
        /// PlayerInstancePool 预热克隆到的还是没删干净的副本（进游戏看着像只是被禁用），
        /// 所以要等一帧让 Destroy 落地后再放出模板、广播事件。
        /// </summary>
        private void BeginRefreshGameResources(bool wasInGame)
        {
            StopRefreshGameResources();
            _refreshRoutine = StartCoroutine(RefreshGameResources(wasInGame));
        }

        private void StopRefreshGameResources()
        {
            if (_refreshRoutine != null)
            {
                StopCoroutine(_refreshRoutine);
                _refreshRoutine = null;
            }
        }

        private IEnumerator RefreshGameResources(bool wasInGame)
        {
            Player = GameObject.Find("Player");
            Cursor = GameObject.Find("Cursor");
            var playerPrefab = CreatePlayerPrefab();

            // 等一帧，让 CreatePlayerPrefab 里排队的 Destroy 在帧末真正执行完
            yield return null;

            _refreshRoutine = null;
            PlayerPrefab = playerPrefab;
            if (wasInGame)
            {
                EventBus.Publish(new GameRestartedEvent());
            }
            else
            {
                EventBus.Publish(new GameStartedEvent());
            }
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
            // 刚体只改成运动学，一个都不能删：LocalPlayer.TeleportTo 靠下标配对本地玩家和
            // 远端实例两边的 Rigidbody2D，删掉任何一个（或删掉带刚体的子物体）传送就会静默失效。
            foreach (var rigidBody2D in playerPrefab.GetComponentsInChildren<Rigidbody2D>())
                rigidBody2D.isKinematic = true;
            foreach (var collider in playerPrefab.GetComponentsInChildren<Collider2D>())
                Destroy(collider);   
            playerPrefab.SetActive(false);
            return playerPrefab;
        }
    }
}
