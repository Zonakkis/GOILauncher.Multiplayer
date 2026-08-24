using System;
using FluentAssertions;
using GOILauncher.Multiplayer.Core.Data.Constants;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Data.Packets;
using LiteNetLib.Utils;
using NUnit.Framework;

namespace GOILauncher.Multiplayer.Tests.Data
{
    [TestFixture]
    public class SkinPacketTests
    {
        [Test]
        public void ClientManifest_RoundTripsSlotHashAndGoldness()
        {
            var payload = SkinTestData.Png(0x11);
            var expected = new C2SSkinManifestPacket { State = SkinTestData.State(payload, 1f) };

            var actual = RoundTrip(expected).State;

            actual.Slot.Should().Be(SkinConstants.PotSlot);
            actual.Goldness.Should().Be(1f);
            actual.Hash.Matches(payload).Should().BeTrue();
            actual.HasTexture.Should().BeTrue();
        }

        [Test]
        public void ServerManifest_RoundTripsPlayerIdAndState()
        {
            var payload = SkinTestData.Png(0x22);
            var expected = new S2CSkinManifestPacket
            {
                PlayerId = 7,
                State = SkinTestData.State(payload, 0.5f)
            };

            var actual = RoundTrip(expected);

            actual.PlayerId.Should().Be(7);
            actual.State.Hash.Matches(payload).Should().BeTrue();
            actual.State.Goldness.Should().Be(0.5f);
        }

        /// <summary>
        /// The gold vanilla pot is the reason goldness travels in every manifest, including the
        /// ones with no texture. If an empty hash ever stopped round-tripping as "vanilla, but
        /// still gold", those players would go black on everyone else's screen.
        /// </summary>
        [Test]
        public void VanillaManifest_KeepsGoldnessAndStaysTextureless()
        {
            var actual = RoundTrip(new C2SSkinManifestPacket { State = SkinTestData.Vanilla(1f) }).State;

            actual.HasTexture.Should().BeFalse();
            actual.Hash.IsEmpty.Should().BeTrue();
            actual.Goldness.Should().Be(1f);
        }

        [Test]
        public void ClientData_RoundTripsBlob()
        {
            var payload = SkinTestData.Png(0x33);
            var expected = new C2SSkinDataPacket { Blob = SkinTestData.Blob(payload) };

            var actual = RoundTrip(expected).Blob;

            actual.Data.Should().Equal(payload);
            actual.Hash.Matches(payload).Should().BeTrue();
        }

        [Test]
        public void ServerData_RoundTripsPlayerIdAndBlob()
        {
            var payload = SkinTestData.Png(0x44);
            var expected = new S2CSkinDataPacket { PlayerId = 3, Blob = SkinTestData.Blob(payload) };

            var actual = RoundTrip(expected);

            actual.PlayerId.Should().Be(3);
            actual.Blob.Data.Should().Equal(payload);
        }

        [Test]
        public void Request_RoundTripsPlayerIdAndHash()
        {
            var payload = SkinTestData.Png(0x55);
            var expected = new C2SSkinRequestPacket
            {
                PlayerId = 12,
                Hash = SkinHash.Compute(payload)
            };

            var actual = RoundTrip(expected);

            actual.PlayerId.Should().Be(12);
            actual.Hash.Matches(payload).Should().BeTrue();
        }

        [Test]
        public void Unavailable_RoundTripsPlayerIdAndHash()
        {
            var payload = SkinTestData.Png(0x66);
            var expected = new S2CSkinUnavailablePacket
            {
                PlayerId = 5,
                Hash = SkinHash.Compute(payload)
            };

            var actual = RoundTrip(expected);

            actual.PlayerId.Should().Be(5);
            actual.Hash.Matches(payload).Should().BeTrue();
        }

        /// <summary>
        /// Pins the wire layout. A hash written with a length prefix, or a slot widened to an int,
        /// misaligns every field after it — and the symptom is a garbled texture or an exception
        /// deep inside the reader, not a failure that points here.
        /// </summary>
        [Test]
        public void State_IsSlotPlusFixedLengthHashPlusGoldness()
        {
            var writer = new NetDataWriter();
            SkinTestData.State(SkinTestData.Png(0x77), 1f).Serialize(writer);

            writer.Length.Should().Be(sizeof(byte) + SkinConstants.HashLength + sizeof(float));
        }

