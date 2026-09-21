using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Autofac;
using FluentAssertions;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Network;
using GOILauncher.Multiplayer.Server.Services;
using Moq;
using NUnit.Framework;

namespace GOILauncher.Multiplayer.Tests.Server
{
    /// <summary>
    /// The observation facade: a read-only view assembled from server events (Poll thread) and
    /// frozen snapshots (any thread), with chat stored per room and expiring with it.
    /// </summary>
    [TestFixture]
    public class ServerObservationTests
    {
        private sealed class Harness
        {
            public readonly FakeNetworkServer Network = new FakeNetworkServer();
            public readonly RecordingServerDispatcher Dispatcher = new RecordingServerDispatcher();
            public readonly ServerEventBus Events = new ServerEventBus(new Mock<ILogger<EventBus>>().Object);
            public readonly PlayerService Players;
            public readonly RoomService Rooms;
            public readonly ObservationService Observation;
            private readonly ChatService _chat;
            public DateTime Now = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            private uint _requestId;

            public Harness()
            {
                Players = new PlayerService(Dispatcher, Events);
                Rooms = new RoomService(Players, Network, Dispatcher, Events, new Mock<ILogger<RoomService>>().Object);
                _chat = new ChatService(Rooms, Players, Network, Dispatcher, Events);
                Observation = new ObservationService(Players, Rooms, Network, Events,
                    new Mock<ILogger<ObservationService>>().Object, () => Now);
                foreach (var module in new IStartable[] { Players, Rooms, _chat, Observation }) module.Start();
                Network.IsRunning = true;
            }

            public void Connect(int id, IPEndPoint endPoint = null)
            {
                Events.Publish(new ClientConnectedEvent(id, endPoint ?? new IPEndPoint(IPAddress.Loopback, 5000 + id)));
                Dispatcher.Receive(new C2SClientHandShakePacket
                { PlayerName = "p" + id, Platform = Platform.PC, IsInGame = false }, id);
            }

            public RoomMembership Member(int id)
            {
                RoomMembership member;
                Assert.That(Rooms.TryGetMembership(id, out member), Is.True);
                return member;
            }

            public int CreateRoom(int id, string name = "Room")
            {
                Dispatcher.Receive(new C2SRoomOperationPacket
                {
                    RequestId = ++_requestId, MembershipId = Member(id).Id, Operation = RoomOperation.Create,
                    Name = name, MaxPlayers = 0, PasswordChange = RoomPasswordChange.Remove
                }, id);
                return Member(id).RoomId;
            }

            public void Say(int id, string text)
            {
                Dispatcher.Receive(new C2SChatMessagePacket { MembershipId = Member(id).Id, Content = text, Timestamp = 0 }, id);
            }

            public void ObservationMarkPoll() { Observation.MarkPoll(); }

            /// <summary>Read the last frozen caches without advancing the poll/liveness counters.</summary>
            public ServerObservationSnapshot Peek() { return Observation.Snapshot; }

            public ServerObservationSnapshot Snapshot()
            {
                Observation.MarkPoll();
                return Observation.Snapshot;
            }
        }

        [Test]
        public void Snapshot_JoinsIdentityTransportAndTraffic_IntoOneRow()
        {
            var h = new Harness();
            h.Connect(1);
            h.Events.Publish(new NetworkLatencyUpdatedEvent(1, 42));
            h.Network.PeerTraffic = new List<PeerTraffic>
            {
                new PeerTraffic(1, packetsSent: 100, packetsReceived: 80, bytesSent: 4000,
                    bytesReceived: 3200, packetLoss: 2, packetLossPercent: 2)
            };

            var conn = h.Snapshot().Connections.Single();

            conn.PlayerId.Should().Be(1);
            conn.Name.Should().Be("p1");
            conn.HasHandshaked.Should().BeTrue();
            conn.EndPoint.Should().Be("127.0.0.1:5001");
            conn.LatencyMilliseconds.Should().Be(42);
            conn.Statistics.Should().NotBeNull();
            conn.Statistics.BytesReceived.Should().Be(3200);
            conn.Statistics.PacketLossPercent.Should().Be(2);
        }

        [Test]
        public void Snapshot_ExposesGlobalLiteNetLibTraffic()
        {
            var h = new Harness();
            h.Network.TotalTraffic = new ServerTraffic(
                packetsSent: 400, packetsReceived: 300, bytesSent: 16000,
                bytesReceived: 12000, packetLoss: 4, packetLossPercent: 1);

            var traffic = h.Snapshot().Traffic;

            traffic.PacketsSent.Should().Be(400);
            traffic.PacketsReceived.Should().Be(300);
            traffic.BytesSent.Should().Be(16000);
            traffic.BytesReceived.Should().Be(12000);
            traffic.PacketLoss.Should().Be(4);
            traffic.PacketLossPercent.Should().Be(1);
        }

