using Autofac;
using GOILauncher.Multiplayer.Client.Services;
using GOILauncher.Multiplayer.Client.Synchronization;
using GOILauncher.Multiplayer.Core.Extensions;

namespace GOILauncher.Multiplayer.Client.Extensions
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder WithClient(this ContainerBuilder builder)
        {
            builder.RegisterClientCore();
            // Every module below registers its packet callbacks and event subscriptions in
            // IStartable.Start(), so the container activates them at Build() time. Composition
            // roots must not resolve them by hand.
            builder.RegisterType<ClientService>()
                .As<IClientService>()
                .As<IStartable>()
                .SingleInstance();
            builder.RegisterType<PlayerService>()
                .As<IPlayerService>()
                .As<IStartable>()
                .SingleInstance();
            builder.RegisterType<ChatService>()
                .As<IChatService>()
                .As<IStartable>()
                .SingleInstance();
            builder.RegisterType<ClientPlayerStateSync>()
                .AsSelf()
                .As<IStartable>()
                .SingleInstance();
            return builder;
        }
    }
}
