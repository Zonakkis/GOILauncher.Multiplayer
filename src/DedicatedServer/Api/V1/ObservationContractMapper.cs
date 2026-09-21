using System;
using System.Collections.Generic;
using System.Linq;
using GOILauncher.Multiplayer.Core.Data.Constants;
using GOILauncher.Multiplayer.Server.Services;

namespace GOILauncher.Multiplayer.DedicatedServer.Api.V1
{
    public static class ObservationContractMapper
    {
        public const int SchemaVersion = 1;

        public static ObservationSnapshotDto Map(
            ServerObservationSnapshot snapshot,
            int gamePort,
            DateTimeOffset generatedAt)
        {
            var chat = new Dictionary<int, IReadOnlyList<ChatMessageDto>>
            {
                [RoomConstants.LobbyId] = MapChat(snapshot.LobbyChat)
            };

            foreach (var room in snapshot.RoomChat)
                chat[room.Key] = MapChat(room.Value);

            return new ObservationSnapshotDto(
                SchemaVersion,
                generatedAt,
                new ServerStatusDto(
                    snapshot.IsRunning,
                    ToDateTimeOffset(snapshot.StartedAt),
                    snapshot.Uptime.HasValue ? (long?)snapshot.Uptime.Value.TotalSeconds : null,
                    snapshot.PollCount,
                    (long)snapshot.MaxPollGap.TotalMilliseconds,
                    snapshot.UnattributedNetworkErrors,
                    gamePort),
                snapshot.Rooms
                    .OrderBy(room => room.Info.Id)
                    .Select(MapRoom)
                    .ToList(),
                snapshot.Connections
                    .OrderBy(connection => connection.PlayerId)
                    .Select(MapConnection)
                    .ToList(),
                chat);
        }

        private static RoomDto MapRoom(RoomObservation room)
        {
            var info = room.Info;
            return new RoomDto(
                info.Id,
                info.Name,
                info.IsLobby,
                info.HasPassword,
                info.PlayerCount,
                info.MaxPlayers > 0 ? info.MaxPlayers : (int?)null,
                info.OwnerPlayerId,
                room.OwnerName);
        }

        private static ConnectionDto MapConnection(ConnectionObservation connection)
        {
            return new ConnectionDto(
                connection.PlayerId,
                connection.Name,
                connection.HasHandshaked ? connection.Platform.ToString() : null,
                connection.HasHandshaked,
                connection.IsInGame,
                connection.RoomId >= 0 ? connection.RoomId : (int?)null,
                string.IsNullOrEmpty(connection.EndPoint) ? null : connection.EndPoint,
                connection.LatencyMilliseconds,
                MapTraffic(connection.Statistics),
                connection.NetworkErrorCount);
        }

        private static ConnectionTrafficDto MapTraffic(ConnectionStatsObservation statistics)
        {
            if (statistics == null) return null;

            return new ConnectionTrafficDto(
                statistics.PacketsSent,
                statistics.PacketsReceived,
                statistics.BytesSent,
                statistics.BytesReceived,
                statistics.PacketLoss,
                statistics.PacketLossPercent);
        }

        private static IReadOnlyList<ChatMessageDto> MapChat(IEnumerable<ChatMessageObservation> messages)
        {
            return messages
                .Select(message => new ChatMessageDto(
                    DateTimeOffset.FromUnixTimeSeconds(message.Timestamp),
                    message.PlayerId,
                    message.PlayerName,
                    message.Content))
                .ToList();
        }

        private static DateTimeOffset? ToDateTimeOffset(DateTime? value)
        {
            return value.HasValue ? new DateTimeOffset(value.Value) : (DateTimeOffset?)null;
        }
    }
}
