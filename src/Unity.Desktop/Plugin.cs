using Autofac;
using BepInEx;
using BepInEx.Logging;
using GOILauncher.Multiplayer.UI;
using GOILauncher.Multiplayer.Unity;
using UnityEngine;
using UniverseLib;
using UniverseLib.Config;
using UniverseLib.UI;

namespace GOILauncher.Multiplayer;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    public static UIBase UIBase { get; private set; }
    private static MultiplayerUI _multiplayerUI;
    private static ChatHudUI _chatHudUI;
    private static PlayerListOverlayUI _playerListOverlayUI;

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
            builder
            .Register(_ => UniversalUI.RegisterUI(MyPluginInfo.PLUGIN_GUID, null))
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

        _multiplayerUI = container.Resolve<MultiplayerUI>();
        _chatHudUI = container.Resolve<ChatHudUI>();
        _playerListOverlayUI = container.Resolve<PlayerListOverlayUI>();
        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void OnLog(string message, LogType type)
    {
        Logger.LogInfo(message);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            _multiplayerUI.Enabled = !_multiplayerUI.Enabled;

        if (Input.GetKeyDown(KeyCode.Tab))
            _playerListOverlayUI.SetActive(true);
        else if (Input.GetKeyUp(KeyCode.Tab))
            _playerListOverlayUI.SetActive(false);

    }
}
