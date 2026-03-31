using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using GOILauncher.Multiplayer.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZenFulcrum.EmbeddedBrowser;

namespace GOILauncher.Multiplayer;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    private UIManager _uiManager;
    private GameObject _canvasObj;
    private GameObject _panelObj;
    private Browser _browser;
    private RawImage _browserImage;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        Invoke(nameof(CreateUI), 1f);
    }

    private void Update()
    {
        if (_canvasObj != null && Input.GetKeyDown(KeyCode.F1))
        {
            if (!_canvasObj.activeSelf)
                Input.imeCompositionMode = IMECompositionMode.On;
            _canvasObj.SetActive(!_canvasObj.activeSelf);
        }
        // if (_browser != null && _browserImage != null)
        // {
        //     _browserImage.texture = _browser.Texture;
        // }
    }

    private void CreateUI()
    {
        var uiManagerObj = new GameObject(nameof(UIManager));
        DontDestroyOnLoad(uiManagerObj);
        _uiManager = uiManagerObj.AddComponent<UIManager>();
        _canvasObj = _uiManager.CreateCanvas();
        _panelObj = _uiManager.CreatePanel(_canvasObj.transform);
        _uiManager.CreateInputField(_panelObj.transform, "Username");
        // var browserObj = new GameObject(nameof(Browser));
        // DontDestroyOnLoad(browserObj);
        // browserObj.transform.SetParent(_panelObj.transform, false);
        // browserObj.AddComponent<PointerUIGUI>();
        // _browser = browserObj.GetComponent<Browser>();
        // _browserImage = browserObj.GetComponent<RawImage>();
        // var rectTransform = _browserImage.rectTransform;
        // rectTransform.anchorMin = Vector2.zero;
        // rectTransform.anchorMax = Vector2.one;
        // rectTransform.offsetMin = Vector2.zero;
        // rectTransform.offsetMax = Vector2.zero;
        // rectTransform.sizeDelta = Vector2.zero;
        // var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        // var path = Path.Combine(folder, "index.html");
        // _browser.LoadURL($"file://{path}", true);
    }
}
