using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GOILauncher.Multiplayer.Core.Log
{
    public class NLogConfiguration
    {
        public static void Configure()
        {
            var config = new LoggingConfiguration();
            var consoleTarget = new ConsoleTarget("console")
            {
                Layout = @"${date:format=yyyy-MM-dd HH\:mm\:ss}|${level:uppercase=true}|${logger:shortName=true}|${message}"
            };
            config.AddTarget(consoleTarget);

            config.AddRule(LogLevel.Info, LogLevel.Fatal, consoleTarget, "*");
            LogManager.Configuration = config;
        }
    }
}
