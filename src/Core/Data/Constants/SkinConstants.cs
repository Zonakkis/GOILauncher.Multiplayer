namespace GOILauncher.Multiplayer.Core.Data.Constants
{
    public static class SkinConstants
    {
        /// <summary>SHA-256 的字节数。皮肤贴图用内容哈希当线上标识。</summary>
        public const int HashLength = 32;

        /// <summary>罐子（<c>Pot/Mesh</c>）的皮肤槽位。</summary>
        public const byte PotSlot = 0;

        /// <summary>玩家身体（<c>dude/Body</c>，社区称 Diogenes）的皮肤槽位。</summary>
        public const byte BodySlot = 1;

        /// <summary>
        /// 本 build 认得的所有皮肤槽位。每个槽位独立宣告、独立缓存、独立下发。
        /// 加部件就往这里加一个：服务端只转发这里列出的槽位（见 <see cref="IsKnownSlot"/>），
        /// 收发两端遍历它来处理"一名玩家的每个槽位"。
        /// </summary>
        public static readonly byte[] Slots = { PotSlot, BodySlot };

        /// <summary>
        /// 这个槽位这个 build 认识吗。收到不认识的槽位说明对面比本端新，转发出去也没人渲染得了。
        /// </summary>
        public static bool IsKnownSlot(byte slot)
        {
            for (var i = 0; i < Slots.Length; i++)
            {
                if (Slots[i] == slot)
                {
                    return true;
                }
            }
            return false;
        }

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
