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
            // Every module below registers its packet callbacks and event subscriptions in
            // IStartable.Start(), so the container activates them at Build() time. Composition
            // roots must not resolve them by hand.
            builder.RegisterType<PlayerService>()
                .As<IPlayerService>()
                .As<IStartable>()
                .SingleInstance();
            builder.RegisterType<ServerService>()
                .As<IServerService>()
                .As<IStartable>()
                .SingleInstance();
            builder.RegisterType<ChatService>()
                .AsSelf()
                .As<IStartable>()
                .SingleInstance();
            builder.RegisterType<PlayerStateRelay>()
                .AsSelf()
                .As<IStartable>()
                .SingleInstance();
            return builder;
        }
    }
}
