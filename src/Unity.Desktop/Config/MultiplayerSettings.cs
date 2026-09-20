using System;
using BepInEx.Configuration;

namespace GOILauncher.Multiplayer.UI.Config
{
    /// <summary>
    /// 联机设置的唯一所有者，存在宿主的配置文件里。怎么存是宿主这一层的事：core 不认识本类，
    /// 键名、类型、默认值和解析容错也都交给 BepInEx 的 ConfigFile，这里只补两条它不管的规矩——
    /// 端口的合法范围，和"清空主机名等于回到默认值"。
    /// </summary>
    /// <remarks>
    /// PlayerName / ClientHost / ClientPort / ServerPort 存的是**初值**：设置页是唯一写入方，
    /// 客户端页和服务端页只拿它们填输入框；玩家在那两页临时改成别的值只影响那一次连接，不回写。
    /// <para>
    /// HideServerPage 是 UI 可见性，不是联机开关：它只决定主面板上出不出现"服务端"页签，内嵌服务端
    /// 照样加载。设置页里的"服务端设置"块不受它影响——那份默认端口是给页签用的，两块各有各的去留理由。
    /// </para>
    /// <para>
    /// Enabled 是联机开关落下来的那一份：唯一真值是 MultiplayerUnityCore.IsLoaded，宿主在每次加载与
    /// 销毁之后回写它，所以它既回答"现在开着没有"，也回答"下次启动要不要自动加载"。联机现在要么已经
    /// 加载、要么已经销毁，没有"加载着但停用"这第三种状态，两边不会各说一套。
    /// </para>
    /// </remarks>
    public sealed class MultiplayerSettings
    {
        private const string Section = "Multiplayer";

        public const bool DefaultEnabled = true;
        public const string DefaultPlayerName = "";
        public const string DefaultClientHost = "127.0.0.1";
        public const int DefaultClientPort = 9027;
        public const int DefaultServerPort = 9027;
        public const bool DefaultHideServerPage = true;

        public const int MinPort = 1;
        public const int MaxPort = 65535;

        private readonly ConfigFile _config;
        private readonly ConfigEntry<bool> _enabled;
        private readonly ConfigEntry<string> _playerName;
        private readonly ConfigEntry<string> _clientHost;
        private readonly ConfigEntry<int> _clientPort;
        private readonly ConfigEntry<int> _serverPort;
        private readonly ConfigEntry<bool> _hideServerPage;

        public MultiplayerSettings(ConfigFile config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            _config = config;
            _enabled = config.Bind(Section, "Enabled", DefaultEnabled,
                "联机开关。关掉即销毁整套联机模块，下次启动按这里自动加载；游戏里随时可以重新开启。");
            _playerName = config.Bind(Section, "PlayerName", DefaultPlayerName,
                "客户端页名字输入框的初值。留空是合法的还没填，连接时会被客户端页拦下。");
            _clientHost = config.Bind(Section, "ClientHost", DefaultClientHost,
                "客户端页服务器地址输入框的初值。");
            _clientPort = config.Bind(Section, "ClientPort", DefaultClientPort,
                "客户端页端口输入框的初值。");
            _serverPort = config.Bind(Section, "ServerPort", DefaultServerPort,
                "服务端页启动端口输入框的初值。");
            _hideServerPage = config.Bind(Section, "HideServerPage", DefaultHideServerPage,
                "隐藏“服务端”页签。只影响主面板上看不看得到这个页签，内嵌服务端仍然照常加载。");
        }

        public event Action<string> PlayerNameChanged;

        public event Action<string> ClientHostChanged;

        public event Action<int> ClientPortChanged;

        public event Action<int> ServerPortChanged;

        /// <summary>
        /// 联机开关。当前有没有加载以 MultiplayerUnityCore.IsLoaded 为准，宿主在每次加载与销毁后回写这里。
        /// </summary>
        public bool Enabled
        {
            get { return _enabled.Value; }
        }

        /// <summary>
        /// 客户端页预填的名字。可以为空：空是"还没填"，连接时由客户端页拒绝，这里不做兜底默认名。
        /// </summary>
        public string PlayerName
        {
            get { return TrimOrEmpty(_playerName.Value); }
        }

