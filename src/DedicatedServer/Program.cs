using Autofac;
using GOILauncher.Multiplayer.Core;
using GOILauncher.Multiplayer.Core.Extensions;
using GOILauncher.Multiplayer.DedicatedServer.Web;
using GOILauncher.Multiplayer.Server.Extensions;
using GOILauncher.Multiplayer.Server.Services;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace GOILauncher.Multiplayer.DedicatedServer
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
                var observation = container.Resolve<IObservationService>();

                var port = GetPort();

                // 先挂日志环形 Target（早于任何业务日志），再启服务，保证启动日志也进面板。
                var logs = WebLogTarget.Register();
                var web = StartWeb(observation, logs, port);

                serverService.Start(port);

                // 快照本身线程安全，Poll 循环不加任何同步；面板挂了不影响服务器。（暂只监听 IPv4；如需 IPv6 双栈改绑定为 [::]。）
                while (_keepRunning)
                {
                    serverService.Poll();
                    // Advances liveness counters and refreshes the read-only view the console
                    // will render from. Cheap; the heavy cache refresh is throttled internally.
                    observation.MarkPoll();
                    Thread.Sleep(15);
                }

                if (web != null)
                {
                    // 给在途请求最多 1 秒收尾（StopAsync 只认 token，超时用 CTS 实现）；
                    // docker stop 宽限期 10s，不会卡退出。
                    using (var stopCts = new CancellationTokenSource(TimeSpan.FromSeconds(1)))
                    {
                        try { web.StopAsync(stopCts.Token).GetAwaiter().GetResult(); }
                        catch (OperationCanceledException) { /* 收尾超时，直接继续关闭 */ }
                    }
                }
                serverService.Stop();
            }
        }

        private static WebApplication StartWeb(IObservationService observation, WebLogTarget logs, int gamePort)
        {
            var webPort = GetWebPort();
            var app = ObservationWeb.Create(observation, logs, webPort, gamePort);
            var thread = new Thread(() =>
            {
                try { app.Run(); }
                catch (Exception ex) { Console.Error.WriteLine("[Web] Web UI线程退出：" + ex.Message); }
            })
            { IsBackground = true, Name = "ObservationWeb" };
            thread.Start();
            Console.WriteLine("[Web] Web UI: http://localhost:" + webPort + "/");
            return app;
        }

        private static int GetWebPort()
        {
            var raw = Environment.GetEnvironmentVariable("GOI_WEB_PORT");
            return string.IsNullOrWhiteSpace(raw) ? 9028 : int.Parse(raw);
        }

        private static int GetPort()
        {
            var raw = Environment.GetEnvironmentVariable("GOI_SERVER_PORT");
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

            // RegisterMultiplayerCore supplies CoreManager and ILogger<>. DedicatedServer owns the
            // diagnostics facade and the per-peer statistics that feed it.
            builder.RegisterMultiplayerCore().WithServer(enableDiagnostics: true);

            var container = builder.Build();

            container.Resolve<CoreManager>();

            return container;
        }
    }
}
