using System.Linq;
using Autofac;
using FluentAssertions;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Server.Services;
using LiteNetLib;
using Moq;
using NUnit.Framework;

namespace GOILauncher.Multiplayer.Tests.Server
{
    [TestFixture]
    public class ServerPlayerServiceTests
    {
        private FakeNetworkServer _networkServer;
        private RecordingServerDispatcher _dispatcher;
        private EventBus _eventBus;
        private PlayerService _playerService;

        [SetUp]
        public void Setup()
        {
            _networkServer = new FakeNetworkServer();
            _dispatcher = new RecordingServerDispatcher();
            _eventBus = new EventBus(new Mock<ILogger<EventBus>>().Object);
            _playerService = new PlayerService(
                _networkServer,
                _dispatcher,
                _eventBus,
                new Mock<ILogger<PlayerService>>().Object);
            ((IStartable)_playerService).Start();
        }

        [Test]
        public void Handshake_UsesSenderIdAsPlayerId()
        {
            Handshake(3, "alice", true);

            _playerService.Players.Should().ContainKey(3);
            _playerService.Players[3].Name.Should().Be("alice");
            _playerService.Players[3].IsInGame.Should().BeTrue();
        }

        [Test]
        public void Handshake_NotifiesExistingPlayers_ButNotTheJoiner()
        {
            Handshake(1, "alice", false);
            _networkServer.Sent.Clear();

            Handshake(2, "bob", false);

            var joined = _networkServer.Sent
                .Where(s => s.Packet is S2CPlayerJoinedPacket)
                .ToList();
            joined.Select(s => s.ClientId).Should().BeEquivalentTo(new[] { 1 });
            ((S2CPlayerJoinedPacket)joined.Single().Packet).PlayerId.Should().Be(2);
        }

        [Test]
        public void Handshake_SendsExistingRosterToJoiner_ExcludingSelf()
        {
            Handshake(1, "alice", false);
            _networkServer.Sent.Clear();

            Handshake(2, "bob", false);

            var listSend = _networkServer.Sent.Single(s => s.Packet is S2CPlayerListPacket);
            listSend.ClientId.Should().Be(2);
            var list = (S2CPlayerListPacket)listSend.Packet;
            list.Players.Select(p => p.Id).Should().BeEquivalentTo(new[] { 1 });
        }

        [Test]
        public void Disconnect_RemovesPlayerAndNotifiesRemaining()
        {
            Handshake(1, "alice", false);
            Handshake(2, "bob", false);
            _networkServer.Sent.Clear();

            _eventBus.Publish(new ClientDisconnectedEvent(2, "test"));

            _playerService.Players.Should().NotContainKey(2);
            var left = _networkServer.Sent.Single(s => s.Packet is S2CPlayerLeftPacket);
            left.ClientId.Should().Be(1);
            ((S2CPlayerLeftPacket)left.Packet).PlayerId.Should().Be(2);
        }

        [Test]
        public void IsInGameUpdate_UsesSenderIdAndFansOutToOthers()
        {
            Handshake(1, "alice", false);
            Handshake(2, "bob", false);
            _networkServer.Sent.Clear();

            _dispatcher.Receive(new C2SIsInGameUpdatePacket(true), 2);

            _playerService.Players[2].IsInGame.Should().BeTrue();
            var update = _networkServer.Sent.Single(s => s.Packet is S2CIsInGameUpdatePacket);
            update.ClientId.Should().Be(1);
            var payload = (S2CIsInGameUpdatePacket)update.Packet;
            payload.PlayerId.Should().Be(2);
            payload.IsInGame.Should().BeTrue();
            update.Method.Should().Be(DeliveryMethod.ReliableOrdered);
        }

        [Test]
        public void IsInGameUpdate_FromUnknownSender_IsIgnored()
        {
            _dispatcher.Receive(new C2SIsInGameUpdatePacket(true), 9);

            _networkServer.Sent.Should().BeEmpty();
        }

        private void Handshake(int senderId, string name, bool isInGame)
        {
            _dispatcher.Receive(
                new C2SClientHandShakePacket
                {
                    PlayerName = name,
                    Platform = Platform.PC,
                    IsInGame = isInGame
                },
                senderId);
        }
    }
}
