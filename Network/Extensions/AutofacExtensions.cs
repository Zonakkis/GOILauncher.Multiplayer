using Autofac;
using GOILauncher.Multiplayer.Network.Converters;
using GOILauncher.Multiplayer.Network.Enums;
using LiteNetLib;

namespace GOILauncher.Multiplayer.Network.Extensions
{
    public static class AutofacExtensions
    {
        public static void RegisterLiteNetLib(this ContainerBuilder builder)
        {
            builder.RegisterType<NetManager>()
                .AsSelf()
                .SingleInstance();
            builder.RegisterType<EventBasedNetListener>()
                .AsSelf()
                .SingleInstance();
            builder.RegisterType<LiteNetLibServer>()
                .As<INetworkServer>()
                .SingleInstance();
            builder.RegisterType<LiteNetLibClient>()
                .As<INetworkClient>()
                .SingleInstance();
            builder.RegisterType<LiteNetLibSendModeConverter>()
                .As<IConverter<SendMode, DeliveryMethod>>()
                .SingleInstance();
            builder.RegisterType<NetworkConverter>()
                .As<INetworkConverter>()
                .SingleInstance();
        }
    }
}
