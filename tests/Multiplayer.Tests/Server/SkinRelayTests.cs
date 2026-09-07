using System.Collections.Generic;
using System.Linq;
using Autofac;
using FluentAssertions;
using GOILauncher.Multiplayer.Core.Data.Constants;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Server.Events;
using GOILauncher.Multiplayer.Server.Services;
using GOILauncher.Multiplayer.Server.Synchronization;
using LiteNetLib;
using Moq;
using NUnit.Framework;

namespace GOILauncher.Multiplayer.Tests.Server
{
    [TestFixture]
    public class SkinRelayTests
    {
        private const int SenderId = 1;
        private const int OtherId = 2;

        private FakeNetworkServer _networkServer;
        private RecordingServerDispatcher _dispatcher;
        private StubPlayerService _playerService;
        private ServerEventBus _eventBus;

        [SetUp]
        public void Setup()
        {
            _networkServer = new FakeNetworkServer();
            _dispatcher = new RecordingServerDispatcher();
            _playerService = new StubPlayerService();
            _eventBus = new ServerEventBus(new Mock<ILogger<EventBus>>().Object);

            var relay = new SkinRelay(_networkServer, _dispatcher, _playerService, new LobbyRoomStub(_playerService), _eventBus,
                new Mock<ILogger<SkinRelay>>().Object);
            ((IStartable)relay).Start();
        }

        [Test]
        public void UnknownSender_IsDropped()
        {
            AddPlayer(OtherId);

            Announce(SenderId, SkinTestData.State(SkinTestData.Png(0x01)));

            _networkServer.Sent.Should().BeEmpty();
        }

        /// <summary>
        /// Slots exist so other parts can be added later. A slot this build does not know about
        /// cannot be rendered by anyone here, so relaying it would only spend bandwidth.
        /// </summary>
        [Test]
        public void UnknownSlot_IsDropped()
        {
            AddPlayer(SenderId);
            AddPlayer(OtherId);

            var state = SkinTestData.State(SkinTestData.Png(0x01));
            state.Slot = SkinConstants.PotSlot + 1;

            Announce(SenderId, state);

            _networkServer.Sent.Should().BeEmpty();
        }

        [Test]
        public void ReadyManifest_IsRelayedToRoomMembersOnTheControlChannel()
        {
            AddPlayer(SenderId);
            AddPlayer(OtherId);
            AddPlayer(3);

            Announce(SenderId, SkinTestData.State(SkinTestData.Png(0x01), 1f));
            Upload(SenderId, SkinTestData.Png(0x01));

            _networkServer.Sent.Select(s => s.ClientId).Should().BeEquivalentTo(new[] { OtherId, 3 });
            _networkServer.Sent.Should().OnlyContain(s =>
                s.Channel == NetworkChannels.Default && s.Method == DeliveryMethod.ReliableOrdered);
        }

        /// <summary>
        /// Members outside the gameplay scene are included on purpose: getting the manifest early means the texture is
        /// already downloaded by the time they enter the level.
        /// </summary>
        [Test]
        public void Manifest_PlayerIdComesFromSender_NotFromTheBody()
        {
            AddPlayer(SenderId);
            AddPlayer(OtherId);

            Announce(SenderId, SkinTestData.State(SkinTestData.Png(0x01), 1f));
            Upload(SenderId, SkinTestData.Png(0x01));

            var relayed = (S2CSkinManifestPacket)_networkServer.Sent.Single().Packet;
            relayed.PlayerId.Should().Be(SenderId);
            relayed.State.Goldness.Should().Be(1f);
        }

        [Test]
        public void Upload_WithoutAManifest_IsDropped()
        {
            AddPlayer(SenderId);
            AddPlayer(OtherId);
            var payload = SkinTestData.Png(0x01);

            Upload(SenderId, payload);
            Request(OtherId, SenderId, SkinHash.Compute(payload));

            LastPacketFor(OtherId).Should().BeOfType<S2CSkinUnavailablePacket>();
        }

