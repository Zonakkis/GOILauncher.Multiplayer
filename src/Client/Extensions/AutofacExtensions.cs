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
            builder.RegisterType<ClientService>()
                .As<IClientService>()
                .SingleInstance();
            builder.RegisterType<PlayerService>()
                .As<IPlayerService>()
                .SingleInstance();
            builder.RegisterType<ChatService>()
                .As<IChatService>()
                .SingleInstance();
            builder.RegisterType<ClientPlayerStateSync>()
                .AsSelf()
                .SingleInstance();
            return builder;
        }
    }
}
