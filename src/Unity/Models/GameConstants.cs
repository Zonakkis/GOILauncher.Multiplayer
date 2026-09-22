namespace GOILauncher.Multiplayer.Unity.Models
{
    public static class GameConstants
    {
        public const float DefaultCameraOrthographicSize = 5f;
        public const int DefaultFontSize = 15;
        public const float DefaultScreenWidth = 1920f;
        public const float DefaultScreenDpi = 120f;
        public const float PlayerWidth = 3.4f;
        public const float PlayerHeight = 3.7f;
        
        /// <summary>
        /// 罐子那个 MeshRenderer 相对 <c>Player</c> 根的路径。皮肤 Mod 换的就是它材质上的
        /// <c>mainTexture</c>，金度也在同一个材质上（见 docs/agent/game-runtime.md）。
        /// </summary>
        public const string PotMeshPath = "Pot/Mesh";

        /// <summary>
        /// 玩家身体（社区称 Diogenes）那个 SkinnedMeshRenderer 相对 <c>Player</c> 根的路径。
        /// 皮肤 Mod 同样只换它材质上的 <c>mainTexture</c>；身体材质没有 <c>_Goldness</c>。
        /// </summary>
        public const string BodyMeshPath = "dude/Body";

        /// <summary>罐子材质上控制金罐的属性名：黑罐 0，金罐 1。身体材质没有这个属性。</summary>
        public const string GoldnessProperty = "_Goldness";

        /// <summary>
        /// 皮肤槽位 → 它对应的 MeshRenderer 相对 <c>Player</c> 根的路径。
        /// 读本地、贴远端、找原版基线三处共用这一张表，加部件只在这里加一行。
        /// 返回 null 表示这个 build 不认识这个槽位。
        /// </summary>
        public static string SkinMeshPath(byte slot)
        {
            switch (slot)
            {
                case Core.Data.Constants.SkinConstants.PotSlot: return PotMeshPath;
                case Core.Data.Constants.SkinConstants.BodySlot: return BodyMeshPath;
                default: return null;
            }
        }
    }
}