using System.Collections.Generic;
using Autofac;
using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Client.Services;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Unity;
using Moq;
using NUnit.Framework;

namespace GOILauncher.Multiplayer.Core.Test.Unity
{
    /// <summary>
    /// PlayerManager owns remote instance lifecycle only; the roster it reads belongs to
    /// the client PlayerService. The pool is mocked, so these tests assert on the decision
    /// (rent / don't rent) rather than on the resulting MonoBehaviour, which cannot be
    /// constructed outside a Unity runtime.
    /// </summary>
    [TestFixture]
    public class PlayerManagerTests
    {
        private const int LocalPlayerId = 7;

        private EventBus _eventBus;
        private Mock<IGameManager> _gameManager;
        private Mock<IPlayerInstancePool> _instancePool;
        private FakePlayerService _playerService;
        private PlayerManager _playerManager;

        [SetUp]
        public void Setup()
        {
            _eventBus = new EventBus(new Mock<ILogger<EventBus>>().Object);
            _gameManager = new Mock<IGameManager>();
            _gameManager.SetupGet(m => m.IsInGame).Returns(true);
            _instancePool = new Mock<IPlayerInstancePool>();
            _playerService = new FakePlayerService();
            _playerService.SetLocalPlayerInfo(new PlayerInfo(LocalPlayerId, "me", Platform.PC, true));
            _playerService.Add(_playerService.LocalPlayer);

            _playerManager = new PlayerManager(
                _eventBus,
                _gameManager.Object,
                _playerService,
                _instancePool.Object,
                new Mock<ILogger<PlayerManager>>().Object);
            ((IStartable)_playerManager).Start();        }

        [Test]
        public void HandshakeWhileAlreadyInGame_InitializesGamePlayerLifecycle()
        {
            _eventBus.Publish(new LocalPlayerReadyEvent(_playerService.LocalPlayer));

            _instancePool.Verify(pool => pool.WarmUp(4), Times.Once);
        }

        [Test]
        public void HandshakeWhileOutsideGame_DoesNotTouchThePool()
        {
            _gameManager.SetupGet(m => m.IsInGame).Returns(false);

            _eventBus.Publish(new LocalPlayerReadyEvent(_playerService.LocalPlayer));

            _instancePool.Verify(pool => pool.WarmUp(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public void RosterReceived_RentsOnlyForRemotePlayersInGame()
        {
            _playerService.Add(new PlayerInfo(2, "alice", Platform.PC, true));
            _playerService.Add(new PlayerInfo(3, "bob", Platform.PC, false));

            _eventBus.Publish(new PlayerRosterReceivedEvent());

            _instancePool.Verify(pool => pool.Rent(It.Is<PlayerInfo>(p => p.Id == 2)), Times.Once);
            _instancePool.Verify(pool => pool.Rent(It.Is<PlayerInfo>(p => p.Id == 3)), Times.Never);
            _instancePool.Verify(pool => pool.Rent(It.Is<PlayerInfo>(p => p.Id == LocalPlayerId)), Times.Never);
        }

        [Test]
        public void RosterReceived_OutsideGame_RentsNothing()
        {
            _gameManager.SetupGet(m => m.IsInGame).Returns(false);
            _playerService.Add(new PlayerInfo(2, "alice", Platform.PC, true));

            _eventBus.Publish(new PlayerRosterReceivedEvent());

            _instancePool.Verify(pool => pool.Rent(It.IsAny<PlayerInfo>()), Times.Never);
        }

        [Test]
        public void PlayerJoinedInGame_RentsAnInstance()
        {
            // PlayerService writes the roster before publishing, so the info is readable here.
            _playerService.Add(new PlayerInfo(4, "carol", Platform.PC, true));

            _eventBus.Publish(new PlayerJoinedEvent(4, "carol", Platform.PC, true));

            _instancePool.Verify(pool => pool.Rent(It.Is<PlayerInfo>(p => p.Id == 4)), Times.Once);
        }

        [Test]
        public void PlayerJoinedInLobby_RentsNothing()
        {
            _playerService.Add(new PlayerInfo(4, "carol", Platform.PC, false));

            _eventBus.Publish(new PlayerJoinedEvent(4, "carol", Platform.PC, false));

            _instancePool.Verify(pool => pool.Rent(It.IsAny<PlayerInfo>()), Times.Never);
        }

        [Test]
        public void PlayerEnteredGame_RentsAnInstance()
        {
            var player = new PlayerInfo(5, "dave", Platform.PC, true);
            _playerService.Add(player);

            _eventBus.Publish(new PlayerEnteredGameEvent(player));

            _instancePool.Verify(pool => pool.Rent(It.Is<PlayerInfo>(p => p.Id == 5)), Times.Once);
        }

        [Test]
        public void LocalPlayer_NeverGetsARemoteInstance()
        {
            _eventBus.Publish(new PlayerEnteredGameEvent(_playerService.LocalPlayer));

            _instancePool.Verify(pool => pool.Rent(It.IsAny<PlayerInfo>()), Times.Never);
        }

        /// <summary>
        /// Regression guard for the single-driver rule: PlayerListUpdatedEvent is a UI
        /// refresh signal. If it ever drives instance lifecycle again, the same roster
        /// change would be handled twice.
        /// </summary>
        [Test]
        public void PlayerListUpdated_DoesNotDriveInstanceLifecycle()
        {
            _playerService.Add(new PlayerInfo(2, "alice", Platform.PC, true));

            _eventBus.Publish(new PlayerListUpdatedEvent());

            _instancePool.Verify(pool => pool.Rent(It.IsAny<PlayerInfo>()), Times.Never);
        }

        private sealed class FakePlayerService : IPlayerService
        {
            private readonly Dictionary<int, PlayerInfo> _players = new Dictionary<int, PlayerInfo>();

            public PlayerInfo LocalPlayer { get; private set; }

            public IEnumerable<PlayerInfo> Players
            {
                get { return _players.Values; }
            }

            public bool TryGetPlayer(int playerId, out PlayerInfo player)
            {
                return _players.TryGetValue(playerId, out player);
            }

            public void SetLocalPlayerInfo(PlayerInfo info)
            {
                LocalPlayer = info;
            }

            public void SetIsInGame(bool isInGame)
            {
            }

            public void Add(PlayerInfo info)
            {
                _players[info.Id] = info;
            }
        }
    }
}