        [Test]
        public void Upload_OfSomethingOtherThanTheAnnouncedHash_IsDropped()
        {
            AddPlayer(SenderId);
            AddPlayer(OtherId);
            var announced = SkinTestData.Png(0x01);

            Announce(SenderId, SkinTestData.State(announced));
            Upload(SenderId, SkinTestData.Png(0x02));
            Request(OtherId, SenderId, SkinHash.Compute(announced));

            LastPacketFor(OtherId).Should().BeOfType<S2CSkinUnavailablePacket>();
        }

        /// <summary>
        /// The server forwards these bytes to everyone, so it cannot take the uploader's word for
        /// what they are — the same validation the client runs has to run here too.
        /// </summary>
        [Test]
        public void Upload_ThatFailsValidation_IsDropped()
        {
            AddPlayer(SenderId);
            AddPlayer(OtherId);
            var bomb = SkinTestData.Png(SkinConstants.MaxTextureSize + 1, 64, 0x01);

            Announce(SenderId, SkinTestData.State(bomb));
            Upload(SenderId, bomb);
            Request(OtherId, SenderId, SkinHash.Compute(bomb));

            LastPacketFor(OtherId).Should().BeOfType<S2CSkinUnavailablePacket>();
        }

        [Test]
        public void Request_ForACachedSkin_IsServedFromTheCache()
        {
            AddPlayer(SenderId);
            AddPlayer(OtherId);
            var payload = SkinTestData.Png(0x01);

            Announce(SenderId, SkinTestData.State(payload));
            Upload(SenderId, payload);
            Request(OtherId, SenderId, SkinHash.Compute(payload));

            var served = LastPacketFor(OtherId).Should().BeOfType<S2CSkinDataPacket>().Subject;
            served.PlayerId.Should().Be(SenderId);
            served.Blob.Data.Should().Equal(payload);
            _networkServer.Sent.Last().Channel.Should().Be(NetworkChannels.Skin);
        }

        /// <summary>
        /// A request can be in flight while its target changes skins. Answering it with whatever is
        /// cached now would paint the requester with a texture nobody announced.
        /// </summary>
        [Test]
        public void Request_ForAHashTheServerDoesNotHold_IsAnsweredWithUnavailable()
        {
            AddPlayer(SenderId);
            AddPlayer(OtherId);
            var payload = SkinTestData.Png(0x01);

            Announce(SenderId, SkinTestData.State(payload));
            Upload(SenderId, payload);
            Request(OtherId, SenderId, SkinHash.Compute(SkinTestData.Png(0x02)));

            LastPacketFor(OtherId).Should().BeOfType<S2CSkinUnavailablePacket>();
        }

        [Test]
        public void ChangingSkin_DropsTheOldBytes()
        {
            AddPlayer(SenderId);
            AddPlayer(OtherId);
            var first = SkinTestData.Png(0x01);

            Announce(SenderId, SkinTestData.State(first));
            Upload(SenderId, first);
            Announce(SenderId, SkinTestData.State(SkinTestData.Png(0x02)));
            Request(OtherId, SenderId, SkinHash.Compute(first));

            LastPacketFor(OtherId).Should().BeOfType<S2CSkinUnavailablePacket>();
        }

        /// <summary>
        /// Re-announcing the same skin must not throw away the cached bytes: a client that already
        /// uploaded this hash will not upload it again.
        /// </summary>
        [Test]
        public void ReAnnouncingTheSameSkin_KeepsTheCachedBytes()
        {
            AddPlayer(SenderId);
            AddPlayer(OtherId);
            var payload = SkinTestData.Png(0x01);

            Announce(SenderId, SkinTestData.State(payload));
            Upload(SenderId, payload);
            Announce(SenderId, SkinTestData.State(payload, 1f));
            Request(OtherId, SenderId, SkinHash.Compute(payload));

            LastPacketFor(OtherId).Should().BeOfType<S2CSkinDataPacket>();
        }

