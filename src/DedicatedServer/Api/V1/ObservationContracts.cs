using System;
using System.Collections.Generic;

namespace GOILauncher.Multiplayer.DedicatedServer.Api.V1
{
    /// <summary>
    /// Stable HTTP contract for the read-only observation dashboard. These DTOs are deliberately
    /// separate from the internal Server observation models so the API can evolve independently.
    /// </summary>
    public sealed record ObservationSnapshotDto(
        int SchemaVersion,
        DateTimeOffset GeneratedAt,
        ServerStatusDto Server,
        IReadOnlyList<RoomDto> Rooms,
        IReadOnlyList<ConnectionDto> Connections,
        IReadOnlyDictionary<int, IReadOnlyList<ChatMessageDto>> Chat);

    public sealed record ServerStatusDto(
        bool IsRunning,
        DateTimeOffset? StartedAt,
        long? UptimeSeconds,
        long PollCount,
        long MaxPollGapMs,
        int UnattributedNetworkErrors,
        int GamePort);

    public sealed record RoomDto(
        int Id,
        string Name,
        bool IsLobby,
        bool HasPassword,
        int PlayerCount,
        int? MaxPlayers,
        int? OwnerPlayerId,
        string OwnerName);

    public sealed record ConnectionDto(
        int PlayerId,
        string Name,
        string Platform,
        bool HasHandshaked,
        bool IsInGame,
        int? RoomId,
        string EndPoint,
        int? LatencyMs,
        ConnectionTrafficDto Traffic,
        int NetworkErrorCount);

    public sealed record ConnectionTrafficDto(
        long PacketsSent,
        long PacketsReceived,
        long BytesSent,
        long BytesReceived,
        long PacketLoss,
        long PacketLossPercent);

    public sealed record ChatMessageDto(
        DateTimeOffset Timestamp,
        int PlayerId,
        string PlayerName,
        string Content);

    public sealed record LogEntryDto(
        long Seq,
        DateTimeOffset Timestamp,
        string Time,
        string Level,
        string Logger,
        string Message);

    public sealed record LogBatchDto(
        IReadOnlyList<LogEntryDto> Rows,
        long NextSeq,
        bool Reset);
}
