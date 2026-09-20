using BepInEx;
using BepInEx.Logging;
using GOILauncher.Multiplayer.UI;
using GOILauncher.Multiplayer.UI.Config;
using GOILauncher.Multiplayer.UI.Pages;
using GOILauncher.Multiplayer.UI.ScrollView.Message;
using GOILauncher.Multiplayer.UI.ScrollView.Player;
using GOILauncher.Multiplayer.UI.Theme;
using GOILauncher.Multiplayer.Unity;
using NLog.Targets;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UniverseLib;
using UniverseLib.Config;
using UniverseLib.Input;
using UniverseLib.UI;

namespace GOILauncher.Multiplayer;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    public static ITheme Theme { get; private set; }
    public static UIBase UIBase { get; private set; }
    // UniverseLib exposes the config flag, but keeps the immediate cursor refresh internal.
    private static readonly FieldInfo RegisteredUisField = typeof(UniversalUI).GetField("registeredUIs", BindingFlags.Static | BindingFlags.NonPublic);
    private static readonly MethodInfo UpdateCursorControlMethod = typeof(CursorUnlocker).GetMethod("UpdateCursorControl", BindingFlags.Static | BindingFlags.NonPublic);

    private MultiplayerSettings _settings;
    private Target _logTarget;
    private MultiplayerUI _multiplayerUI;
    private RoomDialogUI _roomDialogUI;
    private ClientPage _clientPage;
    private ServerPage _serverPage;
    private SettingsPage _settingsPage;
    private ChatHudUI _chatHudUI;
    private PlayerListUI _playerListOverlayUI;
    private Rigidbody2D _blockedCursorBody;
    private bool _hasAppliedCursorState;
    private bool _lastCursorUnlockState;

    private void Awake()
    {
        // Debug
        Application.runInBackground = true;

        // Plugin startup logic
        Logger = base.Logger;

        Universe.Init(1f, OnInitialized, OnLog, new UniverseLibConfig
        {
            Force_Unlock_Mouse = false,
            Disable_EventSystem_Override = false
        });
    }

    /// <summary>
    /// 手工组合 UI，不走容器。理由不是省事：这些窗口按 id 注册在 UniverseLib 的静态表里，
    /// 第二个都建不出来，所以它们的生命周期是进程级的；而联机模块随开随关，两套生命周期
    /// 放进同一个容器里只会互相牵制。页面要用的联机门面改成 Bind / Unbind 递进来。
    /// <para>
    /// 本类是唯一读 MultiplayerUnityCore 的地方：其余 UI 只认自己绑到的那个门面，
    /// 开关按下去也是委托到这里才变成加载与销毁。
    /// </para>
    /// </summary>
    private void OnInitialized()
    {
        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} is loading...");

        _settings = new MultiplayerSettings(Config);
        // NLog 的落地端由宿主提供，且整个进程只建一次：它挂在 NLog 的全局配置上，
        // 不跟着联机模块一起销毁，否则下一轮 Initialize 复用到的是个已销毁的 target。
        _logTarget = new BepInExTarget(Logger)
        {
            Layout = @"${date:format=yyyy-MM-dd HH\:mm\:ss}|${level:uppercase=true}|${logger:shortName=true}|${message}${onexception:inner=${newline}${exception:format=tostring}}"
        };

        Theme = new DarkTheme();
        UIBase = UniversalUI.RegisterUI<ResponsiveUIBase>(MyPluginInfo.PLUGIN_GUID, null);
        var toast = new Toast(UIBase);
        var messageHandler = new MessageHandler();
        var playerListHandler = new PlayerListHandler();

        _roomDialogUI = new RoomDialogUI(UIBase);
        _clientPage = new ClientPage(_settings, toast, _roomDialogUI);
        _serverPage = new ServerPage(_settings, toast);
        _settingsPage = new SettingsPage(_settings, toast);
        _multiplayerUI = new MultiplayerUI(UIBase, _clientPage, _serverPage, _settingsPage, _roomDialogUI);
        // 页签可见性存在设置里，默认就是隐藏的：面板建好先应用一次，不用等玩家去设置页碰那个勾选框。
        _multiplayerUI.SetServerPageVisible(!_settings.HideServerPage);
        _chatHudUI = new ChatHudUI(UIBase, messageHandler, _roomDialogUI);
        _playerListOverlayUI = new PlayerListUI(UIBase, playerListHandler);

        _multiplayerUI.SetActive(false);
        _playerListOverlayUI.SetActive(false);
        _chatHudUI.SetActive(false);

        _chatHudUI.ActiveModeChanged += OnChatActiveModeChanged;
        _settingsPage.LoadToggled += OnLoadToggled;
        _settingsPage.HideServerPageToggled += OnHideServerPageToggled;
        MultiplayerUnityCore.Initialized += OnCoreInitialized;
        MultiplayerUnityCore.Disposing += OnCoreDisposing;

        // 订阅在前、加载在后：加载成功时 Initialized 会立刻把各页面绑上，不用这里补一遍。
        // 这是 Enabled 唯一一次被当作输入读：决定本次启动加不加载。之后它只被写，跟着现状走。
        if (_settings.Enabled)
            MultiplayerUnityCore.Initialize(_logTarget);

        SyncLoadedState();
        ApplyCursorState();
        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void OnLoadToggled(bool load)
    {
        if (load)
            MultiplayerUnityCore.Initialize(_logTarget);
        else
            MultiplayerUnityCore.Dispose();

        SyncLoadedState();
    }

    /// <summary>
    /// 收放"服务端"页签。只是 UI：设置页已经把它落盘了，这里不碰内嵌服务端的加载与销毁。
    /// </summary>
    private void OnHideServerPageToggled(bool hide)
    {
        _multiplayerUI.SetServerPageVisible(!hide);
    }

    /// <summary>
    /// 加载状态变动后统一收尾：落盘的 Enabled、勾选框、聊天窗口，全按 MultiplayerUnityCore.IsLoaded
    /// 这个唯一真值来。启动时那次自动加载也走这里，所以配置里不会留下一个没成真的 true，
    /// 勾选框也不会停在玩家刚点下去、其实没生效的那个值上。
    /// </summary>
    private void SyncLoadedState()
    {
        bool loaded = MultiplayerUnityCore.IsLoaded;
        _settings.SetEnabled(loaded);
        _settingsPage.SetLoaded(loaded);
        ApplyMultiplayerUiState(loaded);
    }

    private void OnCoreInitialized()
    {
        var client = MultiplayerUnityCore.UnityClient;
        var server = MultiplayerUnityCore.UnityServer;
        if (client == null || server == null)
            return;

        _clientPage.Bind(client);
        _serverPage.Bind(server);
        _roomDialogUI.Bind(client);
        _chatHudUI.Bind(client);
        _playerListOverlayUI.Bind(client);
        ApplyMultiplayerUiState(true);
    }

    private void OnCoreDisposing()
    {
        // 顺序反过来会漏：拆的时候门面事件已经不会再发，页面得趁还拿得到时自己退订、归零。
        _clientPage.Unbind();
        _serverPage.Unbind();
        _roomDialogUI.Unbind();
        _chatHudUI.Unbind();
        _playerListOverlayUI.Unbind();
        ApplyMultiplayerUiState(false);
    }

    private void OnLog(string message, LogType type)
    {
        switch (type)
        {
            case LogType.Error:
            case LogType.Exception:
                Logger.LogError(message);
                break;
            case LogType.Warning:
                Logger.LogWarning(message);
                break;
            default:
                Logger.LogInfo(message);
                break;
        }
    }

    private void Update()
    {
        if (_multiplayerUI == null || _playerListOverlayUI == null)
            return;

        // F2 不看联机加没加载：关着也得能进设置页把它重新打开。
        if (Input.GetKeyDown(KeyCode.F2))
        {
            _multiplayerUI.SetActive(!_multiplayerUI.Enabled);
            ApplyCursorState();
        }

        if (!MultiplayerUnityCore.IsLoaded)
        {
            if (_playerListOverlayUI.Enabled)
                _playerListOverlayUI.SetActive(false);

            ApplyCursorState();
            return;
        }

        if ((_roomDialogUI != null && _roomDialogUI.BlocksGameplayShortcuts) || UiInputFocus.IsEditing)
        {
            _playerListOverlayUI.SetActive(false);
            ApplyCursorState();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            _playerListOverlayUI.SetActive(true);
            ApplyCursorState();
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            _playerListOverlayUI.SetActive(false);
            ApplyCursorState();
        }

        ApplyCursorState();
    }

    private void OnDisable()
    {
        ApplyCursorPhysicsState(false);
    }

    private void OnDestroy()
    {
        ApplyCursorPhysicsState(false);

        _chatHudUI.ActiveModeChanged -= OnChatActiveModeChanged;
        _settingsPage.LoadToggled -= OnLoadToggled;
        _settingsPage.HideServerPageToggled -= OnHideServerPageToggled;
        MultiplayerUnityCore.Initialized -= OnCoreInitialized;
        MultiplayerUnityCore.Disposing -= OnCoreDisposing;

        // 先退订再拆：拆的时候不该再回调进这批跟着宿主一起销毁的窗口。
        MultiplayerUnityCore.Dispose();
    }

    private void OnChatActiveModeChanged(bool active)
    {
        ApplyCursorState();
    }

    private void ApplyMultiplayerUiState(bool loaded)
    {
        if (_chatHudUI != null)
            _chatHudUI.SetActive(loaded);

        if (!loaded && _playerListOverlayUI != null && _playerListOverlayUI.Enabled)
            _playerListOverlayUI.SetActive(false);
    }

    private void ApplyCursorState()
    {
        bool shouldUnlockCursor = ShouldUnlockCursor() || IsOtherUniverseUiShowing();

        // 光标状态未变也要处理：UI 打开期间重载场景会换掉 Cursor。
        ApplyCursorPhysicsState(isActiveAndEnabled && shouldUnlockCursor);

        if (_hasAppliedCursorState && _lastCursorUnlockState == shouldUnlockCursor)
            return;

        _hasAppliedCursorState = true;
        _lastCursorUnlockState = shouldUnlockCursor;
        ConfigManager.Force_Unlock_Mouse = shouldUnlockCursor;
        UpdateCursorControlMethod?.Invoke(null, null);
    }

    private void ApplyCursorPhysicsState(bool blocked)
    {
        // 释放只依赖已记录的刚体，所以联机模块没加载、甚至宿主还没初始化完时都能安全调用。
        IGameManager gameManager = MultiplayerUnityCore.GameManager;
        GameObject cursor = blocked && gameManager != null ? gameManager.Cursor : null;
        Rigidbody2D cursorBody = cursor != null ? cursor.GetComponent<Rigidbody2D>() : null;

        if (_blockedCursorBody != null && _blockedCursorBody != cursorBody && !_blockedCursorBody.simulated)
            _blockedCursorBody.simulated = true;

        _blockedCursorBody = cursorBody;
        if (cursorBody != null && cursorBody.simulated)
            cursorBody.simulated = false;
    }

    private bool ShouldUnlockCursor()
    {
        return (_multiplayerUI != null && _multiplayerUI.Enabled)
            || (_roomDialogUI != null && _roomDialogUI.Enabled)
            || IsPlayerListMouseRequested()
            || (_chatHudUI != null && _chatHudUI.IsActiveMode);
    }

    // 玩家列表只是展示，按住 Tab 时游戏输入照常；Tab + 空格才交出鼠标，用来点行内的传送按钮。
    private bool IsPlayerListMouseRequested()
    {
        return _playerListOverlayUI != null
            && _playerListOverlayUI.Enabled
            && Input.GetKey(KeyCode.Space);
    }

    private bool IsOtherUniverseUiShowing()
    {
        if (RegisteredUisField?.GetValue(null) is not IDictionary<string, UIBase> registeredUis)
            return false;

        foreach (KeyValuePair<string, UIBase> registeredUi in registeredUis)
        {
            UIBase uiBase = registeredUi.Value;
            if (uiBase == null || ReferenceEquals(uiBase, UIBase))
                continue;

            if (uiBase.Enabled)
                return true;
        }

        return false;
    }
}
