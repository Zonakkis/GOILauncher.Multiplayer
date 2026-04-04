using NLog;
using NLog.Config;
using NLog.Targets;
namespace GOILauncher.Multiplayer.Core
{
    public class CoreManager
    {
        public CoreManager(Target target)
        {
            ConfigureNLog(target);
        }

        private void ConfigureNLog(Target target)
        {
            var config = new LoggingConfiguration();
            config.AddTarget(target);
            config.AddRuleForAllLevels(target.Name);
            LogManager.Configuration = config;
        }
    }
}
