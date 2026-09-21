using System;
using System.Collections.Generic;
using System.Globalization;
using GOILauncher.Multiplayer.Core.Data.Constants;
using GOILauncher.Multiplayer.Server.Services;

namespace GOILauncher.Multiplayer.DedicatedServer.Web
{
    /// <summary>顶部状态条。</summary>
    public sealed class StatusView
    {
        public bool IsRunning { get; set; }
        public bool IsStalled { get; set; }
        public int Port { get; set; }
        public string Uptime { get; set; }
        public string StartedAt { get; set; }
        public string ServerNow { get; set; }
        public string TimeZoneLabel { get; set; }
        public long PollGapMs { get; set; }
        public int OnlineCount { get; set; }
        public int RoomCount { get; set; }
        public int TotalErrors { get; set; }
    }

    /// <summary>左栏的房间条目（含"其他连接"这个伪条目）。</summary>
    public sealed class RoomItemView
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsLobby { get; set; }
        public bool IsUnassigned { get; set; }
        public bool IsSelected { get; set; }
        public bool HasPassword { get; set; }
        public string OwnerName { get; set; }
        public bool IsEmpty { get; set; }
        public int PlayerCount { get; set; }
        public string CapacityText { get; set; }
        public string KeyHint { get; set; }
    }

    /// <summary>连接表的一行。</summary>
    public sealed class ConnectionRowView
    {
        public int PlayerId { get; set; }
        public string DisplayName { get; set; }
        public bool HasHandshaked { get; set; }
        public bool IsInGame { get; set; }
        public string PlatformLabel { get; set; }
        public string EndPoint { get; set; }
        public int? LatencyMs { get; set; }
        public LatencyLevel Latency { get; set; }
        /// <summary>已格式化（不变文化），cshtml 里不做 ToString——否则逗号小数点的区域设置会毁掉 style 属性。</summary>
        public string LatencyText { get; set; }
        public double? LossPercent { get; set; }
        public string LossText { get; set; }
        public bool LossWarn { get; set; }
        public int LossBarPercent { get; set; }
        public bool HasStats { get; set; }
        public int NetworkErrorCount { get; set; }

        // 展开行才显示的原始计数——旧页面把这几项全丢了。
        public string PacketsSent { get; set; }
        public string PacketsReceived { get; set; }
        public string BytesSent { get; set; }
        public string BytesReceived { get; set; }
        public string PacketLoss { get; set; }
        public string PacketLossPercent { get; set; }
    }

    public sealed class ChatMessageView
    {
        public string Time { get; set; }
        public string PlayerName { get; set; }
        public string Content { get; set; }
    }

    public sealed class DashboardView
    {
        public StatusView Status { get; set; }
        public List<RoomItemView> Rail { get; set; }
        /// <summary>未选中任何房间（服务器未运行／房间表为空）时为 null。</summary>
        public RoomItemView Selected { get; set; }
        public List<ConnectionRowView> Connections { get; set; }
        public List<ChatMessageView> Chat { get; set; }
        public string ChatEmptyText { get; set; }
    }

    /// <summary>
    /// 把只读快照摊成渲染直接能用的形状。这是旧 BuildSnapshot 的替代品，
    /// 但它输出的是视图模型而不是 JSON，所以 RoomChat 那个
    /// ReadOnlyDictionary 手工转换问题不存在了——Razor 直接索引 Dictionary。
    /// 纯读取，无副作用：在 Kestrel 线程上跑是安全的。
    /// </summary>
    public static class DashboardProjection
    {
        /// <summary>尚未入房/握手中的连接归到这个伪房间，与 ConnectionObservation.RoomId 的约定一致。</summary>
        public const int UnassignedRoomId = -1;

        /// <summary>聊天面板最多渲染多少条。环形缓冲上限是 200，留点余量。</summary>
        private const int ChatDisplayLimit = 200;

        public static DashboardView Build(ServerObservationSnapshot snap, int gamePort, int? requestedRoom)
        {
            var byRoom = new Dictionary<int, List<ConnectionObservation>>();
            foreach (var c in snap.Connections)
            {
                List<ConnectionObservation> list;
                if (!byRoom.TryGetValue(c.RoomId, out list))
                    byRoom[c.RoomId] = list = new List<ConnectionObservation>();
                list.Add(c);
            }

            var rail = new List<RoomItemView>();
            RoomObservation lobby = null;
            var custom = new List<RoomObservation>();
            foreach (var r in snap.Rooms)
            {
                if (r.Info.IsLobby) lobby = r;
                else custom.Add(r);
            }
            // 按 Id（创建顺序）稳定排列，不按人数。人数一变就换位置的话，操作员建立不起
            // 位置记忆，数字快捷键也会跟着错位——旧页面的 auto-fill 卡片网格就是这个毛病。
            // 活跃度改用亮度/徽章表达，位置永远不动。
            custom.Sort((a, b) => a.Info.Id.CompareTo(b.Info.Id));

            // 命名参数不是风格偏好：这里连着两个 bool，位置传参写反过一次，
            // 后果是大厅聊天永远不显示（BuildRoomChat 刻意跳过了 LobbyId，
            // 所以选中大厅时 IsLobby 为 false 会去 RoomChat[0] 找一个不存在的键）。
            if (lobby != null)
                rail.Add(Item(lobby.Info.Name, RoomConstants.LobbyId, lobby.Info.PlayerCount,
                    lobby.Info.MaxPlayers, isLobby: true, isUnassigned: false,
                    hasPassword: lobby.Info.HasPassword, ownerName: lobby.OwnerName, byRoom: byRoom));

            foreach (var r in custom)
                rail.Add(Item(r.Info.Name, r.Info.Id, r.Info.PlayerCount, r.Info.MaxPlayers,
                    isLobby: false, isUnassigned: false,
                    hasPassword: r.Info.HasPassword, ownerName: r.OwnerName, byRoom: byRoom));

            List<ConnectionObservation> unassigned;
            byRoom.TryGetValue(UnassignedRoomId, out unassigned);
            if (unassigned != null && unassigned.Count > 0)
                rail.Add(Item("其他连接", UnassignedRoomId, unassigned.Count, 0,
                    isLobby: false, isUnassigned: true, hasPassword: false, ownerName: null, byRoom: byRoom));

            // 选中的房间可能已经消失（被解散），回落到大厅而不是报错。
            var selectedId = requestedRoom ?? RoomConstants.LobbyId;
            var selected = rail.Find(x => x.Id == selectedId) ?? rail.Find(x => x.IsLobby);
            if (selected != null) selected.IsSelected = true;

            // 数字键提示：按左栏最终顺序取前 9 个。因为顺序是稳定的，这个映射也稳定。
            for (var i = 0; i < rail.Count && i < 9; i++) rail[i].KeyHint = (i + 1).ToString(CultureInfo.InvariantCulture);

            var conns = new List<ConnectionObservation>();
            if (selected != null) byRoom.TryGetValue(selected.Id, out conns);
            conns = conns ?? new List<ConnectionObservation>();

            var view = new DashboardView
            {
                Status = BuildStatus(snap, gamePort),
                Rail = rail,
                Selected = selected,
                Connections = BuildRows(DashboardRules.OrderConnections(conns)),
                Chat = BuildChat(snap, selected),
                ChatEmptyText = selected == null
                    ? "服务器未运行。"
                    : (selected.IsUnassigned ? "未入房的连接没有聊天频道。" : "（本房间暂无聊天）")
            };
            return view;
        }

        private static RoomItemView Item(string name, int id, int playerCount, int maxPlayers,
            bool isLobby, bool isUnassigned, bool hasPassword, string ownerName,
            Dictionary<int, List<ConnectionObservation>> byRoom)
        {
            // 房间表里的 PlayerCount 是权威值；"其他连接"没有房间行，用连接数。
            List<ConnectionObservation> live;
            byRoom.TryGetValue(id, out live);
            var actual = isUnassigned ? (live != null ? live.Count : 0) : playerCount;
            return new RoomItemView
            {
                Id = id,
                Name = string.IsNullOrEmpty(name) ? "（无名房间）" : name,
                IsLobby = isLobby,
                IsUnassigned = isUnassigned,
                PlayerCount = actual,
                IsEmpty = actual == 0,
                HasPassword = hasPassword,
                OwnerName = ownerName,
                CapacityText = isUnassigned ? actual + " 人" : DashboardRules.CapacityText(actual, maxPlayers)
            };
        }

        private static StatusView BuildStatus(ServerObservationSnapshot snap, int gamePort)
        {
            var errors = snap.UnattributedNetworkErrors;
            foreach (var c in snap.Connections) errors += c.NetworkErrorCount;

            var offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            return new StatusView
            {
                IsRunning = snap.IsRunning,
                IsStalled = DashboardRules.IsStalled(snap.MaxPollGap),
                Port = gamePort,
                Uptime = DashboardRules.FormatUptime(snap.Uptime),
                StartedAt = DashboardRules.FormatClock(snap.StartedAt),
                ServerNow = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
                TimeZoneLabel = string.Format(CultureInfo.InvariantCulture, "UTC{0}{1:00}:{2:00}",
                    offset < TimeSpan.Zero ? "-" : "+", Math.Abs(offset.Hours), Math.Abs(offset.Minutes)),
                PollGapMs = (long)snap.MaxPollGap.TotalMilliseconds,
                OnlineCount = snap.Connections.Count,
                RoomCount = snap.Rooms.Count,
                TotalErrors = errors
            };
        }

        private static List<ConnectionRowView> BuildRows(IEnumerable<ConnectionObservation> ordered)
        {
            var rows = new List<ConnectionRowView>();
            foreach (var c in ordered)
            {
                var stats = c.Statistics;
                var loss = DashboardRules.LossPercent(stats);
                var lossPercentRaw = loss.HasValue ? loss.Value : 0.0;
                rows.Add(new ConnectionRowView
                {
                    PlayerId = c.PlayerId,
                    // 未握手时 Name 是 null 且 Platform 是 default(Platform)==PC——
                    // 旧页面会照着显示 "PC"，那是个假事实，这里直接不显示。
                    DisplayName = c.HasHandshaked
                        ? (string.IsNullOrEmpty(c.Name) ? "（未命名）" : c.Name)
                        : "（握手中）",
                    HasHandshaked = c.HasHandshaked,
                    IsInGame = c.IsInGame,
                    PlatformLabel = c.HasHandshaked ? DashboardRules.PlatformLabel(c.Platform) : string.Empty,
                    EndPoint = string.IsNullOrEmpty(c.EndPoint) ? "-" : c.EndPoint,
                    LatencyMs = c.LatencyMilliseconds,
                    Latency = DashboardRules.Classify(c.LatencyMilliseconds),
                    LatencyText = c.LatencyMilliseconds.HasValue
                        ? c.LatencyMilliseconds.Value.ToString(CultureInfo.InvariantCulture) + " ms"
                        : "-",
                    LossPercent = loss,
                    LossText = loss.HasValue
                        ? loss.Value.ToString("0.0", CultureInfo.InvariantCulture) + "%"
                        : "-",
                    LossWarn = DashboardRules.IsLossWarn(loss),
                    LossBarPercent = (int)Math.Max(0, Math.Min(100, Math.Round(lossPercentRaw))),
                    HasStats = stats != null,
                    NetworkErrorCount = c.NetworkErrorCount,
                    PacketsSent = stats != null ? DashboardRules.FormatCount(stats.PacketsSent) : "-",
                    PacketsReceived = stats != null ? DashboardRules.FormatCount(stats.PacketsReceived) : "-",
                    BytesSent = stats != null ? DashboardRules.FormatCount(stats.BytesSent) : "-",
                    BytesReceived = stats != null ? DashboardRules.FormatCount(stats.BytesReceived) : "-",
                    PacketLoss = stats != null ? stats.PacketLoss.ToString(CultureInfo.InvariantCulture) : "-",
                    PacketLossPercent = stats != null
                        ? stats.PacketLossPercent.ToString(CultureInfo.InvariantCulture) + "%"
                        : "-"
                });
            }
            return rows;
        }

        private static List<ChatMessageView> BuildChat(ServerObservationSnapshot snap, RoomItemView selected)
        {
            var messages = new List<ChatMessageObservation>();
            if (selected != null && !selected.IsUnassigned)
            {
                if (selected.IsLobby)
                    messages.AddRange(snap.LobbyChat);
                else
                {
                    System.Collections.ObjectModel.ReadOnlyCollection<ChatMessageObservation> room;
                    if (snap.RoomChat.TryGetValue(selected.Id, out room) && room != null)
                        messages.AddRange(room);
                }
            }

            // 只保留最新的 N 条：面板给了可滚动区域，所以比旧卡片的 4/30 宽松得多。
            var start = Math.Max(0, messages.Count - ChatDisplayLimit);
            var result = new List<ChatMessageView>(messages.Count - start);
            for (var i = start; i < messages.Count; i++)
            {
                var m = messages[i];
                result.Add(new ChatMessageView
                {
                    Time = DashboardRules.FormatChatTime(m.Timestamp),
                    PlayerName = string.IsNullOrEmpty(m.PlayerName) ? "？" : m.PlayerName,
                    Content = m.Content ?? string.Empty
                });
            }
            return result;
        }
    }
}
