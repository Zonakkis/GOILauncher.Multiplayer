using Autofac;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Network;
using LiteNetLib;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Extensions
{
    public static class AutofacExtensions
    {
        public static void RegisterMultiplayerCore(this ContainerBuilder builder)
        {
            NLogConfiguration.Configure();
            builder.RegisterGeneric(typeof(NLogLogger<>))
                .As(typeof(ILogger<>))
                .SingleInstance();

            builder.RegisterType<EventBus>()
                .As<IEventBus>()
                .SingleInstance();

            builder.RegisterNetwork();
            builder.RegisterType<PacketDispatcher>()
                .As<IPacketDispatcher>()
                .SingleInstance();
        }

        private static void RegisterNetwork(this ContainerBuilder builder)
        {
            builder.RegisterType<EventBasedNetListener>()
                .AsSelf()
                .SingleInstance();
            builder.Register(c => new NetManager(c.Resolve<EventBasedNetListener>()))
                .AsSelf()
                .SingleInstance();
            builder.RegisterType<NetPacketProcessor>()
                .AsSelf()
                .SingleInstance();
            builder.RegisterType<NetworkServer>()
                .As<INetworkServer>()
                .SingleInstance();
            builder.RegisterType<NetworkClient>()
                .As<INetworkClient>()
                .SingleInstance();
        }
    }
}
