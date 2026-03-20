using Autofac;
using GOILauncher.Multiplayer.Core.Log;

namespace GOILauncher.Multiplayer.Core.Extensions
{
    public static class AutofacExtensions
    {
        public static void RegisterNLog(this ContainerBuilder builder)
        {
            NLogConfiguration.Configure();
            builder.RegisterGeneric(typeof(NLogLogger<>))
                .As(typeof(ILogger<>))
                .SingleInstance();
        }
    }
}
