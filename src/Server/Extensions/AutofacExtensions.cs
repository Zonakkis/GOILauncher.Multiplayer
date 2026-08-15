using Autofac;
using GOILauncher.Multiplayer.Core.Extensions;
using GOILauncher.Multiplayer.Server.Services;
using GOILauncher.Multiplayer.Server.Synchronization;

namespace GOILauncher.Multiplayer.Server.Extensions
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder WithServer(this ContainerBuilder builder)
        {
            builder.RegisterServerCore();
            builder.RegisterType<PlayerService>()
                .As<IPlayerService>()
                .SingleInstance();
            builder.RegisterType<ServerService>()
                .As<IServerService>()
                .SingleInstance();
            builder.RegisterType<ChatService>()
                .SingleInstance();
            builder.RegisterType<PlayerStateRelay>()
                .AsSelf()
                .SingleInstance();
            return builder;
        }
    }
}
