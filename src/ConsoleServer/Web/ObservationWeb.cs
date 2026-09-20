using GOILauncher.Multiplayer.Server.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace ConsoleServer.Web
{
    /// <summary>
    /// Web UI 的 HTTP 宿主：Minimal API + 内嵌单文件页面，零 NuGet、零构建工具链。
    /// 快照在 Kestrel 工作线程上读取——观测门面本就按"任意线程只读快照"设计，这里不加任何锁。
    /// 只做只读投影，不提供任何干预玩家/房间的入口。
    /// </summary>
    public static class ObservationWeb
    {
        private static readonly string IndexHtml = ReadEmbedded("ConsoleServer.Web.index.html");

        private static string ReadEmbedded(string logicalName)
        {
            using var stream = typeof(ObservationWeb).Assembly.GetManifestResourceStream(logicalName);
            if (stream == null) return "<h1>缺少内嵌页面 " + logicalName + "</h1>";
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public static WebApplication Create(IObservationService observation, WebLogTarget logs, int webPort, int gamePort)
        {
            var app = WebApplication.Create();
            app.MapGet("/", () => Results.Content(IndexHtml, "text/html; charset=utf-8"));

            app.MapGet("/api/snapshot", () => Results.Json(BuildSnapshot(observation.Snapshot, gamePort)));

            app.MapGet("/api/logs", (long? since) =>
            {
                var rows = logs.Fetch(since ?? 0, out var nextSeq, out var reset);
                return Results.Json(new { rows, nextSeq, reset });
            });

            app.Urls.Add($"http://0.0.0.0:{webPort}");
            return app;
        }

        /// <summary>
        /// 把观测快照摊平成可序列化的 JSON 形状。两处必须手转：
        /// RoomChat 底层是 ReadOnlyDictionary（System.Text.Json 不认）；DateTime? 序列化成字符串。
        /// </summary>
        private static object BuildSnapshot(ServerObservationSnapshot snap, int gamePort)
        {
            var roomChat = new Dictionary<int, object>();
            foreach (var kv in snap.RoomChat)
                roomChat[kv.Key] = kv.Value;

            return new
            {
                isRunning = snap.IsRunning,
                port = gamePort,
                startedAt = snap.StartedAt,
                uptimeSeconds = snap.Uptime.HasValue ? (long)snap.Uptime.Value.TotalSeconds : (long?)null,
                pollCount = snap.PollCount,
                maxPollGapMs = (long)snap.MaxPollGap.TotalMilliseconds,
                unattributedErrors = snap.UnattributedNetworkErrors,
                connections = snap.Connections,
                rooms = snap.Rooms,
                lobbyChat = snap.LobbyChat,
                roomChat
            };
        }
    }
}
