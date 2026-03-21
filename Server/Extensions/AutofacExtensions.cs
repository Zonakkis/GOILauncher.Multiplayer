using Autofac;
using GOILauncher.Multiplayer.Core.Extensions;
using GOILauncher.Multiplayer.Server.Services;

namespace GOILauncher.Multiplayer.Server.Extensions
{
    public static class AutofacExtensions
    {
        public static void RegisterMultiplayerServer(this ContainerBuilder builder)
        {
            builder.RegisterMultiplayerCore();

            builder.RegisterType<ServerService>()
                .As<IServerService>()
                .SingleInstance();
        }
    }
}
