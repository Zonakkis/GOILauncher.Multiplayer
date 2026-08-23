using System;
using System.Globalization;
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
    /// <para>
    /// 连接地址和端口这三项存的是**默认值**：客户端页 / 服务端页拿它们填输入框初值，玩家在那儿
    /// 临时改成别的地址只影响那一次连接，不回写这里。所以这里的值只有设置页会写。
    /// </para>
    /// </remarks>
    public sealed class MultiplayerSettings : IMultiplayerState
    {
        public const string EnabledKey = "Multiplayer.Enabled";
        public const bool DefaultEnabled = true;

        public const string ClientHostKey = "Multiplayer.Client.DefaultHost";
        public const string DefaultClientHost = "127.0.0.1";

        public const string ClientPortKey = "Multiplayer.Client.DefaultPort";
        public const int DefaultClientPort = 9027;

        public const string ServerPortKey = "Multiplayer.Server.DefaultPort";
        public const int DefaultServerPort = 9027;

        public const int MinPort = 1;
        public const int MaxPort = 65535;

        private readonly ISettingsStore _store;
        private readonly ILogger<MultiplayerSettings> _logger;
        private bool _enabled;
        private string _clientHost;
        private int _clientPort;
        private int _serverPort;

        public MultiplayerSettings(ISettingsStore store, ILogger<MultiplayerSettings> logger)
        {
            if (store == null)
                throw new ArgumentNullException(nameof(store));

            _store = store;
            _logger = logger;
            _enabled = ReadBool(EnabledKey, DefaultEnabled);
            _clientHost = ReadHost(ClientHostKey, DefaultClientHost);
            _clientPort = ReadPort(ClientPortKey, DefaultClientPort);
            _serverPort = ReadPort(ServerPortKey, DefaultServerPort);
        }

        public event Action<bool> EnabledChanged;

        public event Action<string> ClientHostChanged;

        public event Action<int> ClientPortChanged;

        public event Action<int> ServerPortChanged;

        public bool Enabled
        {
            get { return _enabled; }
            set { SetEnabled(value); }
        }

        /// <summary>The host the client page pre-fills. Never blank.</summary>
        public string ClientHost
        {
            get { return _clientHost; }
        }

        /// <summary>The port the client page pre-fills. Always within [<see cref="MinPort"/>, <see cref="MaxPort"/>].</summary>
        public int ClientPort
        {
            get { return _clientPort; }
        }

        /// <summary>The listen port the server page pre-fills. Always within [<see cref="MinPort"/>, <see cref="MaxPort"/>].</summary>
        public int ServerPort
        {
            get { return _serverPort; }
        }

        /// <summary>
        /// The one range check for a port, shared by the read side here and by the input validation in
        /// the UI, so a value this class would reject on load is also one the UI refuses to write.
        /// </summary>
        public static bool IsValidPort(int port)
        {
            return port >= MinPort && port <= MaxPort;
        }

        public void SetEnabled(bool enabled)
        {
            if (_enabled == enabled)
                return;

            _enabled = enabled;
            _store.Write(EnabledKey, enabled ? "true" : "false");
            Raise(EnabledChanged, enabled);
        }

        /// <summary>
        /// Sets the host the client page pre-fills. The value is trimmed, and a blank one falls back to
        /// <see cref="DefaultClientHost"/> so that clearing the field in the UI reads as "back to the
        /// default" rather than storing a host nothing can connect to.
        /// </summary>
        public void SetClientHost(string host)
        {
            string trimmed = host == null ? string.Empty : host.Trim();
            if (trimmed.Length == 0)
                trimmed = DefaultClientHost;

            if (_clientHost == trimmed)
                return;

            _clientHost = trimmed;
            _store.Write(ClientHostKey, trimmed);
            Raise(ClientHostChanged, trimmed);
        }

        /// <summary>
        /// Sets the port the client page pre-fills.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="port"/> is outside [<see cref="MinPort"/>, <see cref="MaxPort"/>]. Validating
        /// what the player typed belongs to the UI, so reaching this is a bug rather than bad input.
        /// </exception>
        public void SetClientPort(int port)
        {
            RequireValidPort(port, nameof(port));

            if (_clientPort == port)
                return;

            _clientPort = port;
            _store.Write(ClientPortKey, ToRaw(port));
            Raise(ClientPortChanged, port);
        }

        /// <summary>
        /// Sets the listen port the server page pre-fills.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="port"/> is outside [<see cref="MinPort"/>, <see cref="MaxPort"/>].
        /// </exception>
        public void SetServerPort(int port)
        {
            RequireValidPort(port, nameof(port));

            if (_serverPort == port)
                return;

            _serverPort = port;
            _store.Write(ServerPortKey, ToRaw(port));
            Raise(ServerPortChanged, port);
        }

        private static void RequireValidPort(int port, string parameterName)
        {
            if (!IsValidPort(port))
                throw new ArgumentOutOfRangeException(parameterName, port, $"A port must be between {MinPort} and {MaxPort}.");
        }

        // The file is written by this class, so it must not follow the machine's regional settings:
        // a config saved under one locale has to read back the same under any other.
        private static string ToRaw(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
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

        private string ReadHost(string key, string defaultValue)
        {
            string raw;
            if (!_store.TryRead(key, out raw) || raw == null)
                return defaultValue;

            string trimmed = raw.Trim();
            return trimmed.Length == 0 ? defaultValue : trimmed;
        }

        private int ReadPort(string key, int defaultValue)
        {
            string raw;
            if (!_store.TryRead(key, out raw) || string.IsNullOrEmpty(raw))
                return defaultValue;

            int parsed;
            if (int.TryParse(raw.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) && IsValidPort(parsed))
                return parsed;

            _logger.Warn($"Setting '{key}' holds an unrecognised value '{raw}'; using {defaultValue}.");
            return defaultValue;
        }

        private void Raise<T>(Action<T> handlers, T value)
        {
            if (handlers == null)
                return;

            // One faulty listener must not stop the others from seeing the change.
            foreach (Action<T> handler in handlers.GetInvocationList())
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
