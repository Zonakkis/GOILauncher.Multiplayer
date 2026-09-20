using System;
using Autofac;
using GOILauncher.Multiplayer.Client.Extensions;
using GOILauncher.Multiplayer.Core;
using GOILauncher.Multiplayer.Core.Extensions;
using GOILauncher.Multiplayer.Server.Extensions;
using GOILauncher.Multiplayer.Unity.Extensions;
using GOILauncher.Multiplayer.Unity.Player;
using GOILauncher.Multiplayer.Unity.Skin;
using NLog.Targets;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GOILauncher.Multiplayer.Unity
{
    /// <summary>
    /// 联机模块的根。只有两个动作：Initialize 建出整张对象图，Dispose 拆掉它。
    /// 没有"已加载但停用"的中间态——关就是拆，开就是重建。
    /// </summary>
    /// <remarks>
    /// 三个门面属性在未加载时返回 null，判据是 _container 这一个字段，不是对象本身。
    /// 这点必须写死：UnityClient / GameManager 是 MonoBehaviour，Destroy 之后托管引用仍然在，
    /// 而 IsConnected 这类纯托管属性不碰 Unity API，照常返回上一轮的旧值；就算走 ==null，
    /// 门面按接口暴露，用的是 object 的引用相等，Unity 那套假 null 根本不参与。
    /// <para>
    /// Initialized / Disposing 标记一次周期的开始和结束。UI 在 Initialized 里取门面并订阅，
    /// 在 Disposing 里退订、清掉缓存的 PlayerView 之类的视图——后者发出时对象图还是活的。
    /// </para>
    /// </remarks>
    public static class MultiplayerUnityCore
    {
        private static IContainer _container;
        private static GameObject _core;

        /// <summary>对象图建好了，可以去读三个门面属性并订阅它们的事件。</summary>
        public static event Action Initialized;

        /// <summary>对象图即将销毁。趁还来得及，退订并丢掉手里缓存的东西。</summary>
        public static event Action Disposing;

        public static bool IsLoaded
        {
            get { return _container != null; }
        }

        public static IGameManager GameManager
        {
            get { return Resolve<IGameManager>(); }
        }

        public static IUnityClient UnityClient
        {
            get { return Resolve<IUnityClient>(); }
        }

        public static IUnityServer UnityServer
        {
            get { return Resolve<IUnityServer>(); }
        }

        /// <summary>
        /// 建起整张对象图。logTarget 是宿主自己的 NLog 落地端（PC 宿主是 BepInEx），
        /// 不给就用 Core 注册的默认值。
        /// </summary>
        /// <returns>有没有加载成功。失败时已经回滚干净，调用方当作没发生过就行。</returns>
        public static bool Initialize(Target logTarget = null)
        {
            if (_container != null)
            {
                Debug.LogWarning("[GOILauncher.Multiplayer] Initialize called while multiplayer is already loaded.");
                return true;
            }

            _core = new GameObject("MultiplayerUnityCore");
            Object.DontDestroyOnLoad(_core);

            try
            {
                var builder = new ContainerBuilder();

                builder
                .RegisterMultiplayerCore().WithServer().WithClient()
                .RegisterGameManager()
                .RegisterUnityClient()
                .RegisterUnityServer()
                .RegisterPlayerInstancePool()
                .RegisterPlayerManager()
                .RegisterPlayerStateSynchronizer()
                .RegisterSkinSynchronizer();

                if (logTarget != null)
                    builder.RegisterInstance(logTarget)
                        .As<Target>()
                        .SingleInstance()
                        .ExternallyOwned();

                _container = builder.Build();
                _container.Resolve<CoreManager>();
                // Unity 适配层由工厂注册创建 GameObject，必须在初始化时显式实例化。
                // 纯 C# 的 Module 实现 IStartable，由容器在 Build() 时自动激活。
                _container.Resolve<IGameManager>();
                _container.Resolve<PlayerStateSynchronizer>();
                _container.Resolve<SkinSynchronizer>();
            }
            catch (Exception ex)
            {
                // 建到一半就抛，不能留半个容器和半个 GameObject 在外头等着被误用。
                Rollback();
                Debug.LogError("[GOILauncher.Multiplayer] Initialize failed: " + ex);
                return false;
            }

            InvokeSafely(Initialized, "an Initialized listener");
            return true;
        }

        /// <summary>
        /// 拆掉整张对象图。没加载、或者已经拆过，都是空操作。
        /// </summary>
        public static void Dispose()
        {
            var container = _container;
            if (container == null)
                return;

            var core = _core;
            // 闸先落下：从这一行起门面属性全是 null，没有人还能拿到这一轮的实例。
            _container = null;
            _core = null;

            TearDown(container, core);
        }

        /// <summary>
        /// 拆一张活着的对象图。和 Dispose 的空判分开写，是因为本方法碰 Unity 引擎对象
        /// （GameObject.SetActive、Object.Destroy），在没有 Unity 运行时的进程里连进来都进不来。
        /// </summary>
        private static void TearDown(IContainer container, GameObject core)
        {
            // 通知要赶在拆之前，UI 得趁对象图还活着退订。
            InvokeSafely(Disposing, "a Disposing listener");

            // Destroy 要到帧末才生效，所以先停掉轮询：不关的话本帧的 Update 还会 Poll 一次，
            // 到的包能把刚清掉的远端实例重新建出来。
            if (core != null)
                core.SetActive(false);

            // 顺序就三条：先断网，再收场景里的玩家实例，最后才是容器。
            // 反过来会让断开途中到达的包重新造实例，或者让销毁后的组件被服务回调碰到。
            Try(container.Resolve<IUnityClient>().Disconnect, "disconnecting the client");
            Try(container.Resolve<IUnityServer>().Stop, "stopping the embedded server");
            Try(() => (container.Resolve<IPlayerManager>() as IDisposable).Dispose(), "releasing player instances");

            // _core 底下挂着 GameManager、UnityClient、UnityServer 和两个同步器，一次带走。
            // GameManager.OnDestroy 负责退订 sceneLoaded 并销毁自己造的 PlayerPrefab 克隆。
            if (core != null)
                Object.Destroy(core);

            // 容器负责它自己创建的那些服务：ClientService / ServerService 连同底下的
            // NetManager（Dispose 里会 Stop，收下网络线程）。日志 target 是外部所有的，
            // 不会被这次销毁掉，否则下一轮 Initialize 复用它时就是个死对象。
            Try(container.Dispose, "disposing the container");
        }

        private static void Rollback()
        {
            var container = _container;
            var core = _core;
            _container = null;
            _core = null;

            if (core != null)
                Object.Destroy(core);
            if (container != null)
                Try(container.Dispose, "rolling back a half-built container");
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

        private static T Resolve<T>()
        {
            var container = _container;
            return container == null ? default(T) : container.Resolve<T>();
        }

        private static void Try(Action action, string what)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                // 关到一半失败也得继续往下关：宁可留一点垃圾，也不能让整张图卡在那儿拆不动。
                Debug.LogError("[GOILauncher.Multiplayer] Failed at " + what + ": " + ex);
            }
        }

        private static void InvokeSafely(Action handlers, string who)
        {
            if (handlers == null)
                return;

            foreach (Action handler in handlers.GetInvocationList())
            {
                try
                {
                    handler();
                }
                catch (Exception ex)
                {
                    Debug.LogError("[GOILauncher.Multiplayer] " + who + " threw: " + ex);
                }
            }
        }
    }
}
