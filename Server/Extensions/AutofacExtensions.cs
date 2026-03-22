using Autofac;
using GOILauncher.Multiplayer.Core.Extensions;
using GOILauncher.Multiplayer.Server.Services;

namespace GOILauncher.Multiplayer.Server.Extensions
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder WithServer(this ContainerBuilder builder)
        {
            builder.RegisterServerCore()
                .RegisterType<ServerService>()
                .As<IServerService>()
                .SingleInstance();
            return builder; 
        }
    }
}
