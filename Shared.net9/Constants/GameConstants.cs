namespace GOILauncher.Multiplayer.Shared.Constants
{
    public static class GameConstants
    {
        private const int FrameRate = 60;
        public const float FrameTime = 1f / FrameRate;
        public const int FrameTimeMilliSeconds = (int)(FrameTime * 1000);
        public const string GameSceneName = "Mian";
        public const float DefaultCameraOrthographicSize = 5f;
        public const int DefaultFontSize = 14;
        public const float PlayerWidth = 3.4f;
        public const float PlayerHeight = 3.7f;
    }
}
