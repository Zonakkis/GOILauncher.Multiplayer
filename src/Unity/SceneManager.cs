using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Unity.Events;
using GOILauncher.Multiplayer.Unity.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace GOILauncher.Multiplayer.Unity
{
    public class SceneManager : MonoBehaviour
    {
        private static readonly object _lock = new object();
        public static SceneManager Instance { get; private set; }
        public IEventBus EventBus { get; set; }
        private Scene _currentScene;
        public bool IsInGame => _currentScene.name == GOIScene.Mian.ToString();

        public static void Init()
        {
            if (Instance != null) return;
            lock (_lock)
            {
                if (Instance == null)
                {
                    var obj = new GameObject(nameof(SceneManager));
                    Instance = obj.AddComponent<SceneManager>();
                    DontDestroyOnLoad(obj);
                }
            }
        }

        public void Awake()
        {
            _currentScene = UnitySceneManager.GetActiveScene();
            UnitySceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var wasInGame = IsInGame;
            _currentScene = scene;
            if (IsInGame)
            {
                if (!wasInGame)
                    EventBus.Publish(new GameStartedEvent());
                else
                    EventBus.Publish(new GameRestartedEvent());
            }
            else if (wasInGame)
                EventBus.Publish(new GameQuitEvent());
        }
    }
}
