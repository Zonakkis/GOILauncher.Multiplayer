using Autofac;
using GOILauncher.Multiplayer.Core.Extensions;
using GOILauncher.Multiplayer.Server.Services;
using GOILauncher.Multiplayer.Server.Synchronization;

namespace GOILauncher.Multiplayer.Server.Extensions
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder WithServer(this ContainerBuilder builder, bool enableDiagnostics = false)
        {
            builder.RegisterServerCore(enableDiagnostics);
            // Every module below registers its packet callbacks and event subscriptions in
            // IStartable.Start(), so the container activates them at Build() time. Composition
            // roots must not resolve them by hand.
            builder.RegisterType<PlayerService>()
                .As<IPlayerService>()
                .As<IStartable>()
                .SingleInstance();
            builder.RegisterType<RoomService>()
                .As<IRoomService>()
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
            if (enableDiagnostics)
            {
                // Read-only diagnostics view; subscribes on the Poll thread and freezes snapshots
                // for a host. This is opt-in because Unity embeds the same server implementation.
                builder.RegisterType<ObservationService>()
                    .As<IObservationService>()
                    .As<IStartable>()
                    .SingleInstance();
            }
            builder.RegisterType<PlayerStateRelay>()
                .AsSelf()
                .As<IStartable>()
                .SingleInstance();
            builder.RegisterType<SkinRelay>()
                .AsSelf()
                .As<IStartable>()
                .SingleInstance();
            return builder;
        }
    }
}
