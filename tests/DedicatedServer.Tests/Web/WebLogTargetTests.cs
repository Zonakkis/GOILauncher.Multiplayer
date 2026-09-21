using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GOILauncher.Multiplayer.DedicatedServer.Web;
using NLog;
using NLog.Config;
using NUnit.Framework;

namespace GOILauncher.Multiplayer.DedicatedServer.Tests.Web
{
    [TestFixture]
    public class WebLogTargetTests
    {
        [Test]
        public void ConcurrentWrites_ProduceContiguousOrderedSequences()
        {
            var config = new LoggingConfiguration();
            var target = new WebLogTarget { Name = "WebLogConcurrencyTest" };
            config.AddTarget(target);
            config.AddRuleForAllLevels(target);

            using var factory = new LogFactory { Configuration = config };
            Parallel.For(0, 500, i => factory.GetLogger("test").Info("message " + i));

            var rows = target.Fetch(0, 1000, out var nextSeq, out var reset);

            reset.Should().BeTrue();
            rows.Should().HaveCount(300);
            rows.Select(row => row.Seq).Should().BeInAscendingOrder();
            rows.Select(row => row.Seq).Should().OnlyHaveUniqueItems();
            rows.Last().Seq.Should().Be(nextSeq);
            rows.First().Seq.Should().Be(nextSeq - 299);
        }
    }
}
