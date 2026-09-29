using System.Collections.Generic;
using System.Linq;
using Autofac;
using FluentAssertions;
using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Client.Extensions;
using GOILauncher.Multiplayer.Client.Services;
using GOILauncher.Multiplayer.Client.Synchronization;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Constants;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Extensions;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Core.Utils;
using GOILauncher.Multiplayer.Network;
using LiteNetLib;
using Moq;
using NUnit.Framework;

namespace GOILauncher.Multiplayer.Tests.Client
{
    [TestFixture]
    public class ClientOpeningSyncTests
    {
        private const int RemoteId = 4;
        private const int OtherRemoteId = 9;

        private RecordingClientDispatcher _dispatcher;
        private FakeNetworkClient _networkClient;
        private ClientEventBus _eventBus;
        private IPlayerService _players;
        private ClientOpeningSync _sync;
        private List<PlayerOpeningReceivedEvent> _received;
        private double _nowSeconds;

        [SetUp]
        public void Setup()
        {
            _dispatcher = new RecordingClientDispatcher();
            _networkClient = new FakeNetworkClient();
            _eventBus = new ClientEventBus(new Mock<ILogger<EventBus>>().Object);
            _players = TestClientRoster.Create();
            _nowSeconds = 1000.0;
            OpeningClock clock = () => _nowSeconds;
            _sync = new ClientOpeningSync(_networkClient, _dispatcher, _eventBus,
                new Mock<ILogger<ClientOpeningSync>>().Object, _players, clock);
            ((IStartable)_sync).Start();

            _received = new List<PlayerOpeningReceivedEvent>();
            _eventBus.Subscribe<PlayerOpeningReceivedEvent>(e => _received.Add(e));
        }

        [Test]
        public void Announce_SendsCurrentMembershipOnReliableOrderedDefaultChannel()
        {
            _sync.Announce(State(8f));

            AssertSingleUpload(2, 8f);
            _received.Should().BeEmpty();
        }

        [Test]
        public void Announce_ZeroIsASentStateRatherThanAnAbsentSnapshot()
        {
            _sync.Announce(State(0f));

            AssertSingleUpload(2, 0f);
        }

        [Test]
        public void Announce_WhileDisconnected_DefersAndAgesUntilMembershipIsReady()
        {
            _networkClient.IsConnected = false;
            _sync.Announce(State(8f));
            _networkClient.Sent.Should().BeEmpty();

            _nowSeconds = 1002.0;
            _eventBus.Publish(new RoomMembershipChangedEvent());
            _networkClient.Sent.Should().BeEmpty();

            _nowSeconds = 1003.0;
            _networkClient.IsConnected = true;
            ReplaceMemberships(42, 43);
            _eventBus.Publish(new RoomMembershipChangedEvent());

            AssertSingleUpload(42, 5f);
        }

        [Test]
        public void Announce_WithNoMembership_WaitsPastIdentityReadyForRoomMembership()
        {
            _players.ReplaceRoomRoster(new RoomMemberInfo[0]);
            _sync.Announce(State(8f));
            _networkClient.Sent.Should().BeEmpty();

            _nowSeconds = 1001.0;
            _eventBus.Publish(new LocalPlayerReadyEvent(_players.LocalPlayer));
            _eventBus.Publish(new RoomMembershipChangedEvent());
            _networkClient.Sent.Should().BeEmpty();

            _nowSeconds = 1002.0;
            ReplaceMemberships(42, 43);
            _eventBus.Publish(new RoomMembershipChangedEvent());

            AssertSingleUpload(42, 6f);
        }

        [TestCase(0f)]
        [TestCase(4f)]
        public void DeferredSnapshot_StillUploadsKnownZeroAfterItsCountdownCompletes(float remaining)
        {
            _networkClient.IsConnected = false;
            _sync.Announce(State(remaining));

            _nowSeconds = 1030.0;
            _networkClient.IsConnected = true;
            _eventBus.Publish(new RoomMembershipChangedEvent());

            AssertSingleUpload(2, 0f);
        }

        [Test]
        public void RoomMembershipReady_WithoutALocalSnapshot_SendsNothing()
        {
            _eventBus.Publish(new RoomMembershipChangedEvent());

            _networkClient.Sent.Should().BeEmpty();
        }

