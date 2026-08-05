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
        private Scene _currentScene;
        public bool IsInGame => _currentScene.name == GOIScene.Mian.ToString();
        public GameObject Player { get; private set; }
        public GameObject PlayerPrefab { get; private set; }

        public void Awake()
        {
            _currentScene = UnitySceneManager.GetActiveScene();
            UnitySceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var wasInGame = IsInGame;
            _currentScene = scene;
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
