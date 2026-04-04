using System;
using Autofac;
using GOILauncher.Multiplayer.Client;
using GOILauncher.Multiplayer.Client.Extensions;
using GOILauncher.Multiplayer.Core;
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
        public static IUnityServer UnityServer => _container.Resolve<IUnityServer>();

        public static IContainer Initialize(Action<ContainerBuilder> configure = null)
        {
            if (_isInitialized) return _container;
            _isInitialized = true;

            _core = new GameObject("MultiplayerUnityCore");
            Object.DontDestroyOnLoad(_core);

            var builder = new ContainerBuilder();

            builder
            .RegisterMultiplayerCore().WithServer().WithClient()
            .RegisterSceneManager()
            .RegisterUnityClient()
            .RegisterUnityServer();

            configure?.Invoke(builder);

            _container = builder.Build();
            _container.Resolve<CoreManager>();
            return _container;
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

        private static ContainerBuilder RegisterUnityServer(this ContainerBuilder builder)
        {
            builder.Register(ctx =>
            {
                var obj = new GameObject(nameof(UnityServer));
                obj.transform.SetParent(_core.transform);
                var unityServer = obj.AddComponent<UnityServer>();
                ctx.InjectProperties(unityServer);
                return unityServer;
            }).
            As<IUnityServer>().
            SingleInstance();
            return builder;
        }
    }
}
