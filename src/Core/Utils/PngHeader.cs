namespace GOILauncher.Multiplayer.Core.Utils
{
    /// <summary>
    /// 只读 PNG 的文件头。存在的理由是在把字节交给 <c>Texture2D.LoadImage</c> 之前就知道尺寸——
    /// 解码完再检查已经晚了，那时内存已经吃下去了。
    /// </summary>
    public static class PngHeader
    {
        // 布局：8 字节签名 + 4 字节块长度 + "IHDR" + 4 字节宽 + 4 字节高。
        private const int WidthOffset = 16;
        private const int HeightOffset = 20;
        private const int MinLength = 24;

        private static readonly byte[] Signature =
        {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A
        };

        public static bool TryReadSize(byte[] data, out int width, out int height)
        {
            width = 0;
            height = 0;

            if (data == null || data.Length < MinLength) return false;

            for (var i = 0; i < Signature.Length; i++)
            {
                if (data[i] != Signature[i]) return false;
            }

            // IHDR 按规范必须是第一个块，所以位置是固定的，不用遍历块表。
            if (data[12] != 'I' || data[13] != 'H' || data[14] != 'D' || data[15] != 'R')
                return false;

            width = ReadBigEndianInt32(data, WidthOffset);
            height = ReadBigEndianInt32(data, HeightOffset);
            return width > 0 && height > 0;
        }

        private static int ReadBigEndianInt32(byte[] data, int offset)
        {
            // PNG 是大端。最高位置 1 的尺寸按 PNG 规范本来就非法，这里会算成负数，
            // 由上面的 > 0 判掉。
            return (data[offset] << 24)
                   | (data[offset + 1] << 16)
                   | (data[offset + 2] << 8)
                   | data[offset + 3];
        }
    }
}
