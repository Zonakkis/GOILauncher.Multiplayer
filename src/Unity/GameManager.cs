using GOILauncher.Multiplayer.Core.Event;
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
        private Scene _currentScene;
        public bool IsInGame => _currentScene.name == GOIScene.Mian.ToString();
        public GameObject Player { get; private set; }

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
        }
    }
}
