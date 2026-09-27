using FluentAssertions;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Unity;
using NLog.Layouts;
using NLog.Targets;
using NUnit.Framework;

namespace GOILauncher.Multiplayer.Tests.Unity
{
    /// <summary>
    /// MultiplayerOptions is the input to MultiplayerCore.Initialize. LogTarget must default to
    /// the module's shared console sink so a host that configures nothing still logs, and so
    /// Initialize can register options.LogTarget unconditionally (no null branch).
    /// </summary>
    [TestFixture]
    public class MultiplayerOptionsTests
    {
        [Test]
        public void LogTarget_DefaultsToTheSharedConsoleTarget()
        {
            var options = new MultiplayerOptions();

            options.LogTarget.Should().BeOfType<ConsoleTarget>();
            ((SimpleLayout)((ConsoleTarget)options.LogTarget).Layout).OriginalText
                .Should().Be(DefaultLogTarget.LayoutText);
        }
    }
}
