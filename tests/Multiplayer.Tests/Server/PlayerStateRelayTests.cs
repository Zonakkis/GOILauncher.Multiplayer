using FluentAssertions;
using Autofac;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Server.Services;
using GOILauncher.Multiplayer.Server.Synchronization;
using LiteNetLib;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace GOILauncher.Multiplayer.Tests.Server
{
    [TestFixture]
    public class PlayerStateRelayTests
    {
        private const int SenderId = 1;

        private FakeNetworkServer _networkServer;
        private RecordingServerDispatcher _dispatcher;
        private StubServerPlayerService _playerService;

        [SetUp]
        public void Setup()
        {
            _networkServer = new FakeNetworkServer();
            _dispatcher = new RecordingServerDispatcher();
            _playerService = new StubServerPlayerService();
            var relay = new PlayerStateRelay(_networkServer, _dispatcher, _playerService, new LobbyRoomStub(_playerService));
            ((IStartable)relay).Start();
        }

        [Test]
        public void UnknownSender_IsDropped()
        {
            AddPlayer(2, true);

            Relay(SenderId, 7);

            _networkServer.Sent.Should().BeEmpty();
        }

        [Test]
        public void SenderOutsideGameplayScene_IsDropped()
        {
            AddPlayer(SenderId, false);
            AddPlayer(2, true);

            Relay(SenderId, 7);

            _networkServer.Sent.Should().BeEmpty();
        }

        [Test]
        public void RelaysOnlyToOtherPlayersInGame_Unreliably()
        {
            AddPlayer(SenderId, true);
            AddPlayer(2, true);
            AddPlayer(3, false);
            AddPlayer(4, true);

            Relay(SenderId, 7);

            _networkServer.Sent.Select(s => s.ClientId).Should().BeEquivalentTo(new[] { 2, 4 });
            _networkServer.Sent.Should().OnlyContain(s => s.Method == DeliveryMethod.Unreliable);
        }

        [Test]
        public void PlayerIdComesFromSender_NotFromPacketBody()
        {
            AddPlayer(SenderId, true);
            AddPlayer(2, true);

            Relay(SenderId, 7);

            var relayed = (S2CPlayerStatePacket)_networkServer.Sent.Single().Packet;
            relayed.PlayerId.Should().Be(SenderId);
            relayed.Sequence.Should().Be(7u);
        }

        private void Relay(int senderId, uint sequence)
        {
            _dispatcher.Receive(
                new C2SPlayerStatePacket { MembershipId = (ulong)senderId + 1, Sequence = sequence, State = default(PlayerState) },
                senderId);
        }

        private void AddPlayer(int id, bool isInGame)
        {
            _playerService.AddPlayer(id, isInGame);
        }

        private sealed class StubServerPlayerService : IPlayerService
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
        }
    }
}
