using Autofac;
using BepInEx;
using BepInEx.Logging;
using GOILauncher.Multiplayer.UI;
using GOILauncher.Multiplayer.UI.Pages;
using GOILauncher.Multiplayer.UI.ScrollView.Message;
using GOILauncher.Multiplayer.UI.Theme;
using GOILauncher.Multiplayer.Unity;
using NLog;
using NLog.Targets;
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
    private static readonly MethodInfo UpdateCursorControlMethod = typeof(CursorUnlocker).GetMethod("UpdateCursorControl", BindingFlags.Static | BindingFlags.NonPublic);

    private MultiplayerUI _multiplayerUI;
    private ChatHudUI _chatHudUI;
    private PlayerListUI _playerListOverlayUI;

    private void Awake()
    {
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

        Theme = container.Resolve<ITheme>();
        _multiplayerUI = container.Resolve<MultiplayerUI>();
        _chatHudUI = container.Resolve<ChatHudUI>();
        _playerListOverlayUI = container.Resolve<PlayerListUI>();
        _chatHudUI.ActiveModeChanged += OnChatActiveModeChanged;
        _multiplayerUI.SetActive(false);
        _playerListOverlayUI.SetActive(false);
        ApplyCursorState();
        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void Configure(ContainerBuilder builder)
    {
        builder.Register(_ => new BepInExTarget(Logger)
        {
            Layout = @"${date:format=yyyy-MM-dd HH\:mm\:ss}|${logger:shortName=true}|${message}"
        })
           .As<Target>()
           .SingleInstance();
        builder.RegisterType<DarkTheme>()
        .As<ITheme>()
        .SingleInstance();
        builder
        .Register(_ => UniversalUI.RegisterUI(MyPluginInfo.PLUGIN_GUID, null))
        .SingleInstance();
        builder.RegisterType<Toast>()
        .AsSelf()
        .SingleInstance();
        builder.RegisterType<ClientPage>()
        .AsSelf()
        .SingleInstance();
        builder.RegisterType<ServerPage>()
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

        if (Input.GetKeyDown(KeyCode.F1))
        {
            _multiplayerUI.SetActive(!_multiplayerUI.Enabled);
            ApplyCursorState();
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

    }

    private void OnChatActiveModeChanged(bool active)
    {
        ApplyCursorState();
    }

    private void ApplyCursorState()
    {
        ConfigManager.Force_Unlock_Mouse = ShouldUnlockCursor();
        UpdateCursorControlMethod?.Invoke(null, null);
    }

    private bool ShouldUnlockCursor()
    {
        return (_multiplayerUI != null && _multiplayerUI.Enabled)
            || (_playerListOverlayUI != null && _playerListOverlayUI.Enabled)
            || (_chatHudUI != null && _chatHudUI.IsActiveMode);
    }
}
