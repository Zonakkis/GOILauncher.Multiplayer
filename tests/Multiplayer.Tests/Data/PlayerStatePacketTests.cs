using FluentAssertions;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Utils;
using LiteNetLib.Utils;
using NUnit.Framework;

namespace GOILauncher.Multiplayer.Tests.Data
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

        /// <summary>
        /// Pins the intentional loss: a rotation with non-zero X/Y comes back with both at 0.
        /// The synced transforms only rotate around Z, so this is the domain fact and not a bug —
        /// but it has to be stated somewhere that fails if the read side stops agreeing.
        /// </summary>
        [Test]
        public void Rotation_DropsXAndY_OnTheWire()
        {
            var packet = new C2SPlayerStatePacket
            {
                Sequence = 1,
                State = new PlayerState
                {
                    PlayerRotation = new UnityQuaternion { X = 1f, Y = 2f, Z = 3f, W = 4f }
                }
            };

            var actual = RoundTrip(packet).State.PlayerRotation;

            actual.X.Should().Be(0f);
            actual.Y.Should().Be(0f);
            actual.Z.Should().Be(3f);
            actual.W.Should().Be(4f);
        }

        /// <summary>
        /// The write and read sides once disagreed on how many floats a quaternion is, which
        /// misaligned every field after the first rotation. Asserting the exact byte count catches
        /// that at the source instead of as an ArgumentOutOfRangeException deep in the reader.
        /// </summary>
        [Test]
        public void State_IsThreePositionsAndThreeZWRotations()
        {
            var writer = new NetDataWriter();
            CreateState(1).Serialize(writer);

            writer.Length.Should().Be((3 * 3 + 3 * 2) * sizeof(float));
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
            // Quaternion X/Y are always 0: only Z/W travel, so filling them in would make these
            // tests assert something the protocol cannot do. Z/W still differ per field, so a
            // swapped field order is still caught by AssertState.
            return new PlayerState
            {
                PlayerPosition = new UnityVector3 { X = value, Y = value + 1, Z = value + 2 },
                PlayerRotation = new UnityQuaternion { Z = value + 2, W = value + 3 },
                HandlePosition = new UnityVector3 { X = value + 4, Y = value + 5, Z = value + 6 },
                HandleRotation = new UnityQuaternion { Z = value + 9, W = value + 10 },
                SliderPosition = new UnityVector3 { X = value + 11, Y = value + 12, Z = value + 13 },
                SliderRotation = new UnityQuaternion { Z = value + 16, W = value + 17 }
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
