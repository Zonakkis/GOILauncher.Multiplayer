using System;
using GOILauncher.Multiplayer.Core.Log;

namespace GOILauncher.Multiplayer.Unity.Config
{
    /// <summary>
    /// The persisted multiplayer settings: what each setting is called, its type, its default, and who
    /// is told when it changes. Storage sits behind <see cref="ISettingsStore"/>, so this class is the
    /// same on every platform.
    /// </summary>
    /// <remarks>
    /// Changing a setting here does nothing to the running services.
    /// <see cref="MultiplayerLifecycleController"/> owns those consequences, which keeps "what the
    /// player chose" separate from "what that does".
    /// </remarks>
    public sealed class MultiplayerSettings : IMultiplayerState
    {
        public const string EnabledKey = "Multiplayer.Enabled";
        public const bool DefaultEnabled = true;

        private readonly ISettingsStore _store;
        private readonly ILogger<MultiplayerSettings> _logger;
        private bool _enabled;

        public MultiplayerSettings(ISettingsStore store, ILogger<MultiplayerSettings> logger)
        {
            if (store == null)
                throw new ArgumentNullException(nameof(store));

            _store = store;
            _logger = logger;
            _enabled = ReadBool(EnabledKey, DefaultEnabled);
        }

        public event Action<bool> EnabledChanged;

        public bool Enabled
        {
            get { return _enabled; }
            set { SetEnabled(value); }
        }

        public void SetEnabled(bool enabled)
        {
            if (_enabled == enabled)
                return;

            _enabled = enabled;
            _store.Write(EnabledKey, enabled ? "true" : "false");
            Raise(EnabledChanged, enabled);
        }

        private bool ReadBool(string key, bool defaultValue)
        {
            string raw;
            if (!_store.TryRead(key, out raw) || string.IsNullOrEmpty(raw))
                return defaultValue;

            bool parsed;
            if (bool.TryParse(raw, out parsed))
                return parsed;

            // Tolerate the other shapes someone editing the file by hand is likely to write.
            if (raw == "1")
                return true;
            if (raw == "0")
                return false;

            _logger.Warn($"Setting '{key}' holds an unrecognised value '{raw}'; using {defaultValue}.");
            return defaultValue;
        }

        private void Raise(Action<bool> handlers, bool value)
        {
            if (handlers == null)
                return;

            // One faulty listener must not stop the others from seeing the change.
            foreach (Action<bool> handler in handlers.GetInvocationList())
            {
                try
                {
                    handler(value);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "A multiplayer settings listener failed.");
                }
            }
        }
    }
}
