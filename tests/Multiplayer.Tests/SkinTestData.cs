using GOILauncher.Multiplayer.Core.Data.Constants;
using GOILauncher.Multiplayer.Core.Data.Models;

namespace GOILauncher.Multiplayer.Tests
{
    /// <summary>
    /// Builds skin payloads for tests. Only the PNG header is real: nothing outside the Unity
    /// layer decodes these bytes, and the test host has no Unity runtime to decode them with.
    /// Everything under test reads the header (for the size limit) and the hash, both of which
    /// these bytes carry honestly.
    /// </summary>
    internal static class SkinTestData
    {
        private static readonly byte[] Signature =
        {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A
        };

        /// <summary>A 64x64 PNG whose body is filled with <paramref name="fill"/>, so different
        /// fills produce different hashes while staying within every limit.</summary>
        public static byte[] Png(byte fill)
        {
            return Png(64, 64, fill);
        }

        public static byte[] Png(int width, int height, byte fill)
        {
            return Png(width, height, fill, 64);
        }

        public static byte[] Png(int width, int height, byte fill, int bodyLength)
        {
            var data = new byte[24 + bodyLength];
            Signature.CopyTo(data, 0);
            WriteBigEndian(data, 8, 13); // IHDR is 13 bytes of chunk data
            data[12] = (byte)'I';
            data[13] = (byte)'H';
            data[14] = (byte)'D';
            data[15] = (byte)'R';
            WriteBigEndian(data, 16, width);
            WriteBigEndian(data, 20, height);
            for (var i = 24; i < data.Length; i++)
            {
                data[i] = fill;
            }
            return data;
        }

        public static SkinBlob Blob(byte[] data)
        {
            return new SkinBlob { Hash = SkinHash.Compute(data), Data = data };
        }

        /// <summary>A pot manifest naming <paramref name="data"/> as its texture.</summary>
        public static SkinState State(byte[] data)
        {
            return State(data, 0f);
        }

        public static SkinState State(byte[] data, float goldness)
        {
            return new SkinState
            {
                Slot = SkinConstants.PotSlot,
                Hash = SkinHash.Compute(data),
                Goldness = goldness
            };
        }

        /// <summary>A pot manifest with no texture: an empty hash plus a goldness that still counts.</summary>
        public static SkinState Vanilla(float goldness)
        {
            return new SkinState { Slot = SkinConstants.PotSlot, Goldness = goldness };
        }

        private static void WriteBigEndian(byte[] data, int offset, int value)
        {
            data[offset] = (byte)(value >> 24);
            data[offset + 1] = (byte)(value >> 16);
            data[offset + 2] = (byte)(value >> 8);
            data[offset + 3] = (byte)value;
        }
    }
}