        /// <summary>客户端页预填的主机。永不为空，空白一律归一化成默认值。</summary>
        public string ClientHost
        {
            get { return NormalizeHost(_clientHost.Value); }
        }

        public int ClientPort
        {
            get { return NormalizePort(_clientPort.Value, DefaultClientPort); }
        }

        public int ServerPort
        {
            get { return NormalizePort(_serverPort.Value, DefaultServerPort); }
        }

        /// <summary>主面板上不显示"服务端"页签。默认隐藏：开服不是给普通玩家用的功能，但也不把它从模块里拆掉。</summary>
        public bool HideServerPage
        {
            get { return _hideServerPage.Value; }
        }

        /// <summary>
        /// 端口的唯一判据，读侧和 UI 的输入校验共用它，所以 UI 不会写进一个下次加载要被判为非法的值。
        /// </summary>
        public static bool IsValidPort(int port)
        {
            return port >= MinPort && port <= MaxPort;
        }

        public void SetEnabled(bool enabled)
        {
            if (_enabled.Value == enabled)
                return;

            _enabled.Value = enabled;
            Save();
        }

        /// <summary>只 trim：空名字是合法状态，不能在这里被归一化成某个默认名。</summary>
        public void SetPlayerName(string name)
        {
            string value = TrimOrEmpty(name);
            if (_playerName.Value == value)
                return;

            _playerName.Value = value;
            Save();
            PlayerNameChanged?.Invoke(value);
        }

        /// <summary>空白按"清空即恢复默认"处理，免得存下一个连不上去的主机。</summary>
        public void SetClientHost(string host)
        {
            string value = NormalizeHost(host);
            if (_clientHost.Value == value)
                return;

            _clientHost.Value = value;
            Save();
            ClientHostChanged?.Invoke(value);
        }

        /// <summary>
        /// 客户端页的默认端口。越界直接抛：校验玩家输入是 UI 的事，走到这里就是 bug。
        /// </summary>
        public void SetClientPort(int port)
        {
            RequireValidPort(port);
            if (_clientPort.Value == port)
                return;

            _clientPort.Value = port;
            Save();
            ClientPortChanged?.Invoke(port);
        }

        /// <summary>
        /// 页签可见性。这里只落盘，改不改 UI 由宿主决定——设置页不直接去动另一个页面的按钮。
        /// </summary>
        public void SetHideServerPage(bool hideServerPage)
        {
            if (_hideServerPage.Value == hideServerPage)
                return;

            _hideServerPage.Value = hideServerPage;
            Save();
        }

        public void SetServerPort(int port)
        {
            RequireValidPort(port);
            if (_serverPort.Value == port)
                return;

            _serverPort.Value = port;
            Save();
            ServerPortChanged?.Invoke(port);
        }

        // ConfigFile 只在 Value 被赋值时标脏。这里显式存一次，让"设置页提交"和"文件已写"
        // 是同一件事：崩溃不会丢掉玩家已经改过的设置，也不用等 BepInEx 的自动保存时机。
        private void Save()
        {
            _config.Save();
        }

        private static void RequireValidPort(int port)
        {
            if (!IsValidPort(port))
                throw new ArgumentOutOfRangeException("port", port,
                    string.Format("A port must be between {0} and {1}.", MinPort, MaxPort));
        }

        private static string TrimOrEmpty(string value)
        {
            return value == null ? string.Empty : value.Trim();
        }

        private static string NormalizeHost(string host)
        {
            string trimmed = TrimOrEmpty(host);
            return trimmed.Length == 0 ? DefaultClientHost : trimmed;
        }

        // 手改配置文件填进来越界的端口按"没配过"处理。ConfigFile 只管能不能解析成 int，
        // 范围是这套设置自己的概念，所以读的时候再过一道，判据和 UI 校验共用。
        private static int NormalizePort(int port, int defaultValue)
        {
            return IsValidPort(port) ? port : defaultValue;
        }
    }
}