        [Test]
        public void ConnectedWithoutHandshake_StillShowsAsHalfOpenRow()
        {
            var h = new Harness();
            h.Events.Publish(new ClientConnectedEvent(7, new IPEndPoint(IPAddress.Loopback, 6007)));

            var conn = h.Snapshot().Connections.Single();

            conn.PlayerId.Should().Be(7);
            conn.HasHandshaked.Should().BeFalse();
            conn.Name.Should().BeNull();
            conn.EndPoint.Should().Be("127.0.0.1:6007");
        }

        [Test]
        public void NetworkError_AttributedToConnectionByEndpoint()
        {
            var h = new Harness();
            h.Connect(1); // endpoint 127.0.0.1:5001
            h.Events.Publish(new NetworkErrorEvent(new IPEndPoint(IPAddress.Loopback, 5001), System.Net.Sockets.SocketError.ConnectionReset));

            h.Snapshot().Connections.Single().NetworkErrorCount.Should().Be(1);
        }

        [Test]
        public void NetworkError_WithoutMatchingConnection_CountsAsUnattributed()
        {
            var h = new Harness();
            h.Events.Publish(new NetworkErrorEvent(new IPEndPoint(IPAddress.Any, 9), System.Net.Sockets.SocketError.SocketError));

            h.Snapshot().UnattributedNetworkErrors.Should().Be(1);
        }

        [Test]
        public void ChatInLobby_SurfacedAsLobbyHistory()
        {
            var h = new Harness();
            h.Connect(1);
            h.Say(1, "hello");

            var msg = h.Snapshot().LobbyChat.Single();

            msg.PlayerName.Should().Be("p1");
            msg.Content.Should().Be("hello");
        }

        [Test]
        public void ChatIsGroupedByRoom_AndRoomChatExcludesLobby()
        {
            var h = new Harness();
            h.Connect(1);
            h.Say(1, "in lobby");
            var roomId = h.CreateRoom(1);
            h.Say(1, "in room");

            var snap = h.Snapshot();

            roomId.Should().NotBe(0);
            snap.LobbyChat.Select(m => m.Content).Should().Equal("in lobby");
            snap.RoomChat[roomId].Select(m => m.Content).Should().Equal("in room");
        }

        [Test]
        public void Chat_DisappearsWithItsRoom_WhenTheLastMemberLeaves()
        {
            var h = new Harness();
            h.Connect(1);
            var roomId = h.CreateRoom(1);
            h.Say(1, "bye bye");
            h.Snapshot().RoomChat.ContainsKey(roomId).Should().BeTrue();

            h.Events.Publish(new ClientDisconnectedEvent(1, "closed"));
            h.Now = h.Now.AddSeconds(1); // clear the cache-refresh throttle so the directory is rebuilt

            var snap = h.Snapshot();
            snap.RoomChat.ContainsKey(roomId).Should().BeFalse();
            snap.Connections.Should().BeEmpty();
        }

        [Test]
        public void Chat_ExpiresOnceOlderThanRetention()
        {
            var h = new Harness();
            h.Connect(1);
            h.CreateRoom(1);
            h.Say(1, "stale");
            h.Snapshot().RoomChat.Values.Sum(r => r.Count).Should().Be(1);

            // Jump well past the retention window; the message's (real-time) timestamp is now old.
            h.Now = DateTime.Now.AddDays(2);

            h.Snapshot().RoomChat.Values.Sum(r => r.Count).Should().Be(0);
        }

        [Test]
        public void Liveness_CountsPollsAndTracksTheWorstGap()
        {
            var h = new Harness();
            h.Now = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            h.ObservationMarkPoll();
            h.Now = h.Now.AddSeconds(3);   // a 3s stall between iterations
            h.ObservationMarkPoll();

            var snap = h.Peek();

            snap.PollCount.Should().Be(2);
            snap.MaxPollGap.Should().Be(TimeSpan.FromSeconds(3));
            snap.Uptime.Should().NotBeNull();
        }

        [Test]
        public void ServerStopped_EmptiesTheWholeView()
        {
            var h = new Harness();
            h.Connect(1);
            h.Say(1, "x");
            h.Snapshot().Connections.Should().NotBeEmpty();

            h.Events.Publish(new ServerStoppedEvent());

            var snap = h.Peek();
            snap.Connections.Should().BeEmpty();
            snap.LobbyChat.Should().BeEmpty();
            snap.PollCount.Should().Be(0);
            snap.Traffic.BytesSent.Should().Be(0);
            snap.Traffic.BytesReceived.Should().Be(0);
        }
    }
}
