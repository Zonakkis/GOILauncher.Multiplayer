using System;
using NLog;

namespace GOILauncher.Multiplayer.Shared.Configuration
{
    public static class NLogConfiguration
    {
        private static bool _isConfigured;
        public static void EnableNLog()
        {
            if (_isConfigured)
                return;
            LogManager.Setup().LoadConfiguration(builder =>
            {
                builder.ForLogger().FilterMinLevel(LogLevel.Info).WriteToConsole();
#if DEBUG
                builder.ForLogger().FilterMinLevel(LogLevel.Debug).WriteToFile($"logs/{DateTime.Now:yyyy-MM-dd}.txt");
#endif
            });
            _isConfigured = true;
        }
    }
}
