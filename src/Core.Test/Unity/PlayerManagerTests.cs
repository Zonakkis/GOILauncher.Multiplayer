using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Unity;
using Moq;
using NUnit.Framework;

namespace GOILauncher.Multiplayer.Core.Test.Unity
{
    [TestFixture]
    public class PlayerManagerTests
    {
        [Test]
        public void HandshakeWhileAlreadyInGame_InitializesGamePlayerLifecycle()
        {
            var eventBus = new EventBus(new Mock<ILogger<EventBus>>().Object);
            var gameManager = new Mock<IGameManager>();
            gameManager.SetupGet(m => m.IsInGame).Returns(true);
            var instancePool = new Mock<IPlayerInstancePool>();
            var logger = new Mock<ILogger<PlayerManager>>();
            var playerManager = new PlayerManager(
                eventBus,
                gameManager.Object,
                instancePool.Object,
                logger.Object);

            playerManager.Init();
            eventBus.Publish(new ServerHandshakeEvent(7));

            instancePool.Verify(pool => pool.WarmUp(4), Times.Once);
        }
    }
}
