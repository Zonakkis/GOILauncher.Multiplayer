using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using GOILauncher.Multiplayer.DedicatedServer.Api.V1;

namespace GOILauncher.Multiplayer.DedicatedServer.Web
{
    /// <summary>
    /// 内存环形日志 Target：与 stdout 的 ConsoleTarget 并存（NLog 允许同一 rule 挂多个 target），
    /// Web UI 通过 /api/v1/logs?after=seq 增量拉取。
    /// 行格式约定为 "HH:mm:ss|LEVEL|logger|消息"，拆列时只切前三个分隔符，消息里的竖线保持原样。
    /// NLog 6 下必须继承 TargetWithLayout（Layout 属性在其上），重写同步 Write 并在结尾发 continuation。
    /// </summary>
    public sealed class WebLogTarget : TargetWithLayout
    {
        private sealed class Entry
        {
            public long Seq;
            public DateTimeOffset Timestamp;
            public string Time;
            public string Level;
            public string Logger;
            public string Message;
        }

        private const int Capacity = 300;
        private readonly object _gate = new object();
        private readonly LinkedList<Entry> _entries = new LinkedList<Entry>();
        private long _seq;
        private long _oldestSeq = 1;

        public WebLogTarget()
        {
            Layout = @"${date:format=HH\:mm\:ss}|${level:uppercase=true}|${logger:shortName=true}|${message}";
        }

        /// <summary>把本 Target 挂进 NLog 全局配置（与 CoreManager 对 ConsoleTarget 的做法同构）。</summary>
        public static WebLogTarget Register()
        {
            var target = new WebLogTarget { Name = "WebLogRing" };
            var config = LogManager.Configuration ?? new LoggingConfiguration();
            var existing = config.FindTargetByName(target.Name) as WebLogTarget;
            if (existing != null) return existing;

            if (config.FindTargetByName(target.Name) != null)
                throw new InvalidOperationException("NLog target 'WebLogRing' already exists with another type.");

            config.AddTarget(target);
            config.AddRuleForAllLevels(target);
            LogManager.Configuration = config;
            LogManager.ReconfigExistingLoggers();
            return target;
        }

        /// <summary>
        /// 取 seq 大于 since 的行；若 since 已被环形淘汰，reset=true 通知客户端清空重拉。
        /// </summary>
        public List<LogEntryDto> Fetch(long since, int limit, out long nextSeq, out bool reset)
        {
            if (limit <= 0) limit = 1;
            if (limit > Capacity) limit = Capacity;

            lock (_gate)
            {
                reset = false;
                if (since + 1 < _oldestSeq && _entries.Count > 0)
                {
                    // 请求的位置已经滚出缓冲：不补历史，让前端整页重置。
                    reset = true;
                    since = _oldestSeq - 1;
                }
                var result = new List<LogEntryDto>();
                foreach (var e in _entries)
                {
                    if (e.Seq <= since) continue;
                    result.Add(new LogEntryDto(e.Seq, e.Timestamp, e.Time, e.Level, e.Logger, e.Message));
                    if (result.Count >= limit) break;
                }
                nextSeq = _seq;
                return result;
            }
        }

        protected override void Write(AsyncLogEventInfo logEvent)
        {
            var rendered = Layout.Render(logEvent.LogEvent);
            var entry = new Entry { Timestamp = new DateTimeOffset(logEvent.LogEvent.TimeStamp) };
            var parts = rendered.Split(new[] { '|' }, 4);
            entry.Time = parts.Length > 0 ? parts[0] : string.Empty;
            entry.Level = parts.Length > 1 ? parts[1] : string.Empty;
            entry.Logger = parts.Length > 2 ? parts[2] : string.Empty;
            entry.Message = parts.Length > 3 ? parts[3] : rendered;

            lock (_gate)
            {
                entry.Seq = ++_seq;
                _entries.AddLast(entry);
                while (_entries.Count > Capacity)
                {
                    _entries.RemoveFirst();
                    _oldestSeq = _entries.First.Value.Seq;
                }
            }
            logEvent.Continuation(null);
        }
    }
}
