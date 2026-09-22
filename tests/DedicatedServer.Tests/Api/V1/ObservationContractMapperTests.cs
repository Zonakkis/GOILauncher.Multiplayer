using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.DedicatedServer.Api.V1;
using GOILauncher.Multiplayer.Server.Services;
using NUnit.Framework;

namespace GOILauncher.Multiplayer.DedicatedServer.Tests.Api.V1
{
    [TestFixture]
    public class ObservationContractMapperTests
    {
        [Test]
        public void Map_ProducesRawVersionedContract()
        {
            var snapshot = new ServerObservationSnapshot(
                true,
                new DateTime(2026, 9, 21, 1, 2, 3, DateTimeKind.Utc),
                TimeSpan.FromSeconds(5),
                42,
                TimeSpan.FromMilliseconds(17),
                2,
                new TrafficObservation(400, 300, 16000, 12000, 4, 1),
                new ReadOnlyCollection<ConnectionObservation>(new[]
                {
                    new ConnectionObservation(
                        7,
                        "player",
                        Platform.PC,
                        true,
                        -1,
                        "127.0.0.1:9027",
                        null,
                        new TrafficObservation(120, 118, 4800, 4600, 2, 1),
                        1,
                        true)
                }),
                new ReadOnlyCollection<RoomObservation>(new[]
                {
                    new RoomObservation(new RoomInfo(3, "room", true, 0, 1, 7), "player")
                }),
                new ReadOnlyCollection<ChatMessageObservation>(new[]
                {
                    new ChatMessageObservation(1_700_000_000, 7, "player", "hello")
                }),
                new Dictionary<int, ReadOnlyCollection<ChatMessageObservation>>());

            var generatedAt = new DateTimeOffset(2026, 9, 21, 1, 2, 4, TimeSpan.Zero);
            var dto = ObservationContractMapper.Map(snapshot, 9027, generatedAt);

            dto.SchemaVersion.Should().Be(1);
            dto.GeneratedAt.Should().Be(generatedAt);
            dto.Server.GamePort.Should().Be(9027);
            dto.Server.UtcOffsetMinutes.Should().Be(
                (int)TimeZoneInfo.Local.GetUtcOffset(generatedAt.UtcDateTime).TotalMinutes);
            dto.Traffic.PacketsSent.Should().Be(400);
            dto.Traffic.PacketsReceived.Should().Be(300);
            dto.Traffic.BytesSent.Should().Be(16000);
            dto.Traffic.BytesReceived.Should().Be(12000);
            dto.Traffic.PacketLoss.Should().Be(4);
            dto.Traffic.PacketLossPercent.Should().Be(1);
            dto.Rooms.Single().MaxPlayers.Should().BeNull();
            dto.Connections.Single().RoomId.Should().BeNull();
            dto.Connections.Single().Platform.Should().Be("PC");
            dto.Connections.Single().Traffic.PacketLossPercent.Should().Be(1);
            dto.Chat[0].Single().Content.Should().Be("hello");
        }

        [Test]
        public void Map_SerializesWithWebFriendlyJsonShape()
        {
            var snapshot = new ServerObservationSnapshot(
                false,
                null,
                null,
                0,
                TimeSpan.Zero,
                0,
                TrafficObservation.Empty,
                new ReadOnlyCollection<ConnectionObservation>(Array.Empty<ConnectionObservation>()),
                new ReadOnlyCollection<RoomObservation>(Array.Empty<RoomObservation>()),
                new ReadOnlyCollection<ChatMessageObservation>(Array.Empty<ChatMessageObservation>()),
                new Dictionary<int, ReadOnlyCollection<ChatMessageObservation>>());

            var json = JsonSerializer.Serialize(
                ObservationContractMapper.Map(snapshot, 9027, DateTimeOffset.UnixEpoch),
                new JsonSerializerOptions(JsonSerializerDefaults.Web));

            json.Should().Contain("\"schemaVersion\":1");
            json.Should().Contain("\"generatedAt\":\"1970-01-01T00:00:00+00:00\"");
            json.Should().Contain("\"utcOffsetMinutes\":");
            json.Should().Contain("\"traffic\":{\"packetsSent\":0");
            json.Should().Contain("\"chat\":{\"0\":[]}");
        }
    }
}
