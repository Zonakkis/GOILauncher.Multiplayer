using System;
using BepInEx.Configuration;
using BepInEx.Logging;
using GOILauncher.Multiplayer.Unity;
using GOILauncher.Multiplayer.Unity.Config;

namespace GOILauncher.Multiplayer
{
    /// <summary>
    /// Owns the persisted multiplayer switch and applies its lifecycle consequences.
    /// </summary>
    public sealed class MultiplayerStateCoordinator : IMultiplayerState
    {
        public const string ConfigSection = "Multiplayer";
        public const string ConfigKey = "Enabled";
        public const bool DefaultEnabled = true;

        private readonly ConfigEntry<bool> _enabledEntry;
        private readonly ManualLogSource _logger;
        private IUnityClient _client;
        private IUnityServer _server;
        private bool _hasAppliedState;
        private bool _appliedEnabled;

        public MultiplayerStateCoordinator(ConfigFile config, ManualLogSource logger)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            _logger = logger;
            _enabledEntry = config.Bind(
                ConfigSection,
                ConfigKey,
                DefaultEnabled,
                "Whether multiplayer connections and the embedded server are enabled.");
            _enabledEntry.SettingChanged += OnSettingChanged;
        }

        public bool Enabled
        {
            get { return _enabledEntry.Value; }
            set { SetEnabled(value); }
        }

        public event Action<bool> EnabledChanged;

        /// <summary>
        /// Connects the coordinator to the Unity lifecycle adapters after the container is built.
        /// </summary>
        public void Attach(IUnityClient client, IUnityServer server)
        {
            _client = client;
            _server = server;
            ApplyState(false, true);
        }

        public void SetEnabled(bool enabled)
        {
            if (_enabledEntry.Value == enabled)
                return;

            _enabledEntry.Value = enabled;
            // ConfigEntry normally raises SettingChanged synchronously. Applying once more keeps
            // the transition correct for alternate BepInEx config implementations as well.
            ApplyState(true, false);
        }

        private void OnSettingChanged(object sender, EventArgs args)
        {
            ApplyState(true, false);
        }

        private void ApplyState(bool notify, bool force)
        {
            bool enabled = _enabledEntry.Value;
            bool changed = !_hasAppliedState || _appliedEnabled != enabled;
            if (!changed && !force)
                return;

            _hasAppliedState = true;
            _appliedEnabled = enabled;

            if (!enabled)
                DisableServices();

            if (notify && changed)
                NotifyEnabledChanged(enabled);
        }

        private void DisableServices()
        {
            if (_client != null)
            {
                try
                {
                    _client.Disconnect();
                }
                catch (Exception ex)
                {
                    LogError("Failed to disconnect the multiplayer client while disabling the feature.", ex);
                }
            }

            if (_server != null)
            {
                try
                {
                    _server.Stop();
                }
                catch (Exception ex)
                {
                    LogError("Failed to stop the embedded multiplayer server while disabling the feature.", ex);
                }
            }
        }

        private void NotifyEnabledChanged(bool enabled)
        {
            Action<bool> handlers = EnabledChanged;
            if (handlers == null)
                return;

            foreach (Action<bool> handler in handlers.GetInvocationList())
            {
                try
                {
                    handler(enabled);
                }
                catch (Exception ex)
                {
                    LogError("A multiplayer state listener failed.", ex);
                }
            }
        }

        private void LogError(string message, Exception exception)
        {
            if (_logger != null)
                _logger.LogError(message + " " + exception);
        }
    }
}
