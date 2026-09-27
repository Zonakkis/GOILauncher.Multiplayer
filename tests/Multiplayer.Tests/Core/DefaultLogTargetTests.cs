using FluentAssertions;
using GOILauncher.Multiplayer.Core.Log;
using NLog.Layouts;
using NLog.Targets;
using NUnit.Framework;

namespace GOILauncher.Multiplayer.Tests.Core
{
    /// <summary>
    /// DefaultLogTarget is the single definition of the module's fallback sink: the console
    /// target that RegisterMultiplayerCore registers and that MultiplayerOptions.LogTarget
    /// defaults to. The layout string used to be copy-pasted; this pins the one place it lives.
    /// </summary>
    [TestFixture]
    public class DefaultLogTargetTests
    {
        [Test]
        public void Create_ReturnsConsoleTargetWithTheSharedLayout()
        {
            Target target = DefaultLogTarget.Create();

            target.Should().BeOfType<ConsoleTarget>();
            ((SimpleLayout)((ConsoleTarget)target).Layout).OriginalText.Should()
                .Be(@"${date:format=yyyy-MM-dd HH\:mm\:ss}|${level:uppercase=true}|${logger:shortName=true}|${message}${onexception:inner=${newline}${exception:format=tostring}}");
        }
    }
}
