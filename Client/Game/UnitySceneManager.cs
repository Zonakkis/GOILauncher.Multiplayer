using System;
using GOILauncher.Multiplayer.Shared.Constants;
using GOILauncher.Multiplayer.Shared.Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GOILauncher.Multiplayer.Client.Game
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
                if (!isInGamePrevious)
                    OnGameSceneEntered();
                else
                    OnGameRestarted();
            else if (isInGamePrevious)
                OnGameSceneLeft();
        }

        private void OnGameSceneEntered()
        {
            GameSceneEntered?.Invoke(this, EventArgs.Empty);
        }

        private void OnGameSceneLeft()
        {
            GameSceneLeft?.Invoke(this, EventArgs.Empty);
        }

        private void OnGameRestarted()
        {
            GameRestarted?.Invoke(this, EventArgs.Empty);
        }
    }
}