namespace GOILauncher.Multiplayer.Core.Data.Constants
{
    public static class SkinConstants
    {
        /// <summary>SHA-256 的字节数。皮肤贴图用内容哈希当线上标识。</summary>
        public const int HashLength = 32;

        /// <summary>Pot 的皮肤槽位。当前只有这一个槽位在用。</summary>
        public const byte PotSlot = 0;

        /// <summary>
        /// 单张贴图的 PNG 字节上限。传输层撑得住远大于此（可靠分片包的上限在几十 MB），
        /// 这个数是给服务端"每人一份"的缓存定个天花板，不是传输限制。
        /// </summary>
        public const int MaxPayloadBytes = 4 * 1024 * 1024;

        /// <summary>
        /// 单边最大像素数。字节上限拦不住解压炸弹——一张纯色的 4 MB PNG 能解出几十亿像素，
        /// 所以尺寸必须在 <c>LoadImage</c> 之前先从 PNG 的 IHDR 里读出来判掉，解码完再查已经晚了。
        /// </summary>
        public const int MaxTextureSize = 4096;
    }
}
