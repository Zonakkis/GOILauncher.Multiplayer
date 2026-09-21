using GOILauncher.Multiplayer.Server.Services;
using GOILauncher.Multiplayer.DedicatedServer.Api.V1;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace GOILauncher.Multiplayer.DedicatedServer.Web
{
    /// <summary>
    /// Web UI 的 HTTP 宿主：版本化观测 API + 同源托管的 React 静态资源。
    ///
    /// 页面本身只读，没有干预玩家/房间的入口。React 构建产物直接进入 wwwroot，
    /// 开发时由 Vite dev server 代理 API，生产环境不启动 Node。
    /// </summary>
    public static class ObservationWeb
    {
        public static WebApplication Create(IObservationService observation, WebLogTarget logs, int webPort, int gamePort)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                ContentRootPath = AppContext.BaseDirectory
            });

            // 内容根必须钉在程序集目录。默认是当前工作目录，那样 `dotnet GOILauncher.Multiplayer.DedicatedServer.dll`
            // 从别的目录启动时 wwwroot 就会找不到，静态资源全部 404。
            builder.WebHost.UseUrls($"http://0.0.0.0:{webPort}");

            // 服务器每 15ms Poll 一次、面板每秒轮询一次，默认的 Information 级请求日志
            // 会往 stdout 刷两行/秒，把真正的业务日志冲没。只压掉 AspNetCore 这一支，
            // Microsoft.Hosting.Lifetime 的启动/关闭消息保留。
            builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);

            builder.Services.AddSingleton(observation);
            builder.Services.AddSingleton(logs);

            var app = builder.Build();
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.MapObservationApi(observation, logs, gamePort);
            app.MapFallbackToFile("index.html");
            return app;
        }
    }
}
