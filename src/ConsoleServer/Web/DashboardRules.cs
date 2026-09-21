using System;
using System.Collections.Generic;
using System.Globalization;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Server.Services;

namespace ConsoleServer.Web
{
    /// <summary>延迟档位。视图只认这个，阈值调整不会漏到 cshtml 里。</summary>
    public enum LatencyLevel
    {
        /// <summary>LiteNetLib 还没吐出第一个样本，不是"0ms"。</summary>
        Unknown,
        Ok,
        Warn,
        Bad
    }

    /// <summary>
    /// 面板的判断层：什么算卡顿、什么算慢、什么时候该标红。全是纯函数，
    /// 不碰观察快照也不碰 HTML，所以能被测试直接驱动。
    ///
    /// 本文件被 tests/Multiplayer.Tests 以 &lt;Compile Link&gt; 引入（该测试项目是 net481，
    /// 引用不了 net8.0 的 ConsoleServer）。代价是这里只能出现 Server/Core 的类型和 BCL，
    /// 不能引用 DashboardView 里的展示类型，也不能用 record / init（net481 没有 IsExternalInit）。
    /// 这正是把规则和投影分开的原因。
    /// </summary>
    public static class DashboardRules
    {
        /// <summary>Poll 间隔超过它就算主循环卡了。循环实际是 66Hz，1 秒已经非常宽松。</summary>
        public static readonly TimeSpan StalledThreshold = TimeSpan.FromMilliseconds(1000);

        public const int LatencyWarnMs = 150;
        public const int LatencyBadMs = 300;

        /// <summary>
        /// 样本少于这个数就不显示丢包率：开局前几个包抖一下就能算出 50%，
        /// 那是噪声不是信号。
        /// </summary>
        public const long MinLossSample = 100;

        public const double LossWarnPercent = 1.0;

        public static bool IsStalled(TimeSpan maxPollGap) => maxPollGap > StalledThreshold;

        public static LatencyLevel Classify(int? milliseconds)
        {
            if (!milliseconds.HasValue) return LatencyLevel.Unknown;
            if (milliseconds.Value >= LatencyBadMs) return LatencyLevel.Bad;
            if (milliseconds.Value > LatencyWarnMs) return LatencyLevel.Warn;
            return LatencyLevel.Ok;
        }

        /// <summary>丢包率；样本不足或对端一个包没发时返回 null，让视图显示 "-" 而不是 0%。</summary>
        public static double? LossPercent(ConnectionStatsObservation stats)
        {
            if (stats == null || stats.PacketsSent < MinLossSample) return null;
            return (stats.PacketsSent - stats.PacketsReceived) / (double)stats.PacketsSent * 100.0;
        }

        public static bool IsLossWarn(double? percent) => percent.HasValue && percent.Value > LossWarnPercent;

        public static string PlatformLabel(Platform platform)
        {
            switch (platform)
            {
                case Platform.PC: return "PC";
                case Platform.iOS: return "iOS";
                case Platform.Android: return "安卓";
                default: return "未知";
            }
        }

        /// <summary>小时不封顶（跑够 100 小时就显示 100），运维看的连续性比日历格式重要。</summary>
        public static string FormatUptime(TimeSpan? uptime)
        {
            if (!uptime.HasValue) return "-";
            var t = uptime.Value;
            return string.Format(CultureInfo.InvariantCulture, "{0}:{1:00}:{2:00}",
                (long)t.TotalHours, t.Minutes, t.Seconds);
        }

        public static string FormatClock(DateTime? time) =>
            time.HasValue ? time.Value.ToString("HH:mm:ss", CultureInfo.InvariantCulture) : "-";

        /// <summary>时间戳是 unix 秒（<see cref="ChatMessageObservation.Timestamp"/>），按服务器本地时区渲染。</summary>
        public static string FormatChatTime(long unixSeconds) =>
            DateTimeOffset.FromUnixTimeSeconds(unixSeconds).ToLocalTime()
                .ToString("HH:mm:ss", CultureInfo.InvariantCulture);

        /// <summary>MaxPlayers 为 0 表示不限人数。</summary>
        public static string CapacityText(int playerCount, int maxPlayers) =>
            maxPlayers > 0 ? playerCount + "/" + maxPlayers : playerCount + "/∞";

        /// <summary>3.4k / 912 —— 字节数太长，表里放不下也不值得。</summary>
        public static string FormatCount(long value)
        {
            if (value < 1000) return value.ToString(CultureInfo.InvariantCulture);
            if (value < 1000000) return (value / 1000.0).ToString("0.0", CultureInfo.InvariantCulture) + "k";
            return (value / 1000000.0).ToString("0.0", CultureInfo.InvariantCulture) + "M";
        }

        /// <summary>
        /// 连接表的显示顺序。操作员打开面板，第一眼该看到谁？
        /// 现在的实现是原样返回（即按 playerId 升序，和旧页面一致）——把它当占位，等你决定。
        /// </summary>
        public static IEnumerable<ConnectionObservation> OrderConnections(IEnumerable<ConnectionObservation> connections)
        {
            // TODO(human): 见下方 Learn by Doing
            return connections;
        }
    }
}
