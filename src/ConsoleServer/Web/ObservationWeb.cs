using GOILauncher.Multiplayer.Server.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.WebEncoders;
using System;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace ConsoleServer.Web
{
    /// <summary>
    /// 递进 Razor Pages 的宿主参数。游戏端口要在状态条上显示，但它不属于 web 层，
    /// 用一个单例把它送进 PageModel，避免为了一个 int 去引入 IOptions。
    /// </summary>
    public sealed class DashboardOptions
    {
        public int GamePort { get; }

        public DashboardOptions(int gamePort) { GamePort = gamePort; }
    }

    /// <summary>
    /// Web UI 的 HTTP 宿主：Razor Pages 服务端渲染 + 1 秒一次的部分刷新。
    ///
    /// 页面本身只读——渲染用的快照按"任意线程只读"设计，所以这里不加任何锁，
    /// 也没有任何干预玩家/房间的入口。唯一的写入口是查询串（选中哪个房间）。
    ///
    /// 关于 DI：PageModel 的构造注入只能由 WebApplication 自带的 IServiceCollection 提供。
    /// 这不是第二套 DI 框架——是 MVC 强制依赖的宿主容器，绕不开。Autofac 4.9.4 钉死在
    /// netstandard2.0 的 Core/Server 上，升不到能当 web 容器的版本，所以没有别的选择。
    /// </summary>
    public static class ObservationWeb
    {
        public static WebApplication Create(IObservationService observation, WebLogTarget logs, int webPort, int gamePort)
        {
            var builder = WebApplication.CreateBuilder();

            // 内容根必须钉在程序集目录。默认是当前工作目录，那样 `dotnet ConsoleServer.dll`
            // 从别的目录启动时 wwwroot 就会找不到，静态资源全部 404。
            builder.WebHost.UseContentRoot(AppContext.BaseDirectory);
            builder.WebHost.UseUrls($"http://0.0.0.0:{webPort}");

            // 服务器每 15ms Poll 一次、面板每秒轮询一次，默认的 Information 级请求日志
            // 会往 stdout 刷两行/秒，把真正的业务日志冲没。只压掉 AspNetCore 这一支，
            // Microsoft.Hosting.Lifetime 的启动/关闭消息保留。
            builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);

            builder.Services.AddSingleton(observation);
            builder.Services.AddSingleton(logs);
            builder.Services.AddSingleton(new DashboardOptions(gamePort));
            builder.Services.AddRazorPages();

            // 默认的 HtmlEncoder 只放行 BasicLatin，中文会被编成 &#x5927;&#x5385; 这种数字实体——
            // 页面能显示，但体积涨四倍、view-source 没法读。这里只额外放行 CJK 区段：
            // HTML 的元字符（< > & " '）全是 ASCII，仍然照旧转义，玩家名里的其他 Unicode
            // （emoji、西里尔字母等）也继续走实体转义。
            var encoderSettings = new TextEncoderSettings();
            encoderSettings.AllowRanges(
                UnicodeRanges.BasicLatin,
                UnicodeRanges.GeneralPunctuation,
                UnicodeRanges.CjkSymbolsandPunctuation,
                UnicodeRanges.CjkUnifiedIdeographs,
                UnicodeRanges.HalfwidthandFullwidthForms);
            builder.Services.Configure<WebEncoderOptions>(o => o.TextEncoderSettings = encoderSettings);

            var app = builder.Build();
            app.UseStaticFiles();

            // 日志是带游标的机器接口（?since=seq，环形缓冲淘汰时回 reset 信号），不是页面，
            // 所以留在 Minimal API 上，由 wwwroot/js/dashboard.js 直接拉。
            app.MapGet("/api/logs", (long? since) =>
            {
                var rows = logs.Fetch(since ?? 0, out var nextSeq, out var reset);
                return Results.Json(new { rows, nextSeq, reset });
            });

            app.MapRazorPages();
            return app;
        }
    }
}
