using FluentAssertions;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Utils;
using LiteNetLib.Utils;
using NUnit.Framework;

namespace GOILauncher.Multiplayer.Core.Test.Data
{
    [TestFixture]
    public class PlayerStatePacketTests
    {
        [Test]
        public void ClientPacket_RoundTripsSequenceAndState()
        {
            var expected = new C2SPlayerStatePacket
            {
                Sequence = 17,
                State = CreateState(1)
            };

            var actual = RoundTrip(expected);

            actual.Sequence.Should().Be(expected.Sequence);
            AssertState(actual.State, expected.State);
        }

        [Test]
        public void ServerPacket_RoundTripsPlayerIdSequenceAndState()
        {
            var expected = new S2CPlayerStatePacket
            {
                PlayerId = 9,
                Sequence = 23,
                State = CreateState(2)
            };

            var actual = RoundTrip(expected);

            actual.PlayerId.Should().Be(expected.PlayerId);
            actual.Sequence.Should().Be(expected.Sequence);
            AssertState(actual.State, expected.State);
        }

        [Test]
        public void SequenceNumber_AcceptsForwardProgressAcrossWrapAround()
        {
            SequenceNumber.IsNewer(0u, uint.MaxValue).Should().BeTrue();
            SequenceNumber.IsNewer(uint.MaxValue, 0u).Should().BeFalse();
            SequenceNumber.IsNewer(0x80000000u, 0u).Should().BeFalse();
        }

        private static C2SPlayerStatePacket RoundTrip(C2SPlayerStatePacket packet)
        {
            var writer = new NetDataWriter();
            packet.Serialize(writer);
            var result = new C2SPlayerStatePacket();
            result.Deserialize(new NetDataReader(writer.CopyData()));
            return result;
        }

        private static S2CPlayerStatePacket RoundTrip(S2CPlayerStatePacket packet)
        {
            var writer = new NetDataWriter();
            packet.Serialize(writer);
            var result = new S2CPlayerStatePacket();
            result.Deserialize(new NetDataReader(writer.CopyData()));
            return result;
        }

        private static PlayerState CreateState(float value)
        {
            return new PlayerState
            {
                PlayerPosition = new UnityVector3 { X = value, Y = value + 1, Z = value + 2 },
                PlayerRotation = new UnityQuaternion { X = value, Y = value + 1, Z = value + 2, W = value + 3 },
                HandlePosition = new UnityVector3 { X = value + 4, Y = value + 5, Z = value + 6 },
                HandleRotation = new UnityQuaternion { X = value + 7, Y = value + 8, Z = value + 9, W = value + 10 },
                SliderPosition = new UnityVector3 { X = value + 11, Y = value + 12, Z = value + 13 },
                SliderRotation = new UnityQuaternion { X = value + 14, Y = value + 15, Z = value + 16, W = value + 17 }
            };
        }

        private static void AssertState(PlayerState actual, PlayerState expected)
        {
            actual.PlayerPosition.Should().Be(expected.PlayerPosition);
            actual.PlayerRotation.Should().Be(expected.PlayerRotation);
            actual.HandlePosition.Should().Be(expected.HandlePosition);
            actual.HandleRotation.Should().Be(expected.HandleRotation);
            actual.SliderPosition.Should().Be(expected.SliderPosition);
            actual.SliderRotation.Should().Be(expected.SliderRotation);
        }
    }
}
