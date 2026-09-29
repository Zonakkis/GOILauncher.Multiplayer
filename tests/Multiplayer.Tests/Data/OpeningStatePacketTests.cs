using FluentAssertions;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Data.Packets;
using LiteNetLib.Utils;
using NUnit.Framework;

namespace GOILauncher.Multiplayer.Tests.Data
{
    [TestFixture]
    public class OpeningStatePacketTests
    {
        [TestCase(0f)]
        [TestCase(2.7121f)]
        public void State_EncodesOnlyRemainingSeconds(float seconds)
        {
            var writer = new NetDataWriter();
            new OpeningState { RemainingSeconds = seconds }.Serialize(writer);

            writer.Length.Should().Be(4);
            var reader = new NetDataReader(writer.CopyData());
            reader.Get<OpeningState>().RemainingSeconds.Should().Be(seconds);
            reader.AvailableBytes.Should().Be(0);
        }

        [Test]
        public void ClientPacket_RoundTripsMembershipAndRemainingTime()
        {
            var writer = new NetDataWriter();
            new C2SOpeningStatePacket
            {
                MembershipId = 42,
                State = new OpeningState { RemainingSeconds = 2.5f }
            }.Serialize(writer);

            var packet = new C2SOpeningStatePacket();
            var reader = new NetDataReader(writer.CopyData());
            packet.Deserialize(reader);

            writer.Length.Should().Be(12);
            packet.MembershipId.Should().Be(42);
            packet.State.RemainingSeconds.Should().Be(2.5f);
            reader.AvailableBytes.Should().Be(0);
        }

        [Test]
        public void ServerPacket_RoundTripsIdentityScopeAndCompletedState()
        {
            var writer = new NetDataWriter();
            new S2COpeningStatePacket
            {
                PlayerId = 9,
                Scope = new RoomPacketScope { RecipientMembershipId = 42, PlayerMembershipId = 81 },
                State = new OpeningState { RemainingSeconds = 0f }
            }.Serialize(writer);

            var packet = new S2COpeningStatePacket();
            var reader = new NetDataReader(writer.CopyData());
            packet.Deserialize(reader);

            packet.PlayerId.Should().Be(9);
            packet.Scope.RecipientMembershipId.Should().Be(42);
            packet.Scope.PlayerMembershipId.Should().Be(81);
            packet.State.RemainingSeconds.Should().Be(0f);
            reader.AvailableBytes.Should().Be(0);
        }

        [TestCase(-1f, false)]
        [TestCase(float.NaN, false)]
        [TestCase(float.PositiveInfinity, false)]
        [TestCase(float.NegativeInfinity, false)]
        [TestCase(0f, true)]
        [TestCase(6.60835f, true)]
        public void State_RejectsNonFiniteAndNegativeTime(float seconds, bool valid)
        {
            new OpeningState { RemainingSeconds = seconds }.IsValid.Should().Be(valid);
        }
    }
}
