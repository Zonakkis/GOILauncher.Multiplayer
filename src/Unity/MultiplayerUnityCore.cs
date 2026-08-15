using System;
using Autofac;
using GOILauncher.Multiplayer.Client.Extensions;
using GOILauncher.Multiplayer.Client.Synchronization;
using GOILauncher.Multiplayer.Core;
using GOILauncher.Multiplayer.Core.Extensions;
using GOILauncher.Multiplayer.Server.Extensions;
using GOILauncher.Multiplayer.Server.Synchronization;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GOILauncher.Multiplayer.Unity
{
    public static class MultiplayerUnityCore
    {
        private static bool _isInitialized = false;
        private static IContainer _container;
        private static GameObject _core;
        public static IGameManager GameManager => _container.Resolve<IGameManager>();

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
            .RegisterGameManager()
            .RegisterUnityClient()
            .RegisterUnityServer()
            .RegisterPlayerInstancePool()
            .RegisterPlayerManager()
            .RegisterPlayerStateSynchronizer();

            configure?.Invoke(builder);

            _container = builder.Build();
            _container.Resolve<CoreManager>();
            _container.Resolve<IGameManager>();
            // PlayerManager 是事件驱动服务，必须立即实例化以完成事件订阅（懒注册不会被自动 Resolve）
            _container.Resolve<IPlayerManager>();
            // 网络包处理 Module 通过构造函数注册回调，必须显式激活。
            _container.Resolve<ClientPlayerStateSync>();
            _container.Resolve<PlayerStateRelay>();
            _container.Resolve<PlayerStateSynchronizer>();
            return _container;
        }

        private static ContainerBuilder RegisterGameManager(this ContainerBuilder builder)
        {
            builder.Register(ctx =>
            {
                var obj = new GameObject(nameof(GameManager));
                obj.transform.SetParent(_core.transform);
                var gameManager = obj.AddComponent<GameManager>();
                ctx.InjectProperties(gameManager);
                return gameManager;
            }).
            As<IGameManager>().
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
                unityClient.Init();
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

        private static ContainerBuilder RegisterPlayerInstancePool(this ContainerBuilder builder)
        {
            builder.RegisterType<PlayerInstancePool>().
            As<IPlayerInstancePool>().
            SingleInstance();
            return builder;
        }

        private static ContainerBuilder RegisterPlayerManager(this ContainerBuilder builder)
        {
            builder.RegisterType<PlayerManager>().
            As<IPlayerManager>().
            SingleInstance().
            OnActivated(e => e.Instance.Init());
            return builder;
        }

        private static ContainerBuilder RegisterPlayerStateSynchronizer(this ContainerBuilder builder)
        {
            builder.Register(ctx =>
            {
                var obj = new GameObject(nameof(PlayerStateSynchronizer));
                obj.transform.SetParent(_core.transform);
                var synchronizer = obj.AddComponent<PlayerStateSynchronizer>();
                ctx.InjectProperties(synchronizer);
                synchronizer.Init();
                return synchronizer;
            })
            .AsSelf()
            .SingleInstance();
            return builder;
        }
    }
}
