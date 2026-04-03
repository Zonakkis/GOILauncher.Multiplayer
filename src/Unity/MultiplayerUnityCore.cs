using System;
using Autofac;
using GOILauncher.Multiplayer.Client;
using GOILauncher.Multiplayer.Client.Extensions;
using GOILauncher.Multiplayer.Core.Extensions;
using GOILauncher.Multiplayer.Server.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GOILauncher.Multiplayer.Unity
{
    public static class MultiplayerUnityCore
    {
        private static bool _isInitialized = false;
        private static IContainer _container;
        private static GameObject _core;
        public static SceneManager SceneManager => _container.Resolve<SceneManager>();

        public static IUnityClient UnityClient => _container.Resolve<IUnityClient>();

        public static IContainer Initialize(Action<ContainerBuilder> configure = null)
        {
            if (_isInitialized) return _container;
            _isInitialized = true;
            
            _core = new GameObject("MultiplayerUnityCore");
            Object.DontDestroyOnLoad(_core);

            var builder = new ContainerBuilder();

            configure?.Invoke(builder);

            builder
            .RegisterMultiplayerCore().WithServer().WithClient()
            .RegisterSceneManager()
            .RegisterUnityClient(); 

            return _container = builder.Build();
        }

        private static ContainerBuilder RegisterSceneManager(this ContainerBuilder builder)
        {
            builder.Register(ctx =>
            {
                var obj = new GameObject(nameof(SceneManager));
                obj.transform.SetParent(_core.transform);
                var sceneManager = obj.AddComponent<SceneManager>();
                ctx.InjectProperties(sceneManager);
                return sceneManager;
            }).
            As<SceneManager>().
            SingleInstance();
            return builder;
        }

        private static ContainerBuilder RegisterUnityClient(this ContainerBuilder builder)
        {
            builder.Register(ctx =>
            {
                var obj = new GameObject(nameof(UnityClient));
                obj.transform.SetParent(_core.transform);
                var unityClient = obj.AddComponent<UnityClient>();
                ctx.InjectProperties(unityClient);
                return unityClient;
            }).
            As<IUnityClient>().
            SingleInstance();
            return builder;
        }
    }
}
