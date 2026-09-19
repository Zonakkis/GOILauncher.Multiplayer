using System;
using Autofac;
using GOILauncher.Multiplayer.Client.Extensions;
using GOILauncher.Multiplayer.Core;
using GOILauncher.Multiplayer.Core.Extensions;
using GOILauncher.Multiplayer.Server.Extensions;
using GOILauncher.Multiplayer.Unity.Config;
using GOILauncher.Multiplayer.Unity.Extensions;
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

        /// <summary>
        /// 设置入口。
        /// </summary>
        public static MultiplayerSettings Settings => _container.Resolve<MultiplayerSettings>();

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
            builder.RegisterComponent<GameManager>(gameManager =>
                {
                    gameManager.transform.SetParent(_core.transform);
                }).
                As<IGameManager>()
                .SingleInstance();
            return builder;
        }

        private static ContainerBuilder RegisterUnityClient(this ContainerBuilder builder)
        {
            builder.RegisterComponent<UnityClient>(unityClient =>
                {
                    unityClient.transform.SetParent(_core.transform);
                    unityClient.Init();
                }).
                As<IUnityClient>()
                .SingleInstance();
            return builder;
        }

        private static ContainerBuilder RegisterUnityServer(this ContainerBuilder builder)
        {
            builder.RegisterComponent<UnityServer>(unityServer =>
                {
                    unityServer.transform.SetParent(_core.transform);
                }).
                As<IUnityServer>()
                .SingleInstance();
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
            builder.RegisterComponent<PlayerStateSynchronizer>(playerStateSynchronizer => 
                {
                    playerStateSynchronizer.transform.SetParent(_core.transform);
                }).
                AsSelf()
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
            builder.RegisterComponent<SkinSynchronizer>(skinSynchronizer =>
                {
                    skinSynchronizer.transform.SetParent(_core.transform);
                    skinSynchronizer.Init();
                }).
                AsSelf()
                .SingleInstance();
            return builder;
        }
    }
}
