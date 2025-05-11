using System;
using System.Collections;
using GOILauncher.Multiplayer.Shared.Constants;
using GOILauncher.Multiplayer.Shared.Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GOILauncher.Multiplayer.Shared.Game
{
    public class UnitySceneManager : MonoBehaviour, IUnitySceneManager
    {
        public bool IsInGame { get; private set; }
        public static UnitySceneManager Instance { get; private set; }

        public event EventHandler GameSceneEntered;
        public event EventHandler GameSceneLeft;
        public event EventHandler GameRestarted;

        public static UnitySceneManager Init()
        {
            if (Instance != null)
                return Instance;
            var sceneManager = new GameObject(nameof(UnitySceneManager), typeof(UnitySceneManager));
            DontDestroyOnLoad(sceneManager);
            return Instance = sceneManager.GetComponent<UnitySceneManager>();
        }

        public void Awake()
        {
            var currentScene = SceneManager.GetActiveScene();
            SceneManager.sceneLoaded += OnSceneLoaded;
            IsInGame = currentScene.name == GameConstants.GameSceneName;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var isInGamePrevious = IsInGame;
            IsInGame = scene.name == GameConstants.GameSceneName;
            if (IsInGame)
                StartCoroutine(!isInGamePrevious ? OnGameSceneEntered() : OnGameRestarted());
            else if (isInGamePrevious)
                StartCoroutine(OnGameSceneLeft());
        }

        private IEnumerator OnGameSceneEntered()
        {
            yield return null;
            yield return null;
            GameSceneEntered?.Invoke(this, EventArgs.Empty);
        }

        private IEnumerator OnGameSceneLeft()
        {
            yield return null;
            GameSceneLeft?.Invoke(this, EventArgs.Empty);
        }

        private IEnumerator OnGameRestarted()
        {
            yield return null;
            GameRestarted?.Invoke(this, EventArgs.Empty);
        }
    }
}