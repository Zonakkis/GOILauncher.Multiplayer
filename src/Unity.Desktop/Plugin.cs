using BepInEx;
using BepInEx.Logging;
using GOILauncher.Multiplayer.UI;
using GOILauncher.Multiplayer.UI.Components;
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
    public static MultiplayerUI MultiplayerUI { get; private set; }
    public static ChatHudUI ChatHudUI { get; private set; }
    public static PlayerListOverlayUI PlayerListOverlayUI { get; private set; }

    public static IMultiplayerUiComponents UiComponents { get; private set; }

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        Universe.Init(1f, OnInitialized, OnLog, new UniverseLibConfig
        {
            Force_Unlock_Mouse = true,
            Disable_EventSystem_Override = false
        });
    }

    private void OnInitialized()
    {
        UIBase = UniversalUI.RegisterUI(MyPluginInfo.PLUGIN_GUID, null);

        UiComponents = new DefaultMultiplayerUiComponents();

        MultiplayerUI = new MultiplayerUI(UIBase, UiComponents.RoomList, UiComponents.ServerControl);
        ChatHudUI = new ChatHudUI(UIBase, UiComponents.Chat);
        PlayerListOverlayUI = new PlayerListOverlayUI(UIBase, UiComponents.PlayerList);

        MultiplayerUI.SetActive(true);
        ChatHudUI.SetActive(true);
        PlayerListOverlayUI.SetActive(false);
    }

    private void OnLog(string message, LogType type)
    {
        Logger.LogInfo(message);
    }

    private void Update()
    {
        if (MultiplayerUI != null && Input.GetKeyDown(KeyCode.F1))
            MultiplayerUI.Enabled = !MultiplayerUI.Enabled;

        if (PlayerListOverlayUI != null)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
                PlayerListOverlayUI.SetActive(true);
            else if (Input.GetKeyUp(KeyCode.Tab))
                PlayerListOverlayUI.SetActive(false);

        }
    }
}
