using Autofac;
using GOILauncher.Multiplayer.Client.Managers;
using GOILauncher.Multiplayer.Core.Extensions;

namespace GOILauncher.Multiplayer.Client.Extensions
{
    public static class AutofacExtensions
    {
        public static void RegisterMultiplayerClient(this ContainerBuilder builder)
        {
            builder.RegisterMultiplayerCore();

            builder.RegisterType<ClientManager>()
                .AsSelf()
                .SingleInstance();
        }
    }
}
