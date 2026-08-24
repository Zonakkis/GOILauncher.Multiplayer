using System;
using Autofac;
using GOILauncher.Multiplayer.Client.Extensions;
using GOILauncher.Multiplayer.Core;
using GOILauncher.Multiplayer.Core.Extensions;
using GOILauncher.Multiplayer.Server.Extensions;
using GOILauncher.Multiplayer.Unity.Config;
using GOILauncher.Multiplayer.Unity.Player;
using GOILauncher.Multiplayer.Unity.Skin;
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
            .RegisterMultiplayerSettings()
            .RegisterPlayerInstancePool()
            .RegisterPlayerManager()
            .RegisterPlayerStateSynchronizer()
            .RegisterSkinSynchronizer();

            configure?.Invoke(builder);

            _container = builder.Build();
            _container.Resolve<CoreManager>();
            // Unity 适配层由工厂注册创建 GameObject，必须在初始化时显式实例化。
            // 纯 C# 的 Module 实现 IStartable，由容器在 Build() 时自动激活。
            _container.Resolve<IGameManager>();
            _container.Resolve<PlayerStateSynchronizer>();
            _container.Resolve<SkinSynchronizer>();
            // 没有任何人依赖它，它靠构造时订阅设置变更生效，所以必须显式解析一次。
            _container.Resolve<MultiplayerLifecycleController>();
            return _container;
        }

        private static ContainerBuilder RegisterMultiplayerSettings(this ContainerBuilder builder)
        {
            builder.RegisterType<FileSettingsStore>()
                .As<ISettingsStore>()
                .SingleInstance();
            builder.RegisterType<MultiplayerSettings>()
                .AsSelf()
                .As<IMultiplayerState>()
                .SingleInstance();
            builder.RegisterType<MultiplayerLifecycleController>()
                .AsSelf()
                .SingleInstance();
            return builder;
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
            As<IStartable>().
            SingleInstance();
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
        private static ContainerBuilder RegisterSkinSynchronizer(this ContainerBuilder builder)
        {
            builder.RegisterType<VanillaPotTexture>()
                .AsSelf()
                .SingleInstance();
            builder.RegisterType<LocalSkinReader>()
                .AsSelf()
                .SingleInstance();
            builder.Register(ctx =>
            {
                var obj = new GameObject(nameof(SkinSynchronizer));
                obj.transform.SetParent(_core.transform);
                var synchronizer = obj.AddComponent<SkinSynchronizer>();
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