        [Test]
        public void Blob_IsFixedLengthHashPlusLengthPrefixedData()
        {
            var payload = SkinTestData.Png(0x88);
            var writer = new NetDataWriter();
            SkinTestData.Blob(payload).Serialize(writer);

            writer.Length.Should().Be(SkinConstants.HashLength + sizeof(int) + payload.Length);
        }

        /// <summary>
        /// An empty hash has to occupy the same 32 bytes as a real one — it is a value, not an
        /// absence, so it must not shorten the packet.
        /// </summary>
        [Test]
        public void EmptyHash_StillOccupiesTheFullHashLength()
        {
            var writer = new NetDataWriter();
            default(SkinHash).Serialize(writer);

            writer.Length.Should().Be(SkinConstants.HashLength);
        }

        [Test]
        public void Blob_WithTruncatedPayload_ThrowsParseException()
        {
            // Announces 1000 bytes and then supplies none.
            var writer = new NetDataWriter();
            writer.Put(default(SkinHash));
            writer.Put(1000);

            Deserializing(writer).Should().Throw<ParseException>();
        }

        /// <summary>
        /// The reader's own guard is <c>AvailableBytes >= length + 4</c>, which overflows for a
        /// length near <see cref="int.MaxValue"/> and lets a 2 GB allocation through as an
        /// OutOfMemoryException — not something <c>PacketDispatcher</c> catches. The size
        /// pre-check in <see cref="SkinBlob"/> is what turns it into a ParseException.
        /// </summary>
        [Test]
        public void Blob_WithForgedHugeLength_ThrowsParseExceptionInsteadOfAllocating()
        {
            var writer = new NetDataWriter();
            writer.Put(default(SkinHash));
            writer.Put(int.MaxValue);

            Deserializing(writer).Should().Throw<ParseException>();
        }

        [Test]
        public void Blob_WithLengthOverTheLimit_ThrowsParseException()
        {
            var writer = new NetDataWriter();
            writer.Put(default(SkinHash));
            writer.Put(SkinConstants.MaxPayloadBytes + 1);

            Deserializing(writer).Should().Throw<ParseException>();
        }

        [Test]
        public void Hash_WithTruncatedBytes_ThrowsParseException()
        {
            var writer = new NetDataWriter();
            writer.Put(new byte[SkinConstants.HashLength - 1], 0, SkinConstants.HashLength - 1);

            Deserializing(writer).Should().Throw<ParseException>();
        }

        private static Action Deserializing(NetDataWriter writer)
        {
            var reader = new NetDataReader(writer.CopyData());
            return () => reader.Get<SkinBlob>();
        }

        private static C2SSkinManifestPacket RoundTrip(C2SSkinManifestPacket packet)
        {
            return RoundTripStruct<C2SSkinManifestPacket>(packet);
        }

        private static S2CSkinManifestPacket RoundTrip(S2CSkinManifestPacket packet)
        {
            return RoundTripStruct<S2CSkinManifestPacket>(packet);
        }

        private static C2SSkinDataPacket RoundTrip(C2SSkinDataPacket packet)
        {
            return RoundTripStruct<C2SSkinDataPacket>(packet);
        }

        private static S2CSkinDataPacket RoundTrip(S2CSkinDataPacket packet)
        {
            return RoundTripStruct<S2CSkinDataPacket>(packet);
        }

        private static C2SSkinRequestPacket RoundTrip(C2SSkinRequestPacket packet)
        {
            return RoundTripStruct<C2SSkinRequestPacket>(packet);
        }

        private static S2CSkinUnavailablePacket RoundTrip(S2CSkinUnavailablePacket packet)
        {
            return RoundTripStruct<S2CSkinUnavailablePacket>(packet);
        }

        private static TPacket RoundTripStruct<TPacket>(TPacket packet)
            where TPacket : struct, INetSerializable
        {
            var writer = new NetDataWriter();
            packet.Serialize(writer);
            return new NetDataReader(writer.CopyData()).Get<TPacket>();
        }
    }
}
