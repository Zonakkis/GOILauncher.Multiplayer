using Autofac;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Handlers;
using GOILauncher.Multiplayer.Core.Data.Packets.S2C;
using GOILauncher.Multiplayer.Core.Data.Serialization;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;

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

            builder.RegisterType<ArraySegmentReader>()
                .AsSelf()
                .InstancePerDependency();
            builder.RegisterType<PacketDispatcher>()
                .As<IPacketDispatcher>()
                .SingleInstance();
            builder.RegisterPacketSerializers();
        }

        private static void RegisterPacketSerializers(this ContainerBuilder builder)
        {
            builder.RegisterType<ServerHandShakePacketSerializer>()
                .As<IPacketSerializer<ServerHandShakePacket>>()
                .InstancePerDependency();
        }
    }
}
