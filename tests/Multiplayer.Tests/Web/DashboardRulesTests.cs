using System;
using System.Collections.Generic;
using System.Linq;
using GOILauncher.Multiplayer.DedicatedServer.Web;
using FluentAssertions;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Server.Services;
using NUnit.Framework;

namespace GOILauncher.Multiplayer.Tests.Web
{
    /// <summary>
    /// 面板的判断层：什么算卡顿、什么算慢、什么时候该报警。
    /// 这些阈值决定面板会不会"狼来了"，所以边界值比典型值更值得钉死 ——
    /// 阈值两侧各测一次，改常量时立刻会红。
    /// </summary>
    [TestFixture]
    public class DashboardRulesTests
    {
        private static ConnectionStatsObservation Stats(long sent, long received) =>
            new ConnectionStatsObservation(sent, received, 0, 0, sent - received, 0);

        private static ConnectionObservation Conn(int id, string name, int roomId = 0, bool handshaked = true,
            int? latency = null, int errors = 0, ConnectionStatsObservation stats = null) =>
            new ConnectionObservation(id, name, Platform.PC, false, roomId, "127.0.0.1:1", latency, stats, errors, handshaked);

        [Test]
        public void IsStalled_OnlyWhenGapExceedsThreshold()
        {
            DashboardRules.IsStalled(TimeSpan.FromMilliseconds(999)).Should().BeFalse();
            // 阈值本身不算卡顿：这是"超过 1000ms"而不是"达到 1000ms"。
            DashboardRules.IsStalled(TimeSpan.FromMilliseconds(1000)).Should().BeFalse();
            DashboardRules.IsStalled(TimeSpan.FromMilliseconds(1001)).Should().BeTrue();
        }

        [Test]
        public void Classify_TreatsMissingSampleAsUnknownNotZero()
        {
            // LiteNetLib 还没报过延迟时是 null，不能当成 0ms 显示成"最快"。
            DashboardRules.Classify(null).Should().Be(LatencyLevel.Unknown);
            DashboardRules.Classify(0).Should().Be(LatencyLevel.Ok);
            DashboardRules.Classify(150).Should().Be(LatencyLevel.Ok);
            DashboardRules.Classify(151).Should().Be(LatencyLevel.Warn);
            DashboardRules.Classify(299).Should().Be(LatencyLevel.Warn);
            DashboardRules.Classify(300).Should().Be(LatencyLevel.Bad);
        }

        [Test]
        public void LossPercent_NeedsEnoughSamplesBeforeItMeansAnything()
        {
            DashboardRules.LossPercent(null).Should().BeNull();
            // 前几个包抖一下就能算出 50%，那是噪声不是信号。
            DashboardRules.LossPercent(Stats(99, 40)).Should().BeNull();
            DashboardRules.LossPercent(Stats(100, 99)).Should().BeApproximately(1.0, 0.0001);
            DashboardRules.LossPercent(Stats(1000, 990)).Should().BeApproximately(1.0, 0.0001);
        }

        [Test]
        public void IsLossWarn_OnlyAboveThreshold()
        {
            DashboardRules.IsLossWarn(null).Should().BeFalse();
            DashboardRules.IsLossWarn(1.0).Should().BeFalse();
            DashboardRules.IsLossWarn(1.01).Should().BeTrue();
        }

        [Test]
        public void CapacityText_RendersZeroMaxPlayersAsUnlimited()
        {
            DashboardRules.CapacityText(0, 0).Should().Be("0/∞");
            DashboardRules.CapacityText(3, 0).Should().Be("3/∞");
            DashboardRules.CapacityText(2, 8).Should().Be("2/8");
        }

        [Test]
        public void FormatUptime_DoesNotWrapAtOneDay()
        {
            // 连续运行时长比日历格式重要：25 小时要显示 25:00:00，不是 01:00:00。
            DashboardRules.FormatUptime(null).Should().Be("-");
            DashboardRules.FormatUptime(TimeSpan.Zero).Should().Be("0:00:00");
            DashboardRules.FormatUptime(new TimeSpan(0, 1, 2, 3)).Should().Be("1:02:03");
            DashboardRules.FormatUptime(TimeSpan.FromHours(25)).Should().Be("25:00:00");
        }

        [Test]
        public void PlatformLabel_MapsEveryEnumMember()
        {
            DashboardRules.PlatformLabel(Platform.PC).Should().Be("PC");
            DashboardRules.PlatformLabel(Platform.iOS).Should().Be("iOS");
            DashboardRules.PlatformLabel(Platform.Android).Should().Be("安卓");
            DashboardRules.PlatformLabel(Platform.Unknown).Should().Be("未知");
        }

        [Test]
        public void FormatCount_SwitchesUnitAtThousand()
        {
            DashboardRules.FormatCount(0).Should().Be("0");
            DashboardRules.FormatCount(999).Should().Be("999");
            DashboardRules.FormatCount(1000).Should().Be("1.0k");
            DashboardRules.FormatCount(1234567).Should().Be("1.2M");
        }

        [Test]
        public void OrderConnections_MustOnlyReorder_NeverDropOrDuplicate()
        {
            // 结构契约而不是具体顺序：顺序是操作员自己的判断（见 OrderConnections 的 TODO），
            // 但无论怎么排，都不能少一个人也不能多出一个人。
            var input = new List<ConnectionObservation>
            {
                Conn(7, "g"), Conn(3, "c"), Conn(11, "k"), Conn(1, "a"), Conn(5, "e")
            };

            var actual = DashboardRules.OrderConnections(input).ToList();

            actual.Should().HaveCount(input.Count);
            actual.Select(c => c.PlayerId).Should().BeEquivalentTo(input.Select(c => c.PlayerId));
        }

        [Test]
        public void OrderConnections_HandlesEmptyInput()
        {
            DashboardRules.OrderConnections(new List<ConnectionObservation>()).Should().BeEmpty();
        }
    }
}
