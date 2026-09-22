using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GOILauncher.Multiplayer.Core.Data.Models;

namespace GOILauncher.Multiplayer.Server.Services
{
    /// <summary>
    /// LiteNetLib packet/byte counters, only populated while statistics are enabled. Used both
    /// per-connection (a connection may have no sample yet, so that field is nullable) and as the
    /// server-wide aggregate for the current run (absent → <see cref="Empty"/>, never null).
    /// </summary>
    public sealed class TrafficObservation
    {
        public long PacketsSent { get; }
        public long PacketsReceived { get; }
        public long BytesSent { get; }
        public long BytesReceived { get; }
        public long PacketLoss { get; }
        public long PacketLossPercent { get; }

        public TrafficObservation(long packetsSent, long packetsReceived, long bytesSent,
            long bytesReceived, long packetLoss, long packetLossPercent)
        {
            PacketsSent = packetsSent; PacketsReceived = packetsReceived; BytesSent = bytesSent;
            BytesReceived = bytesReceived; PacketLoss = packetLoss; PacketLossPercent = packetLossPercent;
        }

        public static TrafficObservation Empty => new TrafficObservation(0, 0, 0, 0, 0, 0);
    }

    /// <summary>
    /// One row of the connections table: identity facts joined from PlayerService plus
    /// transport diagnostics collected by ObservationService. Immutable snapshot.
    /// </summary>
    public sealed class ConnectionObservation
    {
        public int PlayerId { get; }
        public string Name { get; }
        public Platform Platform { get; }
        public bool IsInGame { get; }
        /// <summary>当前房间 Id；-1 表示尚未入房（握手中/房间表查不到）。</summary>
        public int RoomId { get; }
        public string EndPoint { get; }
        /// <summary>Null until LiteNetLib reports a first latency sample.</summary>
        public int? LatencyMilliseconds { get; }
        public TrafficObservation Statistics { get; }
        public int NetworkErrorCount { get; }
        public bool HasHandshaked { get; }

        public ConnectionObservation(int playerId, string name, Platform platform, bool isInGame,
            int roomId, string endPoint, int? latencyMilliseconds, TrafficObservation statistics,
            int networkErrorCount, bool hasHandshaked)
        {
            PlayerId = playerId; Name = name; Platform = platform; IsInGame = isInGame;
            RoomId = roomId; EndPoint = endPoint; LatencyMilliseconds = latencyMilliseconds;
            Statistics = statistics; NetworkErrorCount = networkErrorCount; HasHandshaked = hasHandshaked;
        }
    }

    /// <summary>Room directory row: public metadata plus the owner's display name.</summary>
    public sealed class RoomObservation
    {
        public RoomInfo Info { get; }
        public string OwnerName { get; }

        public RoomObservation(RoomInfo info, string ownerName)
        { Info = info; OwnerName = ownerName; }
    }

    /// <summary>One relayed chat message, stored per room in memory only.</summary>
    public sealed class ChatMessageObservation
    {
        public long Timestamp { get; }
        public int PlayerId { get; }
        public string PlayerName { get; }
        public string Content { get; }

        public ChatMessageObservation(long timestamp, int playerId, string playerName, string content)
        { Timestamp = timestamp; PlayerId = playerId; PlayerName = playerName; Content = content; }
    }

    /// <summary>Immutable view of everything the console may display at one instant.</summary>
    public sealed class ServerObservationSnapshot
    {
        public bool IsRunning { get; }
        public DateTime? StartedAt { get; }
        public TimeSpan? Uptime { get; }
        public long PollCount { get; }
        public TimeSpan MaxPollGap { get; }
        /// <summary>Socket errors that could not be matched to a live connection by endpoint.</summary>
        public int UnattributedNetworkErrors { get; }
        public TrafficObservation Traffic { get; }
        public ReadOnlyCollection<ConnectionObservation> Connections { get; }
        public ReadOnlyCollection<RoomObservation> Rooms { get; }
        public ReadOnlyCollection<ChatMessageObservation> LobbyChat { get; }
        /// <summary>
        /// Chat keyed by room id; only rooms alive at snapshot time appear. A plain Dictionary
        /// (not ReadOnlyDictionary) because Server also builds against net35; treat as read-only.
        /// </summary>
        public Dictionary<int, ReadOnlyCollection<ChatMessageObservation>> RoomChat { get; }

        public ServerObservationSnapshot(bool isRunning, DateTime? startedAt, TimeSpan? uptime, long pollCount,
            TimeSpan maxPollGap, int unattributedNetworkErrors, TrafficObservation traffic,
            ReadOnlyCollection<ConnectionObservation> connections, ReadOnlyCollection<RoomObservation> rooms,
            ReadOnlyCollection<ChatMessageObservation> lobbyChat,
            Dictionary<int, ReadOnlyCollection<ChatMessageObservation>> roomChat)
        {
            IsRunning = isRunning; StartedAt = startedAt; Uptime = uptime;
            PollCount = pollCount; MaxPollGap = maxPollGap; UnattributedNetworkErrors = unattributedNetworkErrors;
            Traffic = traffic;
            Connections = connections; Rooms = rooms; LobbyChat = lobbyChat; RoomChat = roomChat;
        }
    }

    /// <summary>Constructor-injected so observation tests can drive expiry without sleeping.</summary>
    public delegate DateTime ObservationClock();
}
