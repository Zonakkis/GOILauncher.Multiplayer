using Autofac;
using GOILauncher.Multiplayer.Client.Services;
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
            return builder;
        }
    }
}