        [Test]
        public void RepeatedAnnounce_WithTheSameDuration_RestartsAndSendsAgain()
        {
            _sync.Announce(State(8f));
            _nowSeconds = 1003.0;

            _sync.Announce(State(8f));

            _networkClient.Sent.Should().HaveCount(2);
            ((C2SOpeningStatePacket)_networkClient.Sent[1].Packet)
                .State.RemainingSeconds.Should().Be(8f);

            _networkClient.Sent.Clear();
            _nowSeconds = 1005.0;
            ReplaceMemberships(42, 43);
            _eventBus.Publish(new RoomMembershipChangedEvent());
            AssertSingleUpload(42, 6f);
        }

        [Test]
        public void RepeatedAnnounce_Offline_ReplacesRatherThanQueuesSnapshots()
        {
            _networkClient.IsConnected = false;
            _sync.Announce(State(20f));
            _nowSeconds = 1003.0;
            _sync.Announce(State(7f));

            _nowSeconds = 1005.0;
            _networkClient.IsConnected = true;
            _eventBus.Publish(new RoomMembershipChangedEvent());

            AssertSingleUpload(2, 5f);
        }

        [Test]
        public void Announce_ZeroReplacesAnActiveCountdownIncludingFutureReplays()
        {
            _sync.Announce(State(20f));
            _networkClient.Sent.Clear();
            _nowSeconds = 1001.0;

            _sync.Announce(State(0f));
            AssertSingleUpload(2, 0f);

            _networkClient.Sent.Clear();
            _nowSeconds = 1002.0;
            ReplaceMemberships(42, 43);
            _eventBus.Publish(new RoomMembershipChangedEvent());
            AssertSingleUpload(42, 0f);
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void Announce_InvalidState_IsNeitherSentNorSavedForLater(float remaining)
        {
            _sync.Announce(State(remaining));
            _networkClient.Sent.Should().BeEmpty();

            _eventBus.Publish(new RoomMembershipChangedEvent());
            _networkClient.Sent.Should().BeEmpty();
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void Announce_InvalidReplacement_PreservesThePreviousCountdownAndItsAge(float remaining)
        {
            _sync.Announce(State(8f));
            _networkClient.Sent.Clear();
            _nowSeconds = 1002.0;

            _sync.Announce(State(remaining));
            _networkClient.Sent.Should().BeEmpty();

            _nowSeconds = 1003.0;
            ReplaceMemberships(42, 43);
            _eventBus.Publish(new RoomMembershipChangedEvent());
            AssertSingleUpload(42, 5f);
        }

        [Test]
        public void TryGetOpening_BeforeAnySnapshot_IsUnknownEvenForAKnownPlayer()
        {
            OpeningState state;
            _sync.TryGetOpening(RemoteId, out state).Should().BeFalse();
            _sync.TryGetOpening(99, out state).Should().BeFalse();
        }

        [Test]
        public void Receive_PublishesTheSnapshotAfterMakingItReadableInTheCache()
        {
            bool? foundDuringEvent = null;
            var remainingDuringEvent = -1f;
            // EventBus catches handler exceptions: capture facts here, assert outside the handler.
            _eventBus.Subscribe<PlayerOpeningReceivedEvent>(e =>
            {
                OpeningState cached;
                foundDuringEvent = _sync.TryGetOpening(e.PlayerId, out cached);
                remainingDuringEvent = cached.RemainingSeconds;
            });

            ReceiveOpening(RemoteId, 8f);

            var published = _received.Single();
            published.PlayerId.Should().Be(RemoteId);
            published.State.RemainingSeconds.Should().Be(8f);
            foundDuringEvent.Should().Be(true);
            remainingDuringEvent.Should().Be(8f);
            _networkClient.Sent.Should().BeEmpty();
        }

        [Test]
        public void TryGetOpening_AgesOnlyFromLocalReceiveTimeWithoutRestartingOnReads()
        {
            // This arbitrary clock origin must not be interpreted as server time or packet age.
            _nowSeconds = 900000.0;
            ReceiveOpening(RemoteId, 8f);
            AssertOpening(RemoteId, 8f);

            _nowSeconds = 900002.25;
            AssertOpening(RemoteId, 5.75f);
            AssertOpening(RemoteId, 5.75f);
            _nowSeconds = 900003.0;
            AssertOpening(RemoteId, 5f);

            _received.Should().HaveCount(1);
            _received.Single().State.RemainingSeconds.Should().Be(8f);
            _networkClient.Sent.Should().BeEmpty();
        }

        [Test]
        public void TryGetOpening_ExpiredCountdownRemainsKnownZero()
        {
            ReceiveOpening(RemoteId, 4f);

            _nowSeconds = 1004.0;
            AssertOpening(RemoteId, 0f);
            _nowSeconds = 1100.0;
            AssertOpening(RemoteId, 0f);

            _received.Should().HaveCount(1);
        }

        [Test]
        public void Receive_ZeroReplacesAnActiveCountdownAndPublishesCompletion()
        {
            ReceiveOpening(RemoteId, 20f);
            _received.Clear();
            _nowSeconds = 1001.0;

            ReceiveOpening(RemoteId, 0f);

            _received.Single().State.RemainingSeconds.Should().Be(0f);
            AssertOpening(RemoteId, 0f);
            _nowSeconds = 1100.0;
            AssertOpening(RemoteId, 0f);
        }

        [TestCase(8f, 7f)]
        [TestCase(12f, 11f)]
        [TestCase(3f, 2f)]
        public void Receive_ReplacementStartsAFreshCountdownEvenForTheSameDuration(float remaining, float afterOneSecond)
        {
            ReceiveOpening(RemoteId, 8f);
            _nowSeconds = 1003.0;

            ReceiveOpening(RemoteId, remaining);

            _received.Should().HaveCount(2);
            _received[1].State.RemainingSeconds.Should().Be(remaining);
            AssertOpening(RemoteId, remaining);
            _nowSeconds = 1004.0;
            AssertOpening(RemoteId, afterOneSecond);
        }

        [Test]
        public void Receive_AfterKnownZero_CanRestartTheOpening()
        {
            ReceiveOpening(RemoteId, 0f);
            _nowSeconds = 1003.0;

            ReceiveOpening(RemoteId, 8f);

            _received.Should().HaveCount(2);
            AssertOpening(RemoteId, 8f);
            _nowSeconds = 1005.0;
            AssertOpening(RemoteId, 6f);
        }

        [Test]
        public void Receive_DifferentPlayersKeepIndependentCaptureTimes()
        {
            ReceiveOpening(RemoteId, 8f);
            _nowSeconds = 1003.0;
            ReceiveOpening(OtherRemoteId, 8f);

            _nowSeconds = 1004.0;
            AssertOpening(RemoteId, 4f);
            AssertOpening(OtherRemoteId, 7f);
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void Receive_InvalidState_IsNeitherCachedNorPublished(float remaining)
        {
            ReceiveOpening(RemoteId, remaining);

            OpeningState state;
            _sync.TryGetOpening(RemoteId, out state).Should().BeFalse();
            _received.Should().BeEmpty();
            _networkClient.Sent.Should().BeEmpty();
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void Receive_InvalidReplacement_DoesNotEraseOrRestartAValidCountdown(float remaining)
        {
            ReceiveOpening(RemoteId, 8f);
            _received.Clear();
            _nowSeconds = 1002.0;

            ReceiveOpening(RemoteId, remaining);

            _received.Should().BeEmpty();
            _nowSeconds = 1003.0;
            AssertOpening(RemoteId, 5f);
        }

        [TestCase(1)]
        [TestCase(99)]
        public void Receive_SelfOrUnknownPlayer_IsNeitherCachedNorPublished(int playerId)
        {
            ReceiveOpening(playerId, 8f);

            OpeningState state;
            _sync.TryGetOpening(playerId, out state).Should().BeFalse();
            _received.Should().BeEmpty();
        }

        [TestCase(0, 5)]
        [TestCase(1, 5)]
        [TestCase(2, 0)]
        [TestCase(2, 4)]
        public void Receive_WrongMembershipPair_IsNeitherCachedNorPublished(int recipient, int source)
        {
            ReceiveOpening(RemoteId, 8f, new RoomPacketScope((ulong)recipient, (ulong)source));

            OpeningState state;
            _sync.TryGetOpening(RemoteId, out state).Should().BeFalse();
            _received.Should().BeEmpty();
        }

        [Test]
        public void Receive_WithoutLocalMembership_IsNeitherCachedNorPublished()
        {
            // The remote is known, but this client has not received its own membership yet.
            _players.ReplaceRoomRoster(new[]
            {
                new RoomMemberInfo(new PlayerInfo(RemoteId, "remote", Platform.PC, true), 5)
            });

            ReceiveOpening(RemoteId, 8f, new RoomPacketScope(0, 5));

            OpeningState state;
            _sync.TryGetOpening(RemoteId, out state).Should().BeFalse();
            _received.Should().BeEmpty();
        }

        [TestCase(1, 5)]
        [TestCase(2, 4)]
        public void Receive_StaleMembership_DoesNotReplaceOrRestartExistingCountdown(int recipient, int source)
        {
            ReceiveOpening(RemoteId, 8f);
            _received.Clear();
            _nowSeconds = 1002.0;

            ReceiveOpening(RemoteId, 20f, new RoomPacketScope((ulong)recipient, (ulong)source));

            _received.Should().BeEmpty();
            _nowSeconds = 1003.0;
            AssertOpening(RemoteId, 5f);
        }

        [Test]
        public void Receive_RemoteNotInGame_IsNeitherCachedNorPublished()
        {
            ReplaceMemberships(2, 5, remoteInGame: false);

            ReceiveOpening(RemoteId, 8f);

            OpeningState state;
            _sync.TryGetOpening(RemoteId, out state).Should().BeFalse();
            _received.Should().BeEmpty();
        }

        [Test]
        public void Receive_WhileLocalPlayerIsOutsideGame_StillCachesAnInGameRemote()
        {
            _players.SetLocalPlayerInfo(_players.LocalPlayer.WithIsInGame(false));
            ReplaceMemberships(2, 5);

            ReceiveOpening(RemoteId, 8f);

            AssertOpening(RemoteId, 8f);
            _received.Should().HaveCount(1);
        }

        [Test]
        public void RoomChange_ClearsAllRemoteCachesAndReannouncesAgedLocalWithNewMembership()
        {
            _sync.Announce(State(10f));
            ReceiveOpening(RemoteId, 8f);
            ReceiveOpening(OtherRemoteId, 0f);
            _networkClient.Sent.Clear();
            _received.Clear();
            _nowSeconds = 1003.0;
            ReplaceMemberships(42, 43);

            _eventBus.Publish(new RoomMembershipChangedEvent());

            OpeningState state;
            _sync.TryGetOpening(RemoteId, out state).Should().BeFalse();
            _sync.TryGetOpening(OtherRemoteId, out state).Should().BeFalse();
            AssertSingleUpload(42, 7f);
            _received.Should().BeEmpty();

            _networkClient.Sent.Clear();
            _nowSeconds = 1005.0;
            ReplaceMemberships(52, 53);
            _eventBus.Publish(new RoomMembershipChangedEvent());
            AssertSingleUpload(52, 5f);
        }

        [Test]
        public void RoomChange_RejectsOldScopesButAcceptsTheNewMembershipPair()
        {
            ReceiveOpening(RemoteId, 8f);
            ReplaceMemberships(42, 43);
            _eventBus.Publish(new RoomMembershipChangedEvent());
            _received.Clear();

            ReceiveOpening(RemoteId, 20f, new RoomPacketScope(2, 5));
            ReceiveOpening(RemoteId, 20f, new RoomPacketScope(2, 43));
            ReceiveOpening(RemoteId, 20f, new RoomPacketScope(42, 5));

            OpeningState state;
            _sync.TryGetOpening(RemoteId, out state).Should().BeFalse();
            _received.Should().BeEmpty();

            ReceiveOpening(RemoteId, 6f, new RoomPacketScope(42, 43));
            AssertOpening(RemoteId, 6f);
            _received.Should().HaveCount(1);
        }

        [Test]
        public void RoomMetadataAndRosterNotifications_DoNotReplayLocalOrClearRemoteOpening()
        {
            _sync.Announce(State(10f));
            ReceiveOpening(RemoteId, 8f);
            _networkClient.Sent.Clear();
            _received.Clear();
            _nowSeconds = 1002.0;

            _eventBus.Publish(new CurrentRoomChangedEvent());
            _eventBus.Publish(new PlayerRosterReceivedEvent());
            _eventBus.Publish(new PlayerListUpdatedEvent());

            _networkClient.Sent.Should().BeEmpty();
            _received.Should().BeEmpty();
            AssertOpening(RemoteId, 6f);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void PlayerLeft_ClearsOnlyThatRemoteIncludingKnownZero(bool completed)
        {
            _sync.Announce(State(10f));
            ReceiveOpening(RemoteId, completed ? 0f : 8f);
            ReceiveOpening(OtherRemoteId, 9f);
            _received.Clear();
            _networkClient.Sent.Clear();
            var remainingMembers = _players.Players.Where(p => p.Id != RemoteId)
                .Select(p => new RoomMemberInfo(p, (ulong)p.Id + 1)).ToArray();
            _players.ReplaceRoomRoster(remainingMembers);
            _nowSeconds = 1002.0;

            _eventBus.Publish(new PlayerLeftEvent(RemoteId, "remote", Platform.PC));

            OpeningState state;
            _sync.TryGetOpening(RemoteId, out state).Should().BeFalse();
            AssertOpening(OtherRemoteId, 7f);
            ReceiveOpening(RemoteId, 20f);
            _sync.TryGetOpening(RemoteId, out state).Should().BeFalse();
            _received.Should().BeEmpty();
            _networkClient.Sent.Should().BeEmpty();

            _eventBus.Publish(new RoomMembershipChangedEvent());
            AssertSingleUpload(2, 8f);
        }

        [Test]
        public void PlayerRejoinsWithSameId_RejectsPreviousMembershipAndStartsANewCountdown()
        {
            ReceiveOpening(RemoteId, 8f);
            _eventBus.Publish(new PlayerLeftEvent(RemoteId, "remote", Platform.PC));
            ReplaceMemberships(2, 43);
            _received.Clear();
            _nowSeconds = 1003.0;

            ReceiveOpening(RemoteId, 20f, new RoomPacketScope(2, 5));
            OpeningState state;
            _sync.TryGetOpening(RemoteId, out state).Should().BeFalse();
            _received.Should().BeEmpty();

            ReceiveOpening(RemoteId, 6f, new RoomPacketScope(2, 43));
            _received.Should().HaveCount(1);
            _nowSeconds = 1005.0;
            AssertOpening(RemoteId, 4f);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void PlayerQuitGame_ClearsOnlyThatRemoteAndDoesNotAcceptLateOpening(bool completed)
        {
            _sync.Announce(State(10f));
            ReceiveOpening(RemoteId, completed ? 0f : 8f);
            ReceiveOpening(OtherRemoteId, 9f);
            _received.Clear();
            _networkClient.Sent.Clear();
            ReplaceMemberships(2, 5, remoteInGame: false);
            _nowSeconds = 1002.0;

            _eventBus.Publish(new PlayerQuitGameEvent(
                new PlayerInfo(RemoteId, "remote", Platform.PC, false)));

            OpeningState state;
            _sync.TryGetOpening(RemoteId, out state).Should().BeFalse();
            AssertOpening(OtherRemoteId, 7f);
            ReceiveOpening(RemoteId, 20f);
            _sync.TryGetOpening(RemoteId, out state).Should().BeFalse();
            _received.Should().BeEmpty();
            _networkClient.Sent.Should().BeEmpty();

            _eventBus.Publish(new RoomMembershipChangedEvent());
            AssertSingleUpload(2, 8f);
        }

        [Test]
        public void RemoteReentersGame_InTheSameMembership_CanAnnounceAgain()
        {
            ReceiveOpening(RemoteId, 8f);
            ReplaceMemberships(2, 5, remoteInGame: false);
            _eventBus.Publish(new PlayerQuitGameEvent(
                new PlayerInfo(RemoteId, "remote", Platform.PC, false)));
            ReplaceMemberships(2, 5);
            _received.Clear();
            _nowSeconds = 1003.0;

            ReceiveOpening(RemoteId, 6f);

            _received.Should().HaveCount(1);
            _nowSeconds = 1005.0;
            AssertOpening(RemoteId, 4f);
        }

        [Test]
        public void Disconnect_ClearsRemoteCachesButPreservesLocalElapsedTimeAcrossReconnects()
        {
            _sync.Announce(State(20f));
            ReceiveOpening(RemoteId, 8f);
            ReceiveOpening(OtherRemoteId, 0f);
            _networkClient.Sent.Clear();
            _received.Clear();
            _nowSeconds = 1002.0;
            Disconnect();

            OpeningState state;
            _sync.TryGetOpening(RemoteId, out state).Should().BeFalse();
            _sync.TryGetOpening(OtherRemoteId, out state).Should().BeFalse();
            _networkClient.Sent.Should().BeEmpty();
            _received.Should().BeEmpty();

            _nowSeconds = 1007.0;
            _networkClient.IsConnected = true;
            // Reconnecting can assign a new player ID as well as a new membership.
            _players.SetLocalPlayerInfo(new PlayerInfo(12, "me", Platform.PC, true));
            ReplaceMemberships(42, 43);
            _eventBus.Publish(new RoomMembershipChangedEvent());
            AssertSingleUpload(42, 13f);

            _networkClient.Sent.Clear();
            _nowSeconds = 1008.0;
            Disconnect();
            _nowSeconds = 1010.0;
            _networkClient.IsConnected = true;
            ReplaceMemberships(52, 53);
            _eventBus.Publish(new RoomMembershipChangedEvent());
            AssertSingleUpload(52, 10f);
        }

        [Test]
        public void Disconnect_LocalCountdownCanCompleteOfflineAndReplayKnownZero()
        {
            _sync.Announce(State(4f));
            _networkClient.Sent.Clear();
            _nowSeconds = 1001.0;
            Disconnect();

            _nowSeconds = 1010.0;
            _networkClient.IsConnected = true;
            ReplaceMemberships(42, 43);
            _eventBus.Publish(new RoomMembershipChangedEvent());

            AssertSingleUpload(42, 0f);
        }

        [TestCase(0f)]
        [TestCase(10f)]
        public void ClearLocal_ForgetsTheSceneWithoutSendingZeroOrErasingRemoteCaches(float remaining)
        {
            _sync.Announce(State(remaining));
            ReceiveOpening(RemoteId, 8f);
            _networkClient.Sent.Clear();
            _received.Clear();
            _nowSeconds = 1002.0;

            _sync.ClearLocal();
            _sync.ClearLocal();

            _networkClient.Sent.Should().BeEmpty();
            _received.Should().BeEmpty();
            AssertOpening(RemoteId, 6f);

            ReplaceMemberships(42, 43);
            _eventBus.Publish(new RoomMembershipChangedEvent());
            _networkClient.Sent.Should().BeEmpty();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ClearLocal_PreventsOldSceneReplayOnReconnect(bool clearAfterDisconnect)
        {
            _sync.Announce(State(20f));
            _networkClient.Sent.Clear();
            if (!clearAfterDisconnect) _sync.ClearLocal();
            Disconnect();
            if (clearAfterDisconnect) _sync.ClearLocal();

            _nowSeconds = 1005.0;
            _networkClient.IsConnected = true;
            ReplaceMemberships(42, 43);
            _eventBus.Publish(new RoomMembershipChangedEvent());

            _networkClient.Sent.Should().BeEmpty();
        }

        [Test]
        public void ClearLocal_AllowsANewSceneSnapshotWithItsOwnCaptureTime()
        {
            _networkClient.IsConnected = false;
            _sync.Announce(State(20f));
            _sync.ClearLocal();
            _nowSeconds = 1005.0;
            _sync.Announce(State(8f));

            _nowSeconds = 1007.0;
            _networkClient.IsConnected = true;
            ReplaceMemberships(42, 43);
            _eventBus.Publish(new RoomMembershipChangedEvent());

            AssertSingleUpload(42, 6f);
        }

        private static OpeningState State(float remaining)
        {
            return new OpeningState { RemainingSeconds = remaining };
        }

        private void ReceiveOpening(int playerId, float remaining, RoomPacketScope? scope = null)
        {
            _dispatcher.Receive(new S2COpeningStatePacket
            {
                Scope = scope ?? TestClientRoster.Scope(playerId),
                PlayerId = playerId,
                State = State(remaining)
            });
        }

        private void AssertOpening(int playerId, float remaining)
        {
            OpeningState state;
            _sync.TryGetOpening(playerId, out state).Should().BeTrue();
            state.RemainingSeconds.Should().Be(remaining);
        }

        private void AssertSingleUpload(ulong membershipId, float remaining)
        {
            _networkClient.Sent.Should().HaveCount(1);
            var sent = _networkClient.Sent.Single();
            sent.Packet.Should().BeOfType<C2SOpeningStatePacket>();
            var packet = (C2SOpeningStatePacket)sent.Packet;
            packet.MembershipId.Should().Be(membershipId);
            packet.State.RemainingSeconds.Should().Be(remaining);
            sent.Channel.Should().Be(NetworkChannels.Default);
            sent.Method.Should().Be(DeliveryMethod.ReliableOrdered);
        }

        private void ReplaceMemberships(ulong localMembership, ulong remoteMembership, bool remoteInGame = true)
        {
            _players.ReplaceRoomRoster(new[]
            {
                new RoomMemberInfo(_players.LocalPlayer, localMembership),
                new RoomMemberInfo(new PlayerInfo(RemoteId, "remote", Platform.PC, remoteInGame), remoteMembership),
                new RoomMemberInfo(new PlayerInfo(OtherRemoteId, "other", Platform.PC, true), 10)
            });
        }

        private void Disconnect()
        {
            _networkClient.IsConnected = false;
            // TestClientRoster uses its own bus; mirror PlayerService's roster clearing before
            // delivering the lifecycle event to this module, rather than leaving stale membership.
            _players.ReplaceRoomRoster(new RoomMemberInfo[0]);
            _eventBus.Publish(new ServerDisconnectedEvent("closed"));
        }
    }

    // Separate fixture: these tests must not inherit the direct-construction/manual-Start setup.
    [TestFixture, NonParallelizable]
    public class ClientOpeningSyncCompositionTests
    {
        private FakeNetworkClient _networkClient;
        private RecordingClientDispatcher _dispatcher;
        private double _nowSeconds;

        [SetUp]
        public void Setup()
        {
            _networkClient = new FakeNetworkClient { IsConnected = false };
            _dispatcher = new RecordingClientDispatcher();
            _nowSeconds = 1000.0;
        }

        [Test]
        public void WithClient_BuildActivatesOpeningHandlerBeforeExplicitResolution()
        {
            using (var container = BuildClientContainer())
            {
                var events = container.Resolve<IClientEventBus>();
                var players = container.Resolve<IPlayerService>();
                var rooms = container.Resolve<IRoomService>();
                var received = new List<PlayerOpeningReceivedEvent>();
                events.Subscribe<PlayerOpeningReceivedEvent>(e => received.Add(e));

                players.SetLocalPlayerInfo(new PlayerInfo(0, "me", Platform.PC, true));
                _networkClient.IsConnected = true;
                events.Publish(new ServerHandshakeEvent(11));
                _dispatcher.Receive(RoomSnapshot(11, 101, 202));

                players.LocalMembershipId.Should().Be(101UL);
                rooms.CurrentRoom.Id.Should().Be(7);

                // No Resolve<ClientOpeningSync> or manual Start before this packet:
                // removing its IStartable registration must leave the handler absent.
                ReceiveOpening(new RoomPacketScope(101, 202), 8f);

                received.Should().HaveCount(1);
                received.Single().PlayerId.Should().Be(4);
                received.Single().State.RemainingSeconds.Should().Be(8f);

                var sync = container.Resolve<ClientOpeningSync>();
                _nowSeconds = 1002.0;
                OpeningState state;
                sync.TryGetOpening(4, out state).Should().BeTrue();
                state.RemainingSeconds.Should().Be(6f);
                container.Resolve<ClientOpeningSync>().Should().BeSameAs(sync);
                _networkClient.Sent.Select(s => s.Packet).OfType<C2SOpeningStatePacket>()
                    .Should().BeEmpty();
            }
        }

        [Test]
        public void WithClient_OfflineSnapshotAgesThroughActualHandshakeRosterAndReconnect()
        {
            using (var container = BuildClientContainer())
            {
                var events = container.Resolve<IClientEventBus>();
                var players = container.Resolve<IPlayerService>();
                var rooms = container.Resolve<IRoomService>();
                var sync = container.Resolve<ClientOpeningSync>();
                var received = new List<PlayerOpeningReceivedEvent>();
                events.Subscribe<PlayerOpeningReceivedEvent>(e => received.Add(e));

                sync.Announce(new OpeningState { RemainingSeconds = 20f });
                _networkClient.Sent.Should().BeEmpty();
                players.LocalMembershipId.Should().Be(0UL);

                _nowSeconds = 1002.0;
                // MultiplayerClient.Connect restores this scene fact before the handshake.
                players.SetLocalPlayerInfo(new PlayerInfo(0, "me", Platform.PC, true));
                _networkClient.IsConnected = true;
                events.Publish(new ServerHandshakeEvent(11));

                players.LocalPlayer.Id.Should().Be(11);
                players.LocalMembershipId.Should().Be(0UL);
                rooms.CurrentRoom.Should().BeNull();
                AssertOnlyHandshakeSent();
                _networkClient.Sent.Clear();

                _nowSeconds = 1003.0;
                _dispatcher.Receive(RoomSnapshot(11, 101, 202));

                players.LocalMembershipId.Should().Be(101UL);
                players.Players.Select(p => p.Id).Should().BeEquivalentTo(new[] { 11, 4 });
                rooms.CurrentRoom.Id.Should().Be(7);
                // RoomService installed the actual roster and published readiness itself.
                AssertSingleUpload(101, 17f);

                ReceiveOpening(new RoomPacketScope(101, 202), 8f);
                OpeningState state;
                sync.TryGetOpening(4, out state).Should().BeTrue();
                state.RemainingSeconds.Should().Be(8f);
                received.Should().HaveCount(1);
                received.Clear();
                _networkClient.Sent.Clear();

                _nowSeconds = 1005.0;
                _networkClient.IsConnected = false;
                events.Publish(new ServerDisconnectedEvent("closed"));

                // The real PlayerService, unlike TestClientRoster's isolated bus, resets these.
                players.LocalPlayer.Id.Should().Be(0);
                players.LocalPlayer.IsInGame.Should().BeFalse();
                players.LocalMembershipId.Should().Be(0UL);
                players.Players.Should().BeEmpty();
                rooms.CurrentRoom.Should().BeNull();
                sync.TryGetOpening(4, out state).Should().BeFalse();
                _networkClient.Sent.Should().BeEmpty();

                _nowSeconds = 1007.0;
                players.SetLocalPlayerInfo(new PlayerInfo(0, "me", Platform.PC, true));
                _networkClient.IsConnected = true;
                events.Publish(new ServerHandshakeEvent(27));

                players.LocalPlayer.Id.Should().Be(27);
                players.LocalPlayer.IsInGame.Should().BeTrue();
                players.LocalMembershipId.Should().Be(0UL);
                AssertOnlyHandshakeSent();
                _networkClient.Sent.Clear();
                ReceiveOpening(new RoomPacketScope(101, 202), 20f);
                sync.TryGetOpening(4, out state).Should().BeFalse();
                received.Should().BeEmpty();

                _nowSeconds = 1009.0;
                _dispatcher.Receive(RoomSnapshot(27, 303, 404));

                players.LocalMembershipId.Should().Be(303UL);
                players.Players.Select(p => p.Id).Should().BeEquivalentTo(new[] { 27, 4 });
                rooms.CurrentRoom.Id.Should().Be(7);
                // No second Announce: all nine seconds, including disconnected time, count.
                AssertSingleUpload(303, 11f);
                _networkClient.Sent.Clear();

                ReceiveOpening(new RoomPacketScope(101, 202), 20f);
                ReceiveOpening(new RoomPacketScope(101, 404), 20f);
                ReceiveOpening(new RoomPacketScope(303, 202), 20f);
                sync.TryGetOpening(4, out state).Should().BeFalse();
                received.Should().BeEmpty();

                ReceiveOpening(new RoomPacketScope(303, 404), 6f);
                received.Should().HaveCount(1);
                received.Single().PlayerId.Should().Be(4);
                received.Single().State.RemainingSeconds.Should().Be(6f);
                _nowSeconds = 1010.0;
                sync.TryGetOpening(4, out state).Should().BeTrue();
                state.RemainingSeconds.Should().Be(5f);
                _networkClient.Sent.Should().BeEmpty();
            }
        }

        private IContainer BuildClientContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterMultiplayerCore().WithClient();
            builder.RegisterInstance(_networkClient).As<INetworkClient>();
            builder.RegisterInstance(_dispatcher).As<IClientPacketDispatcher>();
            // Register only in this test container; Autofac must select the clock overload.
            OpeningClock clock = () => _nowSeconds;
            builder.RegisterInstance(clock).As<OpeningClock>();
            return builder.Build();
        }

        private static S2CPlayerListPacket RoomSnapshot(int localPlayerId, ulong localMembership, ulong remoteMembership)
        {
            return new S2CPlayerListPacket
            {
                Room = new RoomInfo(7, "opening room", false, 8, 2, localPlayerId),
                Members = new List<RoomMemberInfo>
                {
                    new RoomMemberInfo(new PlayerInfo(localPlayerId, "me", Platform.PC, true), localMembership),
                    new RoomMemberInfo(new PlayerInfo(4, "remote", Platform.PC, true), remoteMembership)
                }
            };
        }

        private void ReceiveOpening(RoomPacketScope scope, float remaining)
        {
            _dispatcher.Receive(new S2COpeningStatePacket
            {
                Scope = scope,
                PlayerId = 4,
                State = new OpeningState { RemainingSeconds = remaining }
            });
        }

        private void AssertOnlyHandshakeSent()
        {
            _networkClient.Sent.Should().HaveCount(1);
            var sent = _networkClient.Sent.Single();
            sent.Packet.Should().BeOfType<C2SClientHandShakePacket>();
            var handshake = (C2SClientHandShakePacket)sent.Packet;
            handshake.PlayerName.Should().Be("me");
            handshake.Platform.Should().Be(Platform.PC);
            handshake.IsInGame.Should().BeTrue();
            sent.Channel.Should().Be(NetworkChannels.Default);
            sent.Method.Should().Be(DeliveryMethod.ReliableOrdered);
        }

        private void AssertSingleUpload(ulong membershipId, float remaining)
        {
            _networkClient.Sent.Should().HaveCount(1);
            var sent = _networkClient.Sent.Single();
            sent.Packet.Should().BeOfType<C2SOpeningStatePacket>();
            var packet = (C2SOpeningStatePacket)sent.Packet;
            packet.MembershipId.Should().Be(membershipId);
            packet.State.RemainingSeconds.Should().Be(remaining);
            sent.Channel.Should().Be(NetworkChannels.Default);
            sent.Method.Should().Be(DeliveryMethod.ReliableOrdered);
        }
    }
}
