using Autofac;
using BepInEx;
using BepInEx.Logging;
using GOILauncher.Multiplayer.UI;
using GOILauncher.Multiplayer.UI.Components;
using GOILauncher.Multiplayer.UI.Pages;
using GOILauncher.Multiplayer.UI.ScrollView.Message;
using GOILauncher.Multiplayer.UI.ScrollView.Player;
using GOILauncher.Multiplayer.UI.Theme;
using GOILauncher.Multiplayer.Unity;
using NLog;
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

    private MultiplayerUI _multiplayerUI;
    private MultiplayerStateCoordinator _multiplayerState;
    private ChatHudUI _chatHudUI;
    private PlayerListUI _playerListOverlayUI;
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

    private void OnInitialized()
    {
        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} is loading...");
        var container = MultiplayerUnityCore.Initialize(Configure);

        _multiplayerState = container.Resolve<MultiplayerStateCoordinator>();
        _multiplayerState.Attach(
            container.Resolve<IUnityClient>(),
            container.Resolve<IUnityServer>());
        _multiplayerState.EnabledChanged += OnMultiplayerEnabledChanged;

        Theme = container.Resolve<ITheme>();
        UIBase = container.Resolve<UIBase>();
        _multiplayerUI = container.Resolve<MultiplayerUI>();
        _chatHudUI = container.Resolve<ChatHudUI>();
        _playerListOverlayUI = container.Resolve<PlayerListUI>();
        _chatHudUI.ActiveModeChanged += OnChatActiveModeChanged;
        _multiplayerUI.SetActive(false);
        _playerListOverlayUI.SetActive(false);
        ApplyMultiplayerUiState(_multiplayerState.Enabled);
        ApplyCursorState();
        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void Configure(ContainerBuilder builder)
    {
        builder.Register(_ => new BepInExTarget(Logger)
        {
            Layout = @"${date:format=yyyy-MM-dd HH\:mm\:ss}|${level:uppercase=true}|${logger:shortName=true}|${message}${onexception:inner=${newline}${exception:format=tostring}}"
        })
           .As<Target>()
           .SingleInstance();
        builder.RegisterType<DarkTheme>()
        .As<ITheme>()
        .SingleInstance();
        builder
        .Register(_ => UniversalUI.RegisterUI<ResponsiveUIBase>(MyPluginInfo.PLUGIN_GUID, null))
        .As<UIBase>()
        .SingleInstance();
        builder.RegisterType<Toast>()
        .AsSelf()
        .SingleInstance();
        builder.Register(_ => new MultiplayerStateCoordinator(Config, Logger))
        .AsSelf()
        .As<IMultiplayerState>()
        .SingleInstance();
        builder.RegisterType<ClientPage>()
        .AsSelf()
        .SingleInstance();
        builder.RegisterType<ServerPage>()
        .AsSelf()
        .SingleInstance();
        builder.RegisterType<SettingsPage>()
        .AsSelf()
        .SingleInstance();
        builder.RegisterType<MultiplayerUI>()
        .AsSelf()
        .SingleInstance();
        builder.RegisterType<ChatHudUI>()
        .AsSelf()
        .SingleInstance();
        builder.RegisterType<MessageHandler>()
        .AsSelf()
        .SingleInstance();
        builder.RegisterType<PlayerListHandler>()
        .AsSelf()
        .SingleInstance();
        builder.RegisterType<PlayerListUI>()
        .AsSelf()
        .SingleInstance();
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

        if (Input.GetKeyDown(KeyCode.F2))
        {
            _multiplayerUI.SetActive(!_multiplayerUI.Enabled);
            ApplyCursorState();
        }

        if (_multiplayerState == null || !_multiplayerState.Enabled)
        {
            if (_playerListOverlayUI.Enabled)
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

    private void OnChatActiveModeChanged(bool active)
    {
        ApplyCursorState();
    }

    private void OnMultiplayerEnabledChanged(bool enabled)
    {
        ApplyMultiplayerUiState(enabled);
        ApplyCursorState();
    }

    private void ApplyMultiplayerUiState(bool enabled)
    {
        if (_chatHudUI != null)
            _chatHudUI.SetActive(enabled);

        if (!enabled && _playerListOverlayUI != null && _playerListOverlayUI.Enabled)
            _playerListOverlayUI.SetActive(false);
    }

    private void ApplyCursorState()
    {
        bool shouldUnlockCursor = ShouldUnlockCursor() || IsOtherUniverseUiShowing();

        if (_hasAppliedCursorState && _lastCursorUnlockState == shouldUnlockCursor)
            return;

        _hasAppliedCursorState = true;
        _lastCursorUnlockState = shouldUnlockCursor;
        ConfigManager.Force_Unlock_Mouse = shouldUnlockCursor;
        UpdateCursorControlMethod?.Invoke(null, null);
    }

    private bool ShouldUnlockCursor()
    {
        return (_multiplayerUI != null && _multiplayerUI.Enabled)
            || (_playerListOverlayUI != null && _playerListOverlayUI.Enabled)
            || (_chatHudUI != null && _chatHudUI.IsActiveMode);
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
