using System.Collections.Generic;
using System.Linq;
using Autofac;
using FluentAssertions;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Constants;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Extensions;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Network;
using GOILauncher.Multiplayer.Server.Events;
using GOILauncher.Multiplayer.Server.Extensions;
using GOILauncher.Multiplayer.Server.Services;
using GOILauncher.Multiplayer.Server.Synchronization;
using GOILauncher.Multiplayer.Tests.Rooms;
using LiteNetLib;
using Moq;
using NUnit.Framework;

namespace GOILauncher.Multiplayer.Tests.Server
{
    [TestFixture]
    public class OpeningRelayTests
    {
        private const int SenderId = 1;
        private const int OtherId = 2;

        private FakeNetworkServer _network;
        private RecordingServerDispatcher _dispatcher;
        private StubPlayerService _players;
        private ServerEventBus _events;
        private double _now;

        [SetUp]
        public void Setup()
        {
            _now = 100.0;
            _network = new FakeNetworkServer();
            _dispatcher = new RecordingServerDispatcher();
            _players = new StubPlayerService();
            _events = new ServerEventBus(new Mock<ILogger<EventBus>>().Object);
            var relay = new OpeningRelay(_network, _dispatcher, _players, new LobbyRoomStub(_players),
                _events, new Mock<ILogger<OpeningRelay>>().Object, () => _now);
            ((IStartable)relay).Start();
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(97)]
        public void AcceptedState_UsesPacketSenderIdentity_AndRelaysToOtherMembersIncludingThoseOutsideGame(int senderId)
        {
            AddPlayer(senderId);
            AddPlayer(OtherId);
            AddPlayer(3, false);

            Announce(senderId, 6.5f);

            _network.Sent.Select(s => s.ClientId).Should().BeEquivalentTo(new[] { OtherId, 3 });
            AssertOpening(_network, OtherId, senderId, 6.5f, 3, (ulong)senderId + 1);
            AssertOpening(_network, 3, senderId, 6.5f, 4, (ulong)senderId + 1);
        }

        [Test]
        public void UnknownSender_IsDroppedWithoutCreatingACacheForALaterKnownPlayer()
        {
            AddPlayer(OtherId);

            Announce(SenderId, 8f);

            _network.Sent.Should().BeEmpty();
            AddPlayer(SenderId);
            _events.Publish(new PlayerRoomEnteredEvent(SenderId));
            _network.Sent.Should().BeEmpty();
        }

