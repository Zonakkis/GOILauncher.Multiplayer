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
            var config = LogManager.Configuration ?? new LoggingConfiguration();
            if (string.IsNullOrEmpty(target.Name))
                target.Name = target.GetType().Name;

            Target configuredTarget = config.FindTargetByName(target.Name);
            if (configuredTarget == null)
            {
                config.AddTarget(target);
                configuredTarget = target;
            }

            if (!HasRuleForTarget(config, configuredTarget))
                config.AddRuleForAllLevels(configuredTarget);

            LogManager.Configuration = config;
            LogManager.ReconfigExistingLoggers();
        }

        private bool HasRuleForTarget(LoggingConfiguration config, Target target)
        {
            foreach (LoggingRule rule in config.LoggingRules)
                foreach (Target ruleTarget in rule.Targets)
                {
                    if (ReferenceEquals(ruleTarget, target) || ruleTarget.Name == target.Name)
                        return true;
                }

            return false;
        }
    }
}
