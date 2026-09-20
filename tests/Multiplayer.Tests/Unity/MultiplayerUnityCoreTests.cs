using FluentAssertions;
using GOILauncher.Multiplayer.Unity;
using NUnit.Framework;

namespace GOILauncher.Multiplayer.Tests.Unity
{
    /// <summary>
    /// The unloaded contract. Initialize and Dispose need a real Unity runtime (they create a
    /// GameObject and add components), so what can be pinned down here is the part the UI depends on:
    /// an unloaded core answers with null rather than an exception, and disposing twice is harmless.
    /// </summary>
    [TestFixture, NonParallelizable]
    public class MultiplayerUnityCoreTests
    {
        [Test]
        public void Unloaded_FacadesAreNullRatherThanThrowing()
        {
            MultiplayerUnityCore.IsLoaded.Should().BeFalse();
            MultiplayerUnityCore.GameManager.Should().BeNull();
            MultiplayerUnityCore.UnityClient.Should().BeNull();
            MultiplayerUnityCore.UnityServer.Should().BeNull();
        }

        [Test]
        public void Unloaded_DisposeIsNoOp()
        {
            Assert.DoesNotThrow(() => MultiplayerUnityCore.Dispose());
            MultiplayerUnityCore.IsLoaded.Should().BeFalse();
        }
    }
}
