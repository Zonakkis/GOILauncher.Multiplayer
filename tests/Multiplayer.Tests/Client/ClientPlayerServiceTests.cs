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
    [TestFixture]
    public class ClientPlayerServiceTests
    {
        private RecordingClientDispatcher _dispatcher;
        private FakeNetworkClient _networkClient;
        private ClientEventBus _eventBus;
        private PlayerService _playerService;
        private ulong _membership;
        [SetUp]
        public void Setup()
        {
            _dispatcher = new RecordingClientDispatcher(); _networkClient = new FakeNetworkClient();
            _eventBus = new ClientEventBus(new Mock<ILogger<EventBus>>().Object);
            _playerService = new PlayerService(_networkClient, _dispatcher, _eventBus, new Mock<ILogger<PlayerService>>().Object);
            ((IStartable)_playerService).Start();
            ((IStartable)new RoomService(_networkClient, _dispatcher, _eventBus, _playerService)).Start();
            _playerService.SetLocalPlayerInfo(new PlayerInfo(0, "me", Platform.PC, false));
            _membership = 100;
        }
        private void Enter(params PlayerInfo[] remote)
        {
            _dispatcher.Receive(new S2CPlayerListPacket
            {
                Room = new RoomInfo(0, "大厅", false, 0, remote.Length + 1, null),
                Members = remote.Select(p => new RoomMemberInfo(p, (ulong)p.Id + 1))
                    .Concat(new[] { new RoomMemberInfo(_playerService.LocalPlayer, ++_membership) }).ToList()
            });
        }
        private RoomPacketScope Scope(int playerId) => new RoomPacketScope(_membership, (ulong)playerId + 1);
        [Test]
        public void Handshake_PublishesLocalPlayerReadyAfterTheLocalIdIsWritten()
        {
            int seen = -1;
            _eventBus.Subscribe<LocalPlayerReadyEvent>(e => seen = _playerService.LocalPlayer.Id);
            _eventBus.Publish(new ServerHandshakeEvent(7));
            seen.Should().Be(7);
            _playerService.Players.Select(p => p.Id).Should().Equal(7);
            _playerService.LocalMembershipId.Should().Be(0); // identity ready is not room ready
        }
        [Test]
        public void PlayerList_KeepsLocalPlayerAndPublishesRosterReceived()
        {
            _eventBus.Publish(new ServerHandshakeEvent(7));
            int events = 0;
            _eventBus.Subscribe<PlayerRosterReceivedEvent>(e => events++);
            Enter(new PlayerInfo(1, "alice", Platform.PC, true), new PlayerInfo(2, "bob", Platform.PC, false));
            events.Should().Be(1);
            _playerService.Players.Select(p => p.Id).Should().BeEquivalentTo(new[] { 1, 2, 7 });
        }
        [Test]
        public void PlayerJoined_IsAlreadyInTheRosterWhenTheEventIsPublished()
        {
            _eventBus.Publish(new ServerHandshakeEvent(7)); Enter();
            PlayerInfo seen = null;
            _eventBus.Subscribe<PlayerJoinedEvent>(e => _playerService.TryGetPlayer(e.PlayerId, out seen));
            _dispatcher.Receive(new S2CPlayerJoinedPacket
            { Scope = Scope(3), PlayerId = 3, PlayerName = "carol", Platform = Platform.PC, IsInGame = true });
            seen.Should().NotBeNull(); seen.Name.Should().Be("carol"); seen.IsInGame.Should().BeTrue();
        }
        [Test]
        public void Disconnect_ClearsRosterAndReleasesTheServerAssignedId()
        {
            _eventBus.Publish(new ServerHandshakeEvent(7)); Enter();
            _eventBus.Publish(new ServerDisconnectedEvent("closed"));
            _playerService.Players.Should().BeEmpty(); _playerService.LocalMembershipId.Should().Be(0);
            _playerService.LocalPlayer.Id.Should().Be(0); _playerService.LocalPlayer.Name.Should().Be("me");
        }
        [Test]
        public void Handshake_StartsFromAnEmptyRoster()
        {
            _eventBus.Publish(new ServerHandshakeEvent(7)); Enter(new PlayerInfo(1, "alice", Platform.PC, true));
            _eventBus.Publish(new ServerHandshakeEvent(9));
            _playerService.Players.Select(p => p.Id).Should().Equal(9);
        }
        [Test]
        public void IsInGameUpdate_PublishesTransitionEventsOnlyWhenTheValueChanges()
        {
            _eventBus.Publish(new ServerHandshakeEvent(7)); Enter(new PlayerInfo(1, "alice", Platform.PC, false));
            int entered = 0, quit = 0;
            _eventBus.Subscribe<PlayerEnteredGameEvent>(e => entered++); _eventBus.Subscribe<PlayerQuitGameEvent>(e => quit++);
            _dispatcher.Receive(new S2CIsInGameUpdatePacket { Scope = Scope(1), PlayerId = 1, IsInGame = true });
            _dispatcher.Receive(new S2CIsInGameUpdatePacket { Scope = Scope(1), PlayerId = 1, IsInGame = true });
            _dispatcher.Receive(new S2CIsInGameUpdatePacket { Scope = Scope(1), PlayerId = 1, IsInGame = false });
            entered.Should().Be(1); quit.Should().Be(1);
        }
        [Test]
        public void PlayerLeft_RemovesFromRosterBeforeTheEventIsPublished()
        {
            _eventBus.Publish(new ServerHandshakeEvent(7)); Enter(new PlayerInfo(1, "alice", Platform.PC, true));
            bool present = true;
            _eventBus.Subscribe<PlayerLeftEvent>(e => { PlayerInfo found; present = _playerService.TryGetPlayer(e.PlayerId, out found); });
            _dispatcher.Receive(new S2CPlayerLeftPacket { Scope = Scope(1), PlayerId = 1 });
            present.Should().BeFalse(); _playerService.Players.Select(p => p.Id).Should().Equal(7);
        }
    }
}
