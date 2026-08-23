using System.Collections.Generic;
using System.Linq;
using Autofac;
using FluentAssertions;
using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Client.Services;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using Moq;
using NUnit.Framework;

namespace GOILauncher.Multiplayer.Tests.Client
{
    /// <summary>
    /// PlayerService is the single authoritative roster on the client. These tests pin
    /// the two things every other module now relies on: what the roster contains, and
    /// that each event is published only after the roster already reflects it.
    /// </summary>
    [TestFixture]
    public class ClientPlayerServiceTests
    {
        private RecordingClientDispatcher _dispatcher;
        private FakeNetworkClient _networkClient;
        private EventBus _eventBus;
        private PlayerService _playerService;

        [SetUp]
        public void Setup()
        {
            _dispatcher = new RecordingClientDispatcher();
            _networkClient = new FakeNetworkClient();
            _eventBus = new EventBus(new Mock<ILogger<EventBus>>().Object);
            _playerService = new PlayerService(
                _networkClient,
                _dispatcher,
                _eventBus,
                new Mock<ILogger<PlayerService>>().Object);
            ((IStartable)_playerService).Start();
            _playerService.SetLocalPlayerInfo(new PlayerInfo(0, "me", Platform.PC, false));
        }

        [Test]
        public void Handshake_PublishesLocalPlayerReadyAfterTheLocalIdIsWritten()
        {
            // PlayerManager reads IPlayerService inside this handler, so the ordering is
            // a contract, not an implementation detail of the subscription order.
            var idSeenByHandler = -1;
            _eventBus.Subscribe<LocalPlayerReadyEvent>(e => idSeenByHandler = _playerService.LocalPlayer.Id);

            _eventBus.Publish(new ServerHandshakeEvent(7));

            idSeenByHandler.Should().Be(7);
            _playerService.Players.Select(p => p.Id).Should().BeEquivalentTo(new[] { 7 });
        }

        [Test]
        public void PlayerList_KeepsLocalPlayerAndPublishesRosterReceived()
        {
            _eventBus.Publish(new ServerHandshakeEvent(7));
            var rosterEvents = 0;
            _eventBus.Subscribe<PlayerRosterReceivedEvent>(e => rosterEvents++);

            _dispatcher.Receive(new S2CPlayerListPacket
            {
                Players = new List<PlayerInfo>
                {
                    new PlayerInfo(1, "alice", Platform.PC, true),
                    new PlayerInfo(2, "bob", Platform.PC, false)
                }
            });

            rosterEvents.Should().Be(1);
            _playerService.Players.Select(p => p.Id).Should().BeEquivalentTo(new[] { 7, 1, 2 });
        }

        [Test]
        public void PlayerJoined_IsAlreadyInTheRosterWhenTheEventIsPublished()
        {
            _eventBus.Publish(new ServerHandshakeEvent(7));
            PlayerInfo seenByHandler = null;
            _eventBus.Subscribe<PlayerJoinedEvent>(e =>
            {
                PlayerInfo found;
                _playerService.TryGetPlayer(e.PlayerId, out found);
                seenByHandler = found;
            });

            _dispatcher.Receive(new S2CPlayerJoinedPacket
            {
                PlayerId = 3,
                PlayerName = "carol",
                Platform = Platform.PC,
                IsInGame = true
            });

            seenByHandler.Should().NotBeNull();
            seenByHandler.Name.Should().Be("carol");
            seenByHandler.IsInGame.Should().BeTrue();
        }

        [Test]
        public void Disconnect_ClearsRosterAndReleasesTheServerAssignedId()
        {
            _eventBus.Publish(new ServerHandshakeEvent(7));

            _eventBus.Publish(new ServerDisconnectedEvent("closed"));

            _playerService.Players.Should().BeEmpty();
            // The id belongs to the connection; the name is reused on the next connect.
            _playerService.LocalPlayer.Id.Should().Be(0);
            _playerService.LocalPlayer.Name.Should().Be("me");
        }

        [Test]
        public void Handshake_StartsFromAnEmptyRoster()
        {
            _eventBus.Publish(new ServerHandshakeEvent(7));
            _dispatcher.Receive(new S2CPlayerListPacket
            {
                Players = new List<PlayerInfo> { new PlayerInfo(1, "alice", Platform.PC, true) }
            });

            _eventBus.Publish(new ServerHandshakeEvent(9));

            _playerService.Players.Select(p => p.Id).Should().BeEquivalentTo(new[] { 9 });
        }

        [Test]
        public void IsInGameUpdate_PublishesTransitionEventsOnlyWhenTheValueChanges()
        {
            _eventBus.Publish(new ServerHandshakeEvent(7));
            _dispatcher.Receive(new S2CPlayerListPacket
            {
                Players = new List<PlayerInfo> { new PlayerInfo(1, "alice", Platform.PC, false) }
            });

            var entered = 0;
            var quit = 0;
            _eventBus.Subscribe<PlayerEnteredGameEvent>(e => entered++);
            _eventBus.Subscribe<PlayerQuitGameEvent>(e => quit++);

            _dispatcher.Receive(new S2CIsInGameUpdatePacket { PlayerId = 1, IsInGame = true });
            _dispatcher.Receive(new S2CIsInGameUpdatePacket { PlayerId = 1, IsInGame = true });
            _dispatcher.Receive(new S2CIsInGameUpdatePacket { PlayerId = 1, IsInGame = false });

            entered.Should().Be(1);
            quit.Should().Be(1);
        }

        [Test]
        public void PlayerLeft_RemovesFromRosterBeforeTheEventIsPublished()
        {
            _eventBus.Publish(new ServerHandshakeEvent(7));
            _dispatcher.Receive(new S2CPlayerListPacket
            {
                Players = new List<PlayerInfo> { new PlayerInfo(1, "alice", Platform.PC, true) }
            });
            var stillPresent = true;
            _eventBus.Subscribe<PlayerLeftEvent>(e =>
            {
                PlayerInfo found;
                stillPresent = _playerService.TryGetPlayer(e.PlayerId, out found);
            });

            _dispatcher.Receive(new S2CPlayerLeftPacket { PlayerId = 1 });

            stillPresent.Should().BeFalse();
            _playerService.Players.Select(p => p.Id).Should().BeEquivalentTo(new[] { 7 });
        }
    }
}
