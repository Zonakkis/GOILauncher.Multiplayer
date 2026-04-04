using BepInEx.Logging;
using NLog;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GOILauncher.Multiplayer
{
    [Target("BepInExTarget")]
    public class BepInExTarget : TargetWithLayout
    {
        private ManualLogSource _logger;

        public BepInExTarget(ManualLogSource logger)
        {
            Name = "BepInExTarget";
            _logger = logger;
        }

        protected override void Write(LogEventInfo logEvent)
        {
            string logMessage = Layout.Render(logEvent);
            switch (logEvent.Level.Name)
            {
                case "Trace":
                case "Debug":
                    _logger.LogDebug(logMessage);
                    break;
                case "Info":
                    _logger.LogInfo(logMessage);
                    break;
                case "Warn":
                    _logger.LogWarning(logMessage);
                    break;
                case "Error":
                    _logger.LogError(logMessage);
                    break;
                case "Fatal":
                    _logger.LogFatal(logMessage);
                    break;
                default:
                    _logger.LogInfo(logMessage);
                    break;
            }
        }
    }
}