        /// <summary>
        /// A late joiner only receives the player roster, which carries no skins. Without this
        /// catch-up every pot in the room stays vanilla until its owner restarts the level.
        /// </summary>
        [Test]
        public void RoomEntry_PushesEveryKnownManifestExceptTheNewcomersOwn()
        {
            AddPlayer(SenderId);
            AddPlayer(OtherId);
            Announce(SenderId, SkinTestData.State(SkinTestData.Png(0x01)));
            Upload(SenderId, SkinTestData.Png(0x01));
            Announce(OtherId, SkinTestData.State(SkinTestData.Png(0x02)));
            Upload(OtherId, SkinTestData.Png(0x02));
            _networkServer.Sent.Clear();

            AddPlayer(3);
            _eventBus.Publish(new PlayerRoomEnteredEvent(3));

            var pushed = _networkServer.Sent
                .Where(s => s.ClientId == 3)
                .Select(s => ((S2CSkinManifestPacket)s.Packet).PlayerId)
                .ToList();
            pushed.Should().BeEquivalentTo(new[] { SenderId, OtherId });
            _networkServer.Sent.Should().OnlyContain(s => s.ClientId == 3);
        }

        [Test]
        public void RoomEntry_DoesNotEchoTheNewcomersOwnManifestBack()
        {
            AddPlayer(SenderId);
            Announce(SenderId, SkinTestData.State(SkinTestData.Png(0x01)));
            _networkServer.Sent.Clear();

            _eventBus.Publish(new PlayerRoomEnteredEvent(SenderId));

            _networkServer.Sent.Should().BeEmpty();
        }

        [Test]
        public void Disconnect_ForgetsTheManifestAndTheBytes()
        {
            AddPlayer(SenderId);
            AddPlayer(OtherId);
            var payload = SkinTestData.Png(0x01);
            Announce(SenderId, SkinTestData.State(payload));
            Upload(SenderId, payload);

            _playerService.Players.Remove(SenderId);
            _eventBus.Publish(new ClientDisconnectedEvent(SenderId, "left"));
            _networkServer.Sent.Clear();
            Request(OtherId, SenderId, SkinHash.Compute(payload));
            _networkServer.Sent.Should().BeEmpty(); // no longer a room member: no information is disclosed
            AddPlayer(SenderId);
            Request(OtherId, SenderId, SkinHash.Compute(payload));
            LastPacketFor(OtherId).Should().BeOfType<S2CSkinUnavailablePacket>();

            _networkServer.Sent.Clear();
            AddPlayer(3);
            _eventBus.Publish(new PlayerRoomEnteredEvent(3));
            _networkServer.Sent.Should().BeEmpty();
        }

        private void Announce(int senderId, SkinState state)
        {
            _dispatcher.Receive(new C2SSkinManifestPacket { State = state }, senderId);
        }

        private void Upload(int senderId, byte[] payload)
        {
            _dispatcher.Receive(new C2SSkinDataPacket { Blob = SkinTestData.Blob(payload) }, senderId);
        }

        private void Request(int senderId, int ownerId, SkinHash hash)
        {
            _dispatcher.Receive(new C2SSkinRequestPacket { Scope = new RoomPacketScope((ulong)senderId + 1, (ulong)ownerId + 1), PlayerId = ownerId, Hash = hash }, senderId);
        }

        private object LastPacketFor(int clientId)
        {
            return _networkServer.Sent.Last(s => s.ClientId == clientId).Packet;
        }

        private void AddPlayer(int id)
        {
            _playerService.Players[id] = new PlayerInfo(id, "p" + id, Platform.PC, true);
        }

        private sealed class StubPlayerService : IPlayerService
        {
            public Dictionary<int, PlayerInfo> Players { get; private set; }

            public StubPlayerService()
            {
                Players = new Dictionary<int, PlayerInfo>();
            }
        }
    }
}
