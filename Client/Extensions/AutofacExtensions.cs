using Autofac;
using GOILauncher.Multiplayer.Client.Services;
using GOILauncher.Multiplayer.Core.Extensions;

namespace GOILauncher.Multiplayer.Client.Extensions
{
    public static class AutofacExtensions
    {
        public static void RegisterMultiplayerClient(this ContainerBuilder builder)
        {
            builder.RegisterMultiplayerCore().WithClient();

            builder.RegisterType<ClientService>()
                .As<IClientService>()
                .SingleInstance();
        }
    }
}
