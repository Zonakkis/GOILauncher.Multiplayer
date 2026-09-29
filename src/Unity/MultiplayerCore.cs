using System;
using System.Runtime.CompilerServices;
using Autofac;
using GOILauncher.Multiplayer.Client.Extensions;
using GOILauncher.Multiplayer.Core;
using GOILauncher.Multiplayer.Core.Extensions;
using GOILauncher.Multiplayer.Server.Extensions;
using GOILauncher.Multiplayer.Unity.Extensions;
using GOILauncher.Multiplayer.Unity.Opening;
using GOILauncher.Multiplayer.Unity.Player;
using GOILauncher.Multiplayer.Unity.Skin;
using NLog.Targets;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GOILauncher.Multiplayer.Unity
{
    /// <summary>
    /// 联机模块的根。<see cref="Initialize"/> 建出整张对象图并返回一个实例，<see cref="Dispose"/>
    /// 拆掉它。没有"已加载但停用"的中间态——关就是拆，开就是重建。宿主持有返回的实例；
    /// 手里的实例为 null，就代表这一轮没加载。
    /// </summary>
    /// <remarks>
    /// 三个门面属性在拆除后返回 null，判据是 _container 这一个字段，不是对象本身。
    /// 这点必须写死：MultiplayerClient / GameManager 是 MonoBehaviour，Destroy 之后托管引用仍然在，
    /// 而 IsConnected 这类纯托管属性不碰 Unity API，照常返回上一轮的旧值；就算走 ==null，
    /// 门面按接口暴露，用的是 object 的引用相等，Unity 那套假 null 根本不参与。
    /// </remarks>
    public sealed class MultiplayerCore
    {
        private IContainer _container;
        private GameObject _core;

        private MultiplayerCore(IContainer container, GameObject core)
        {
            _container = container;
            _core = core;
        }

        public IGameManager GameManager => Resolve<IGameManager>();

        public IMultiplayerClient MultiplayerClient => Resolve<IMultiplayerClient>();

        public IMultiplayerServer MultiplayerServer => Resolve<IMultiplayerServer>();

        /// <summary>
        /// 建起整张对象图并返回实例。<paramref name="configure"/> 在一个带默认值的
        /// <see cref="MultiplayerOptions"/> 上改需要改的项（日志落地端等）；不给就全默认。
        /// </summary>
        /// <returns>加载好的实例；失败时已回滚干净并返回 null，调用方当作没发生过。</returns>
        public static MultiplayerCore Initialize(Action<MultiplayerOptions> configure = null)
        {
            var options = new MultiplayerOptions();
            configure?.Invoke(options);

            var core = new GameObject("MultiplayerCore");
            Object.DontDestroyOnLoad(core);

            IContainer container = null;
            try
            {
                var builder = new ContainerBuilder();

                builder
                    .RegisterMultiplayerCore().WithServer().WithClient()
                    .RegisterGameManager(core)
                    .RegisterMultiplayerClient(core)
                    .RegisterMultiplayerServer(core)
                    .RegisterPlayerInstancePool()
                    .RegisterPlayerManager()
                    .RegisterPlayerStateSynchronizer(core)
                    .RegisterSkinSynchronizer(core)
                    .RegisterOpeningSynchronizer(core);

                // options.LogTarget 恒非空（默认是共享控制台落地端），所以无条件注册它，
                // 盖过 RegisterMultiplayerCore 里的默认。ExternallyOwned：它挂在 NLog 全局
                // 配置上，容器销毁不能连它一起 dispose，否则下一轮复用到的是死对象。
                builder.RegisterInstance(options.LogTarget)
                    .As<Target>()
                    .SingleInstance()
                    .ExternallyOwned();

                container = builder.Build();
                container.Resolve<CoreManager>();
                // Unity 适配层由工厂注册创建 GameObject，必须在初始化时显式实例化。
                // 纯 C# 的 Module 实现 IStartable，由容器在 Build() 时自动激活。
                container.Resolve<IGameManager>();
                container.Resolve<PlayerStateSynchronizer>();
                container.Resolve<SkinSynchronizer>();
                container.Resolve<OpeningSynchronizer>();
            }
            catch (Exception ex)
            {
                // 建到一半就抛，不能留半个容器和半个 GameObject 在外头等着被误用。
                if (core != null)
                    Object.Destroy(core);
                if (container != null)
                    Try(container.Dispose, "rolling back a half-built container");
                Debug.LogError("[GOILauncher.Multiplayer] Initialize failed: " + ex);
                return null;
            }

            return new MultiplayerCore(container, core);
        }

        /// <summary>
        /// 拆掉整张对象图。已经拆过就是空操作。拆完三个门面属性一律返回 null。
        /// </summary>
        public void Dispose()
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
        /// 拆一张活着的对象图。和 Dispose 的空判分开写、并标 NoInlining，是因为本方法碰 Unity
        /// 引擎对象（GameObject.SetActive、Object.Destroy），这些 ECall 在没有 Unity 运行时的
        /// 进程里连方法都编译不进来；分开且不内联，Dispose 的空判才挡得住。同理见 PlayerManager。
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void TearDown(IContainer container, GameObject core)
        {
            // Destroy 要到帧末才生效，所以先停掉轮询：不关的话本帧的 Update 还会 Poll 一次，
            // 到的包能把刚清掉的远端实例重新建出来。
            if (core != null)
                core.SetActive(false);

            // 顺序就三条：先断网，再收场景里的玩家实例，最后才是容器。
            // 反过来会让断开途中到达的包重新造实例，或者让销毁后的组件被服务回调碰到。
            Try(container.Resolve<IMultiplayerClient>().Disconnect, "disconnecting the client");
            Try(container.Resolve<IMultiplayerServer>().Stop, "stopping the embedded server");
            Try(() => (container.Resolve<IPlayerManager>() as IDisposable).Dispose(), "releasing player instances");

            // core 底下挂着 GameManager、MultiplayerClient、MultiplayerServer 和各同步器，一次带走。
            // GameManager.OnDestroy 负责退订 sceneLoaded 并销毁自己造的 PlayerPrefab 克隆。
            if (core != null)
                Object.Destroy(core);

            // 容器负责它自己创建的那些服务：ClientService / ServerService 连同底下的
            // NetManager（Dispose 里会 Stop，收下网络线程）。日志 target 是外部所有的，
            // 不会被这次销毁掉，否则下一轮 Initialize 复用它时就是个死对象。
            Try(container.Dispose, "disposing the container");
        }

        private T Resolve<T>()
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
    }

    /// <summary>
    /// MultiplayerCore 组图用的注册扩展。因为 MultiplayerCore 现在是实例类，扩展方法不能挂在它
    /// 身上，单独放这个静态类里；每个方法把 core 这个 GameObject 收进来，替代原先闭包捕获的静态字段。
    /// </summary>
    internal static class MultiplayerCoreRegistrations
    {
        internal static ContainerBuilder RegisterGameManager(this ContainerBuilder builder, GameObject core)
        {
            builder.RegisterComponent<GameManager>(gameManager =>
                {
                    gameManager.transform.SetParent(core.transform);
                }).
                As<IGameManager>()
                .SingleInstance();
            return builder;
        }

        internal static ContainerBuilder RegisterMultiplayerClient(this ContainerBuilder builder, GameObject core)
        {
            builder.RegisterComponent<MultiplayerClient>(multiplayerClient =>
                {
                    multiplayerClient.transform.SetParent(core.transform);
                    multiplayerClient.Init();
                }).
                As<IMultiplayerClient>()
                .SingleInstance();
            return builder;
        }

        internal static ContainerBuilder RegisterMultiplayerServer(this ContainerBuilder builder, GameObject core)
        {
            builder.RegisterComponent<MultiplayerServer>(multiplayerServer =>
                {
                    multiplayerServer.transform.SetParent(core.transform);
                }).
                As<IMultiplayerServer>()
                .SingleInstance();
            return builder;
        }

        internal static ContainerBuilder RegisterPlayerInstancePool(this ContainerBuilder builder)
        {
            builder.RegisterType<PlayerInstancePool>().
            As<IPlayerInstancePool>().
            SingleInstance();
            return builder;
        }

        internal static ContainerBuilder RegisterPlayerManager(this ContainerBuilder builder)
        {
            builder.RegisterType<PlayerManager>().
            As<IPlayerManager>().
            As<IStartable>().
            SingleInstance();
            return builder;
        }

        internal static ContainerBuilder RegisterPlayerStateSynchronizer(this ContainerBuilder builder, GameObject core)
        {
            builder.RegisterComponent<PlayerStateSynchronizer>(playerStateSynchronizer =>
                {
                    playerStateSynchronizer.transform.SetParent(core.transform);
                    playerStateSynchronizer.Init();
                }).
                AsSelf()
                .SingleInstance();

            return builder;
        }

        internal static ContainerBuilder RegisterSkinSynchronizer(this ContainerBuilder builder, GameObject core)
        {
            builder.RegisterType<VanillaSkins>()
                .AsSelf()
                .SingleInstance();
            builder.RegisterType<LocalSkinReader>()
                .AsSelf()
                .SingleInstance();
            builder.RegisterComponent<SkinSynchronizer>(skinSynchronizer =>
                {
                    skinSynchronizer.transform.SetParent(core.transform);
                    skinSynchronizer.Init();
                }).
                AsSelf()
                .SingleInstance();
            return builder;
        }

        internal static ContainerBuilder RegisterOpeningSynchronizer(this ContainerBuilder builder, GameObject core)
        {
            builder.RegisterType<LocalOpeningReader>().AsSelf().SingleInstance();
            builder.RegisterComponent<OpeningSynchronizer>(synchronizer =>
                {
                    synchronizer.transform.SetParent(core.transform);
                    synchronizer.Init();
                })
                .AsSelf()
                .SingleInstance();
            return builder;
        }
    }
}
