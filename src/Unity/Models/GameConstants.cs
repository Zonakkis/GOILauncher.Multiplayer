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
        /// <c>mainTexture</c>，金度也在同一个材质上（见 docs/game-runtime.md）。
        /// </summary>
        public const string PotMeshPath = "Pot/Mesh";

        /// <summary>罐子材质上控制金罐的属性名：黑罐 0，金罐 1。</summary>
        public const string GoldnessProperty = "_Goldness";
    }
}