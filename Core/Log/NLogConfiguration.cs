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
            var consoleTarget = new ConsoleTarget("console");
            config.AddTarget(consoleTarget);

            config.AddRule(LogLevel.Info, LogLevel.Fatal, consoleTarget, "*");
        }
    }
}
