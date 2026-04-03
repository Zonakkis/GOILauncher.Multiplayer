using System;
using Autofac;
using GOILauncher.Multiplayer.Client;
using GOILauncher.Multiplayer.Client.Extensions;
using GOILauncher.Multiplayer.Client.Services;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Extensions;
using GOILauncher.Multiplayer.Server.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GOILauncher.Multiplayer.Unity
{
    public class UnityCore
    {
        private static bool _isInitialized = false;
        private static IContainer _container;
        private static readonly object _sceneManagerLock = new object();
        private static readonly object _unityClientLock = new object();
        private static SceneManager _sceneManager;
        private static UnityClient _unityClient;


        public static IContainer Initialize(Action<ContainerBuilder> configure = null)
        {
            if (_isInitialized) return _container;
            _isInitialized = true;

            var builder = new ContainerBuilder();

            configure?.Invoke(builder);

            builder.RegisterMultiplayerCore().WithServer().WithClient();

            return _container = builder.Build();
        }

        public static SceneManager SceneManager
        {
            get
            {
                if (_sceneManager != null) return _sceneManager;
                lock (_sceneManagerLock)
                {
                    if (_sceneManager == null)
                    {
                        Initialize();
                        var obj = new GameObject(nameof(SceneManager));
                        Object.DontDestroyOnLoad(obj);
                        _sceneManager = obj.AddComponent<SceneManager>();
                        _sceneManager.EventBus = _container.Resolve<IEventBus>();
                    }
                }
                return _sceneManager;
            }
        }

        public static UnityClient UnityClient
        {
            get
            {
                if (_unityClient != null) return _unityClient;
                lock (_unityClientLock)
                {
                    if (_unityClient == null)
                    {
                        Initialize();
                        var obj = new GameObject(nameof(UnityClient));
                        var unityClient = obj.AddComponent<UnityClient>();
                        Object.DontDestroyOnLoad(obj);
                        unityClient.ClientService = _container.Resolve<IClientService>();
                        unityClient.PlayerService = _container.Resolve<IPlayerService>();
                        _unityClient = unityClient;
                    }
                }
                return _unityClient;
            }
        }

    }
}
