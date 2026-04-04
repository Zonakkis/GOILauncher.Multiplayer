using Autofac;
using GOILauncher.Multiplayer.Core;
using GOILauncher.Multiplayer.Server.Extensions;
using GOILauncher.Multiplayer.Server.Services;
using System;
using System.Configuration;
using System.Threading;

namespace ConsoleServer
{
    internal class Program
    {
        private static bool _keepRunning = true;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += OnCancelKeyPress;


            using (var container = Register())
            {
                var serverService = container.Resolve<IServerService>();

                var port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
                serverService.Start(port);

                while (_keepRunning)
                {
                    serverService.Poll();
                    Thread.Sleep(15);
                }

                serverService.Stop();
            }
        }

        private static void OnCancelKeyPress(object _, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            _keepRunning = false;
        }

        private static IContainer Register()
        {
            var builder = new ContainerBuilder();

            builder.WithServer();

            var container = builder.Build();

            container.Resolve<CoreManager>();

            return container;
        }
    }
}
