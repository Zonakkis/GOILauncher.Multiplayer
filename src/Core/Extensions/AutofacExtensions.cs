using Autofac;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Constants;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Core.Network;
using GOILauncher.Multiplayer.Network;
using LiteNetLib;
using LiteNetLib.Utils;
using NLog.Targets;

namespace GOILauncher.Multiplayer.Core.Extensions
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder RegisterMultiplayerCore(this ContainerBuilder builder)
        {
            builder.RegisterType<CoreManager>()
                .AsSelf()
                .SingleInstance();

            builder.Register(ctx => new ConsoleTarget()
            {
                Layout = @"${date:format=yyyy-MM-dd HH\:mm\:ss}|${level:uppercase=true}|${logger:shortName=true}|${message}${onexception:inner=${newline}${exception:format=tostring}}"
            })
                .As<Target>()
                .SingleInstance();

            builder.RegisterGeneric(typeof(NLogLogger<>))
                .As(typeof(ILogger<>))
                .SingleInstance();

            builder.RegisterType<EventBus>()
                .As<IEventBus>()
                .SingleInstance();

            return builder;
        }

        public static ContainerBuilder RegisterClientCore(this ContainerBuilder builder)
        {
            // Each role owns a private NetPacketProcessor. Sharing one would let a packet
            // arriving on the server socket invoke a client-side handler (and vice versa)
            // whenever both roles run in the same process.
            builder.Register(c => new ClientPacketDispatcher(
                    new NetPacketProcessor(),
                    c.Resolve<ILogger<PacketDispatcher>>()))
                .As<IClientPacketDispatcher>()
                .SingleInstance();
            builder.RegisterType<NetworkClientListener>()
                .AsSelf()
                .SingleInstance();
            builder.Register(c =>
            {
                var netManager = new NetManager(c.Resolve<NetworkClientListener>())
                {
                    ChannelsCount = NetworkChannels.Count
                };
                return new NetworkClient(netManager,
                    c.Resolve<IEventBus>(),
                    c.Resolve<ILogger<NetworkClient>>());
            })
                .As<INetworkClient>()
                .As<IStartable>()
                .SingleInstance();
            return builder;
        }

        public static ContainerBuilder RegisterServerCore(this ContainerBuilder builder)
        {
            builder.Register(c => new ServerPacketDispatcher(
                    new NetPacketProcessor(),
                    c.Resolve<ILogger<PacketDispatcher>>()))
                .As<IServerPacketDispatcher>()
                .SingleInstance();
            builder.RegisterType<NetworkServerListener>()
                .AsSelf()
                .SingleInstance();
            builder.Register(c =>
            {
                var netManager = new NetManager(c.Resolve<NetworkServerListener>())
                {
                    ChannelsCount = NetworkChannels.Count
                };
                return new NetworkServer(netManager,
                    c.Resolve<ILogger<NetworkServer>>());
            })
                .As<INetworkServer>()
                .SingleInstance();
            return builder;
        }
    }
}
