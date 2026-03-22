using Autofac;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Core.Network;
using GOILauncher.Multiplayer.Network;
using LiteNetLib;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Extensions
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder RegisterMultiplayerCore(this ContainerBuilder builder)
        {
            NLogConfiguration.Configure();
            builder.RegisterGeneric(typeof(NLogLogger<>))
                .As(typeof(ILogger<>))
                .SingleInstance();

            builder.RegisterType<EventBus>()
                .As<IEventBus>()
                .SingleInstance();

            builder.RegisterType<NetPacketProcessor>()
                .AsSelf()
                .SingleInstance();
            builder.RegisterType<PacketDispatcher>()
                .As<IPacketDispatcher>()
                .SingleInstance();
            return builder;
        }

        public static ContainerBuilder WithClient(this ContainerBuilder builder)
        {
            builder.RegisterType<NetworkClientListener>()
                .AsSelf()
                .SingleInstance();
            builder.Register(c =>
            {
                var netManager = new NetManager(c.Resolve<NetworkClientListener>());
                return new NetworkClient(netManager,
                    c.Resolve<NetPacketProcessor>(),
                    c.Resolve<IEventBus>(),
                    c.Resolve<ILogger<NetworkClient>>());
            })
                .As<INetworkClient>()
                .SingleInstance();
            return builder;
        }

        public static ContainerBuilder WithServer(this ContainerBuilder builder)
        {
            builder.RegisterType<NetworkServerListener>()
                .AsSelf()
                .SingleInstance();
            builder.Register(c =>
            {
                var netManager = new NetManager(c.Resolve<NetworkServerListener>());
                return new NetworkServer(netManager,
                    c.Resolve<NetPacketProcessor>(),
                    c.Resolve<IEventBus>(),
                    c.Resolve<ILogger<NetworkServer>>());
            })
                .As<INetworkServer>()
                .SingleInstance();
            return builder;
        }
    }
}
