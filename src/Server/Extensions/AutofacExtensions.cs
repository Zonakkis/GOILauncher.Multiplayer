using Autofac;
using GOILauncher.Multiplayer.Core.Extensions;
using GOILauncher.Multiplayer.Server.Services;

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
            return builder;
        }
    }
}
