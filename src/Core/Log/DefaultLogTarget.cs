using NLog.Targets;

namespace GOILauncher.Multiplayer.Core.Log
{
    /// <summary>
    /// The module's fallback log sink: a console target with the shared line layout.
    /// RegisterMultiplayerCore registers this, and (Unity) MultiplayerOptions.LogTarget defaults
    /// to it, so the layout string lives in exactly one place instead of being copy-pasted.
    /// </summary>
    public static class DefaultLogTarget
    {
        public const string LayoutText =
            @"${date:format=yyyy-MM-dd HH\:mm\:ss}|${level:uppercase=true}|${logger:shortName=true}|${message}${onexception:inner=${newline}${exception:format=tostring}}";

        public static Target Create() => new ConsoleTarget { Layout = LayoutText };
    }
}
