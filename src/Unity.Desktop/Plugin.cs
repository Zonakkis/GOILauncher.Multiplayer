using Autofac;
using BepInEx;
using BepInEx.Logging;
using GOILauncher.Multiplayer.UI;
using GOILauncher.Multiplayer.UI.Theme;
using GOILauncher.Multiplayer.Unity;
using NLog;
using NLog.Targets;
using UnityEngine;
using UniverseLib;
using UniverseLib.Config;
using UniverseLib.UI;

namespace GOILauncher.Multiplayer;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    public static ITheme Theme { get; private set; }
    public static UIBase UIBase { get; private set; }
    private MultiplayerUI _multiplayerUI;
    private ChatHudUI _chatHudUI;
    private PlayerListOverlayUI _playerListOverlayUI;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;

        Universe.Init(1f, OnInitialized, OnLog, new UniverseLibConfig
        {
            Force_Unlock_Mouse = true,
            Disable_EventSystem_Override = false
        });
    }

    private void OnInitialized()
    {
        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} is loading...");
        var container = MultiplayerUnityCore.Initialize(builder =>
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
            builder.RegisterType<MultiplayerUI>()
            .AsSelf()
            .SingleInstance();
            builder.RegisterType<ChatHudUI>()
            .AsSelf()
            .SingleInstance();
            builder.RegisterType<PlayerListOverlayUI>()
            .AsSelf()
            .SingleInstance();
        });

        Theme = container.Resolve<ITheme>();
        _multiplayerUI = container.Resolve<MultiplayerUI>();
        _chatHudUI = container.Resolve<ChatHudUI>();
        _playerListOverlayUI = container.Resolve<PlayerListOverlayUI>();
        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} is loaded!");
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
        if (Input.GetKeyDown(KeyCode.F1))
            _multiplayerUI.SetActive(!_multiplayerUI.Enabled);

        if (Input.GetKeyDown(KeyCode.Tab))
            _playerListOverlayUI.SetActive(true);
        else if (Input.GetKeyUp(KeyCode.Tab))
            _playerListOverlayUI.SetActive(false);

    }
}
