using FluentAssertions;
using GOILauncher.Multiplayer.Core.Data.Constants;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Utils;
using NUnit.Framework;

namespace GOILauncher.Multiplayer.Tests.Data
{
    [TestFixture]
    public class SkinPayloadTests
    {
        [Test]
        public void PngHeader_ReadsSizeFromIhdr()
        {
            int width, height;

            PngHeader.TryReadSize(SkinTestData.Png(1024, 512, 0x01), out width, out height)
                .Should().BeTrue();

            width.Should().Be(1024);
            height.Should().Be(512);
        }

        [Test]
        public void PngHeader_RejectsNull()
        {
            int width, height;
            PngHeader.TryReadSize(null, out width, out height).Should().BeFalse();
        }

        /// <summary>
        /// A payload shorter than the header must not be read past its end. This is the guard that
        /// keeps a two-byte packet from becoming an IndexOutOfRangeException.
        /// </summary>
        [Test]
        public void PngHeader_RejectsPayloadShorterThanTheHeader()
        {
            var truncated = new byte[20];
            System.Array.Copy(SkinTestData.Png(64, 64, 0x01), truncated, truncated.Length);

            int width, height;
            PngHeader.TryReadSize(truncated, out width, out height).Should().BeFalse();
        }

        [Test]
        public void PngHeader_RejectsNonPngSignature()
        {
            var data = SkinTestData.Png(64, 64, 0x01);
            data[1] = 0x00;

            int width, height;
            PngHeader.TryReadSize(data, out width, out height).Should().BeFalse();
        }

        [Test]
        public void PngHeader_RejectsChunkThatIsNotIhdr()
        {
            var data = SkinTestData.Png(64, 64, 0x01);
            data[12] = (byte)'X';

            int width, height;
            PngHeader.TryReadSize(data, out width, out height).Should().BeFalse();
        }

        [Test]
        public void PngHeader_RejectsZeroSize()
        {
            int width, height;
            PngHeader.TryReadSize(SkinTestData.Png(0, 64, 0x01), out width, out height)
                .Should().BeFalse();
        }

        /// <summary>
        /// A width with the high bit set is illegal per the PNG spec and reads back as a negative
        /// int here. It has to be rejected rather than compared against the size limit, where a
        /// negative number would pass.
        /// </summary>
        [Test]
        public void PngHeader_RejectsSizeWithTheHighBitSet()
        {
            var data = SkinTestData.Png(64, 64, 0x01);
            data[16] = 0xFF;

            int width, height;
            PngHeader.TryReadSize(data, out width, out height).Should().BeFalse();
        }

        [Test]
        public void Validator_AcceptsAPngWhoseHashMatches()
        {
            string reason;

            SkinPayloadValidator.IsValid(SkinTestData.Blob(SkinTestData.Png(0x01)), out reason)
                .Should().BeTrue();

            reason.Should().BeNull();
        }

        [Test]
        public void Validator_RejectsEmptyPayload()
        {
            string reason;

            SkinPayloadValidator.IsValid(new SkinBlob { Data = new byte[0] }, out reason)
                .Should().BeFalse();

            reason.Should().Contain("empty");
        }

        [Test]
        public void Validator_RejectsNullPayload()
        {
            string reason;
            SkinPayloadValidator.IsValid(default(SkinBlob), out reason).Should().BeFalse();
        }

        [Test]
        public void Validator_RejectsPayloadOverTheByteLimit()
        {
            var oversized = new byte[SkinConstants.MaxPayloadBytes + 1];
            string reason;

            SkinPayloadValidator.IsValid(new SkinBlob { Data = oversized }, out reason)
                .Should().BeFalse();

            reason.Should().Contain("byte limit");
        }

        [Test]
        public void Validator_RejectsSomethingThatIsNotAPng()
        {
            var data = new byte[64];
            for (var i = 0; i < data.Length; i++) data[i] = 0x7F;

            string reason;
            SkinPayloadValidator.IsValid(SkinTestData.Blob(data), out reason).Should().BeFalse();
            reason.Should().Contain("not a PNG");
        }

        /// <summary>
        /// The dimension check is the one that matters: a few hundred KB of PNG can decode into
        /// gigabytes of pixels, so the byte limit alone does not bound memory.
        /// </summary>
        [Test]
        public void Validator_RejectsATextureOverThePixelLimit()
        {
            var bomb = SkinTestData.Png(SkinConstants.MaxTextureSize + 1, 64, 0x01);
            string reason;

            SkinPayloadValidator.IsValid(SkinTestData.Blob(bomb), out reason).Should().BeFalse();
            reason.Should().Contain("pixel limit");
        }

        [Test]
        public void Validator_RejectsPayloadThatDoesNotMatchTheAnnouncedHash()
        {
            var blob = new SkinBlob
            {
                Hash = SkinHash.Compute(SkinTestData.Png(0x01)),
                Data = SkinTestData.Png(0x02)
            };

            string reason;
            SkinPayloadValidator.IsValid(blob, out reason).Should().BeFalse();
            reason.Should().Contain("hash");
        }

        [Test]
        public void Hash_OfTheSameBytesIsEqualAndUsableAsADictionaryKey()
        {
            var first = SkinHash.Compute(SkinTestData.Png(0x01));
            var second = SkinHash.Compute(SkinTestData.Png(0x01));
            var other = SkinHash.Compute(SkinTestData.Png(0x02));

            first.Equals(second).Should().BeTrue();
            first.GetHashCode().Should().Be(second.GetHashCode());
            first.Equals(other).Should().BeFalse();

            var map = new System.Collections.Generic.Dictionary<SkinHash, string> { { first, "a" } };
            map.ContainsKey(second).Should().BeTrue();
            map.ContainsKey(other).Should().BeFalse();
        }

        /// <summary>
        /// Empty is the protocol's "vanilla texture", so it must survive a round trip through
        /// <c>default</c> and compare equal to a freshly computed empty hash.
        /// </summary>
        [Test]
        public void Hash_OfNothingIsEmptyAndEqualsDefault()
        {
            SkinHash.Compute(null).IsEmpty.Should().BeTrue();
            SkinHash.Compute(new byte[0]).IsEmpty.Should().BeTrue();
            SkinHash.Compute(null).Equals(default(SkinHash)).Should().BeTrue();
            default(SkinHash).Bytes.Length.Should().Be(SkinConstants.HashLength);
        }
    }
}
