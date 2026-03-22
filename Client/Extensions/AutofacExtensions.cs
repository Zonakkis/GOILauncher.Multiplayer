using Autofac;
using GOILauncher.Multiplayer.Client.Services;
using GOILauncher.Multiplayer.Core.Extensions;

namespace GOILauncher.Multiplayer.Client.Extensions
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder WithClient(this ContainerBuilder builder)
        {
            builder.RegisterClientCore()
                .RegisterType<ClientService>()
                .As<IClientService>()
                .SingleInstance();
            return builder;
        }
    }
}