        [Test]
        public void SenderOutsideGame_IsDroppedWithoutCreatingACache()
        {
            AddPlayer(SenderId, false);
            AddPlayer(OtherId);

            Announce(SenderId, 8f);

            _network.Sent.Should().BeEmpty();
            AddPlayer(SenderId);
            _events.Publish(new PlayerRoomEnteredEvent(SenderId));
            _network.Sent.Should().BeEmpty();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void RejectedSource_DoesNotEraseOrReplaceAnEarlierGoodCountdown(bool sourceIsUnknown)
        {
            AddPlayer(SenderId);
            AddPlayer(OtherId);
            Announce(SenderId, 5f);
            _network.Sent.Clear();
            _now = 101.0;
            // Deliberately no lifecycle event: packet rejection must not mutate the cache.
            if (sourceIsUnknown) _players.RemovePlayer(SenderId);
            else AddPlayer(SenderId, false);

            Announce(SenderId, 90f);

            _network.Sent.Should().BeEmpty();
            AddPlayer(SenderId);
            _now = 102.0;
            AddPlayer(3, false);
            _events.Publish(new PlayerRoomEnteredEvent(3));
            _network.Sent.Should().ContainSingle();
            AssertOpening(_network, 3, SenderId, 3f, 4, 2);
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void InvalidDuration_NeitherRelaysNorPoisonsTheEarlierGoodCountdown(float invalidSeconds)
        {
            AddPlayer(SenderId);
            AddPlayer(OtherId);
            Announce(SenderId, 5f);
            _network.Sent.Clear();
            _now = 101.0;

            Announce(SenderId, invalidSeconds);

            _network.Sent.Should().BeEmpty();
            _now = 102.0;
            AddPlayer(3, false);
            _events.Publish(new PlayerRoomEnteredEvent(3));
            _network.Sent.Should().ContainSingle();
            AssertOpening(_network, 3, SenderId, 3f, 4, 2);
        }

        [TestCase(102.5, 2.5f)]
        [TestCase(105.0, 0f)]
        [TestCase(110.0, 0f)]
        public void LateJoin_ReceivesTimeRemainingOnTheServerClock_EvenWhenNobodyWasPresentAtCapture(
            double joinTime, float expectedSeconds)
        {
            AddPlayer(SenderId);
            Announce(SenderId, 5f);
            _network.Sent.Should().BeEmpty();
            _now = joinTime;

            AddPlayer(OtherId, false);
            _events.Publish(new PlayerRoomEnteredEvent(OtherId));

            _network.Sent.Should().ContainSingle();
            AssertOpening(_network, OtherId, SenderId, expectedSeconds, 3, 2);
        }

        [Test]
        public void LateJoins_AgeEachPlayersOwnCapture_WithoutRestartingOrDoubleSubtractingOnReplay()
        {
            AddPlayer(SenderId);
            Announce(SenderId, 6.5f);
            _now = 101.5;
            AddPlayer(OtherId);
            Announce(OtherId, 8f);
            _network.Sent.Clear();
            _now = 103.25;

            AddPlayer(3, false);
            _events.Publish(new PlayerRoomEnteredEvent(3));

            _network.Sent.Should().HaveCount(2);
            AssertOpening(_network, 3, SenderId, 3.25f, 4, 2);
            AssertOpening(_network, 3, OtherId, 6.25f, 4, 3);

            _network.Sent.Clear();
            _now = 104.0;
            AddPlayer(4, false);
            _events.Publish(new PlayerRoomEnteredEvent(4));

            _network.Sent.Should().HaveCount(2);
            AssertOpening(_network, 4, SenderId, 2.5f, 5, 2);
            AssertOpening(_network, 4, OtherId, 5.5f, 5, 3);
        }

        [Test]
        public void ExplicitZero_IsRelayedAndRememberedAsCompleted_NotAsMissingState()
        {
            AddPlayer(SenderId);
            AddPlayer(OtherId);

            Announce(SenderId, 0f);

            _network.Sent.Should().ContainSingle();
            AssertOpening(_network, OtherId, SenderId, 0f, 3, 2);
            _network.Sent.Clear();
            _now = 200.0;
            AddPlayer(3, false);
            _events.Publish(new PlayerRoomEnteredEvent(3));
            _network.Sent.Should().ContainSingle();
            AssertOpening(_network, 3, SenderId, 0f, 4, 2);
        }

        [TestCase(0f, 0f)]
        [TestCase(2f, 1f)]
        [TestCase(8f, 7f)]
        public void NewDuration_ReplacesThePreviousCountdown(float newSeconds, float expectedSeconds)
        {
            AddPlayer(SenderId);
            AddPlayer(OtherId);
            Announce(SenderId, 5f);
            _network.Sent.Clear();
            _now = 102.0;

            Announce(SenderId, newSeconds);

            _network.Sent.Should().ContainSingle();
            AssertOpening(_network, OtherId, SenderId, newSeconds, 3, 2);
            _network.Sent.Clear();
            _now = 103.0;
            AddPlayer(3);
            _events.Publish(new PlayerRoomEnteredEvent(3));
            _network.Sent.Should().ContainSingle();
            AssertOpening(_network, 3, SenderId, expectedSeconds, 4, 2);
        }

        [Test]
        public void RepeatedEqualDuration_RestartsTheCountdown_AndIsRelayedAgain()
        {
            AddPlayer(SenderId);
            AddPlayer(OtherId);
            Announce(SenderId, 5f);
            _network.Sent.Clear();
            _now = 102.0;

            Announce(SenderId, 5f);

            _network.Sent.Should().ContainSingle();
            AssertOpening(_network, OtherId, SenderId, 5f, 3, 2);
            _network.Sent.Clear();
            _now = 103.0;
            AddPlayer(3);
            _events.Publish(new PlayerRoomEnteredEvent(3));
            _network.Sent.Should().ContainSingle();
            AssertOpening(_network, 3, SenderId, 4f, 4, 2);
        }

        [TestCase(0f)]
        [TestCase(2f)]
        public void CompletedCountdown_CanBeReplacedByANewOpening(float initialSeconds)
        {
            AddPlayer(SenderId);
            AddPlayer(OtherId);
            Announce(SenderId, initialSeconds);
            _network.Sent.Clear();
            _now = 110.0;

            Announce(SenderId, 5f);

            _network.Sent.Should().ContainSingle();
            AssertOpening(_network, OtherId, SenderId, 5f, 3, 2);
            _network.Sent.Clear();
            _now = 111.0;
            AddPlayer(3);
            _events.Publish(new PlayerRoomEnteredEvent(3));
            _network.Sent.Should().ContainSingle();
            AssertOpening(_network, 3, SenderId, 4f, 4, 2);
        }

        [Test]
        public void RoomEntry_DoesNotEchoTheNewcomersOwnCachedState()
        {
            AddPlayer(SenderId);
            Announce(SenderId, 5f);

            _events.Publish(new PlayerRoomEnteredEvent(SenderId));

            _network.Sent.Should().BeEmpty();
        }

        [Test]
        public void InGameStatusEvent_DoesNotClearOrRestartAnExistingCountdown()
        {
            AddPlayer(SenderId);
            Announce(SenderId, 5f);
            _now = 101.0;

            PlayerInfo sender;
            _players.TryGetPlayer(SenderId, out sender);
            _events.Publish(new PlayerStatusChangedEvent(sender));

            _now = 102.0;
            AddPlayer(OtherId, false);
            _events.Publish(new PlayerRoomEnteredEvent(OtherId));
            _network.Sent.Should().ContainSingle();
            AssertOpening(_network, OtherId, SenderId, 3f, 3, 2);
        }

        [Test]
        public void RoomEntry_ReplaysBothDirectionsAfterOrderedRosters_WithCurrentScopesAndNoCrossRoomLeak()
        {
            var server = CreateRoomServer();
            server.Add(1);
            server.Add(2);
            server.Add(3);
            server.Add(4, false);
            server.Add(5);
            var room = server.Create(2);
            server.Join(3, room);
            server.Join(4, room);
            Announce(server, 1, 8f);
            Announce(server, 2, 10f);
            Announce(server, 3, 12f);
            Announce(server, 5, 15f);
            var oldSourceMembership = server.Member(1).Id;
            _now = 102.0;

            server.Join(1, room);

            var opening = OpeningPackets(server.Network);
            opening.Should().HaveCount(5);
            server.Member(1).Id.Should().NotBe(oldSourceMembership);
            AssertOpening(server.Network, 1, 2, 8f, server.Member(1).Id, server.Member(2).Id);
            AssertOpening(server.Network, 1, 3, 10f, server.Member(1).Id, server.Member(3).Id);
            foreach (var recipient in new[] { 2, 3, 4 })
                AssertOpening(server.Network, recipient, 1, 6f, server.Member(recipient).Id, server.Member(1).Id);

            // Each receiver must first learn the source's current membership on the same stream.
            foreach (var sent in opening)
            {
                var roster = server.Network.Sent.Single(s => s.ClientId == sent.ClientId &&
                    (sent.ClientId == 1 ? s.Packet is S2CPlayerListPacket : s.Packet is S2CPlayerJoinedPacket));
                roster.Channel.Should().Be(NetworkChannels.Default);
                roster.Method.Should().Be(DeliveryMethod.ReliableOrdered);
                server.Network.Sent.IndexOf(roster).Should().BeLessThan(server.Network.Sent.IndexOf(sent));
            }
        }

        [Test]
        public void LiveRelay_UsesCurrentRoomOnly_IncludingNonGameReceivers()
        {
            var server = CreateRoomServer();
            server.Add(1);
            server.Add(2);
            server.Add(3, false);
            server.Add(4);
            var room = server.Create(1);
            server.Join(2, room);
            server.Join(3, room);
            server.Network.Sent.Clear();

            Announce(server, 1, 4f);

            server.Network.Sent.Select(s => s.ClientId).Should().BeEquivalentTo(new[] { 2, 3 });
            AssertOpening(server.Network, 2, 1, 4f, server.Member(2).Id, server.Member(1).Id);
            AssertOpening(server.Network, 3, 1, 4f, server.Member(3).Id, server.Member(1).Id);
        }

        [Test]
        public void CachedState_FollowsTheSourcesCurrentMembership_NotTheRoomWhereItWasCaptured()
        {
            var server = CreateRoomServer();
            server.Add(1);
            server.Add(2);
            Announce(server, 1, 10f);
            Announce(server, 2, 7f);
            var room = server.Create(1);
            _now = 101.0;
            server.Network.Sent.Clear();

            server.Add(3, false);

            OpeningPackets(server.Network).Should().ContainSingle();
            AssertOpening(server.Network, 3, 2, 6f, server.Member(3).Id, server.Member(2).Id);

            server.Add(4, false);
            server.Join(4, room);

            OpeningPackets(server.Network).Should().ContainSingle();
            AssertOpening(server.Network, 4, 1, 9f, server.Member(4).Id, server.Member(1).Id);
        }

        [Test]
        public void ReturningToARoom_ReplaysWithFreshSenderAndRecipientMemberships_WithoutRestartingTheCache()
        {
            var server = CreateRoomServer();
            server.Add(1);
            server.Add(2);
            Announce(server, 1, 9f);
            var originalSource = server.Member(1).Id;
            var originalRecipient = server.Member(2).Id;
            _now = 101.0;
            server.Create(1);
            _now = 102.0;

            server.Join(1, RoomConstants.LobbyId);

            OpeningPackets(server.Network).Should().ContainSingle();
            server.Member(1).Id.Should().NotBe(originalSource);
            AssertOpening(server.Network, 2, 1, 7f, originalRecipient, server.Member(1).Id);

            _now = 103.0;
            server.Create(2);
            _now = 104.0;
            server.Join(2, RoomConstants.LobbyId);

            OpeningPackets(server.Network).Should().ContainSingle();
            server.Member(2).Id.Should().NotBe(originalRecipient);
            AssertOpening(server.Network, 2, 1, 5f, server.Member(2).Id, server.Member(1).Id);
        }

        [TestCase("old")]
        [TestCase("zero")]
        [TestCase("other-player")]
        public void InvalidMembership_NeitherRelaysIntoTheNewRoomNorPoisonsTheRetainedCountdown(string membershipKind)
        {
            var server = CreateRoomServer();
            server.Add(1);
            server.Add(2);
            var oldMembership = server.Member(1).Id;
            Announce(server, 1, 10f);
            var room = server.Create(2);
            _now = 101.0;
            server.Join(1, room);
            server.Network.Sent.Clear();
            _now = 102.0;
            var invalidMembership = membershipKind == "old" ? oldMembership :
                membershipKind == "zero" ? 0UL : server.Member(2).Id;

            server.Dispatcher.Receive(new C2SOpeningStatePacket
            {
                MembershipId = invalidMembership,
                State = new OpeningState { RemainingSeconds = 90f }
            }, 1);

            server.Network.Sent.Should().BeEmpty();
            _now = 103.0;
            server.Add(3, false);
            server.Join(3, room);
            OpeningPackets(server.Network).Should().ContainSingle();
            AssertOpening(server.Network, 3, 1, 7f, server.Member(3).Id, server.Member(1).Id);
        }

        [Test]
        public void QuitGame_RemovesOnlyThatSourcesCache_AndEnteringGameDoesNotReviveIt()
        {
            var server = CreateRoomServer();
            server.Add(1);
            server.Add(2);
            Announce(server, 1, 8f);
            Announce(server, 2, 10f);

            server.Dispatcher.Receive(new C2SIsInGameUpdatePacket(false), 1);
            server.Dispatcher.Receive(new C2SIsInGameUpdatePacket(true), 1);

            server.Network.Sent.Clear();
            _now = 102.0;
            server.Add(3, false);
            OpeningPackets(server.Network).Should().ContainSingle();
            AssertOpening(server.Network, 3, 2, 8f, server.Member(3).Id, server.Member(2).Id);
        }

        [Test]
        public void Disconnect_RemovesOnlyThatSourcesCache_EvenWhenThePlayerIdIsReused()
        {
            var server = CreateRoomServer();
            server.Add(1);
            server.Add(2);
            Announce(server, 1, 8f);
            Announce(server, 2, 10f);

            server.Events.Publish(new ClientDisconnectedEvent(1, "left"));

            server.Network.Sent.Clear();
            _now = 102.0;
            server.Add(1);
            OpeningPackets(server.Network).Should().ContainSingle();
            AssertOpening(server.Network, 1, 2, 8f, server.Member(1).Id, server.Member(2).Id);
        }

        [Test]
        public void ServerStop_ClearsEveryCountdownWithoutDisconnectEvents_AndAcceptsNewStateAfterRestart()
        {
            var server = CreateRoomServer();
            server.Add(1);
            server.Add(2);
            Announce(server, 1, 8f);
            Announce(server, 2, 10f);

            server.Events.Publish(new ServerStoppedEvent());

            server.Network.Sent.Clear();
            _now = 101.0;
            server.Add(1);
            server.Add(2);
            server.Add(3, false);
            OpeningPackets(server.Network).Should().BeEmpty();

            server.Network.Sent.Clear();
            Announce(server, 1, 3f);
            server.Network.Sent.Should().HaveCount(2);
            AssertOpening(server.Network, 2, 1, 3f, server.Member(2).Id, server.Member(1).Id);
            AssertOpening(server.Network, 3, 1, 3f, server.Member(3).Id, server.Member(1).Id);
        }

        [Test]
        public void WithServer_ActivatesOpeningRelayWithoutManualResolution_AndSharesOneInstance()
        {
            var network = new FakeNetworkServer();
            var dispatcher = new RecordingServerDispatcher();
            var builder = new ContainerBuilder();
            builder.RegisterMultiplayerCore().WithServer();
            builder.RegisterInstance(network).As<INetworkServer>();
            builder.RegisterInstance(dispatcher).As<IServerPacketDispatcher>();

            using (var container = builder.Build())
            {
                dispatcher.Receive(new C2SClientHandShakePacket
                    { PlayerName = "source", Platform = Platform.PC, IsInGame = true }, 1);
                dispatcher.Receive(new C2SClientHandShakePacket
                    { PlayerName = "recipient", Platform = Platform.PC, IsInGame = false }, 2);
                RoomMembership source, recipient;
                var rooms = container.Resolve<IRoomService>();
                rooms.TryGetMembership(1, out source).Should().BeTrue();
                rooms.TryGetMembership(2, out recipient).Should().BeTrue();
                network.Sent.Clear();

                // Zero gives an exact assertion using the production constructor's real clock.
                dispatcher.Receive(new C2SOpeningStatePacket
                {
                    MembershipId = source.Id,
                    State = new OpeningState { RemainingSeconds = 0f }
                }, 1);

                network.Sent.Should().ContainSingle();
                AssertOpening(network, 2, 1, 0f, recipient.Id, source.Id);
                var relay = container.Resolve<OpeningRelay>();
                container.Resolve<OpeningRelay>().Should().BeSameAs(relay);
                container.Resolve<IEnumerable<IStartable>>().OfType<OpeningRelay>()
                    .Should().ContainSingle().Which.Should().BeSameAs(relay);
            }
        }

        private void AddPlayer(int id, bool isInGame = true)
        {
            _players.AddPlayer(id, isInGame);
        }

        private void Announce(int playerId, float seconds)
        {
            _dispatcher.Receive(new C2SOpeningStatePacket
            {
                MembershipId = (ulong)playerId + 1,
                State = new OpeningState { RemainingSeconds = seconds }
            }, playerId);
        }

        private RoomServerFixture CreateRoomServer()
        {
            var server = new RoomServerFixture();
            var relay = new OpeningRelay(server.Network, server.Dispatcher, server.Players, server.Rooms,
                server.Events, new Mock<ILogger<OpeningRelay>>().Object, () => _now);
            ((IStartable)relay).Start();
            return server;
        }

        private static void Announce(RoomServerFixture server, int playerId, float seconds)
        {
            server.Dispatcher.Receive(new C2SOpeningStatePacket
            {
                MembershipId = server.Member(playerId).Id,
                State = new OpeningState { RemainingSeconds = seconds }
            }, playerId);
        }

        private static List<SentPacket> OpeningPackets(FakeNetworkServer network)
        {
            return network.Sent.Where(s => s.Packet is S2COpeningStatePacket).ToList();
        }

        private static void AssertOpening(FakeNetworkServer network, int recipientId, int playerId,
            float seconds, ulong recipientMembership, ulong playerMembership)
        {
            var sent = OpeningPackets(network).Where(s => s.ClientId == recipientId &&
                ((S2COpeningStatePacket)s.Packet).PlayerId == playerId).Should().ContainSingle().Subject;
            sent.Channel.Should().Be(NetworkChannels.Default);
            sent.Method.Should().Be(DeliveryMethod.ReliableOrdered);
            var packet = (S2COpeningStatePacket)sent.Packet;
            packet.State.RemainingSeconds.Should().Be(seconds);
            packet.Scope.RecipientMembershipId.Should().Be(recipientMembership);
            packet.Scope.PlayerMembershipId.Should().Be(playerMembership);
        }

        private sealed class StubPlayerService : IPlayerService
        {
            private readonly Dictionary<int, PlayerInfo> _players = new Dictionary<int, PlayerInfo>();

            public IEnumerable<PlayerInfo> Players { get { return _players.Values; } }

            public bool TryGetPlayer(int playerId, out PlayerInfo player)
            {
                return _players.TryGetValue(playerId, out player);
            }

            public void AddPlayer(int id, bool isInGame)
            {
                _players[id] = new PlayerInfo(id, "p" + id, Platform.PC, isInGame);
            }

            public void RemovePlayer(int id)
            {
                _players.Remove(id);
            }
        }
    }
}
