using Autofac;
using GOILauncher.Multiplayer.Core;
using GOILauncher.Multiplayer.Core.Extensions;
using GOILauncher.Multiplayer.Server.Extensions;
using GOILauncher.Multiplayer.Server.Services;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace ConsoleServer
{
    internal class Program
    {
        private static bool _keepRunning = true;
        // PosixSignalRegistration is unregistered when the returned handle is collected;
        // hold it for the process lifetime.
        private static readonly List<PosixSignalRegistration> _signalRegistrations = new List<PosixSignalRegistration>();

        static void Main(string[] args)
        {
            RegisterShutdownHandlers();

            using (var container = Register())
            {
                var serverService = container.Resolve<IServerService>();

                var port = GetPort();
                serverService.Start(port);

                while (_keepRunning)
                {
                    serverService.Poll();
                    Thread.Sleep(15);
                }

                serverService.Stop();
            }
        }

        private static int GetPort()
        {
            var raw = Environment.GetEnvironmentVariable("PORT");
            return string.IsNullOrWhiteSpace(raw) ? 9027 : int.Parse(raw);
        }

        private static void RegisterShutdownHandlers()
        {
            Console.CancelKeyPress += OnCancelKeyPress;

            // `docker stop` sends SIGTERM, which Console.CancelKeyPress never sees (it only
            // covers Ctrl+C / Ctrl+Break). With net6+ PosixSignalRegistration we drain
            // gracefully on SIGTERM too, so a container stop doesn't cut connections hard.
            try
            {
                _signalRegistrations.Add(PosixSignalRegistration.Create(PosixSignal.SIGTERM, ctx =>
                {
                    ctx.Cancel = true;
                    _keepRunning = false;
                }));
            }
            catch (PlatformNotSupportedException)
            {
                // Non-POSIX platform — SIGTERM cannot occur anyway.
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

            // RegisterMultiplayerCore supplies CoreManager, ILogger<> and IEventBus;
            // WithServer only registers the server role on top of them.
            builder.RegisterMultiplayerCore().WithServer();

            var container = builder.Build();

            container.Resolve<CoreManager>();

            return container;
        }
    }
}
