using System.Diagnostics.CodeAnalysis;
using BepInEx;
using BepInEx.Logging;

namespace GOILauncher.Multiplayer;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
        
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity calls this method via reflection.")]
    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }
}
