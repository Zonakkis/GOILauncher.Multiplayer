using System;
using BepInEx;
using BepInEx.Logging;
using GOILauncher.Multiplayer.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GOILauncher.Multiplayer;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    private UIManager _uiManager;
    private GameObject _canvasObj;
    private GameObject _panelObj;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        Invoke(nameof(CreateUI), 1f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            _canvasObj?.SetActive(!_canvasObj.activeSelf);
        }
    }

    private void CreateUI()
    {
        var uiManagerObj = new GameObject(nameof(UIManager));
        DontDestroyOnLoad(uiManagerObj);
        _uiManager = uiManagerObj.AddComponent<UIManager>();
        _canvasObj = _uiManager.CreateCanvas();
        _panelObj = _uiManager.CreatePanel(_canvasObj.transform);
    }
}
