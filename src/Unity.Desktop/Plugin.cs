using BepInEx;
using BepInEx.Logging;
using GOILauncher.Multiplayer.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace GOILauncher.Multiplayer;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    private GameObject _canvasObj;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        CreateUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            _canvasObj.SetActive(!_canvasObj.activeSelf);
        }
    }

    private void CreateUI()
    {
        CreateCanvas();
        CreatePanel();
        _canvasObj.SetActive(false);
    }

    private void CreateCanvas()
    {
        // Create a new Canvas GameObject
        _canvasObj = new GameObject("MultiplayerCanvas");
        DontDestroyOnLoad(_canvasObj);
        var canvas = _canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        // Set the sorting order to ensure it appears above other UI elements
        canvas.sortingOrder = 0721;

        // Add a GraphicRaycaster to the Canvas to handle UI interactions
        _canvasObj.AddComponent<GraphicRaycaster>();

        // Add a CanvasScaler to ensure the UI scales properly across different screen resolutions
        var scaler = _canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
    }

    private void CreatePanel()
    {
        // Create a new Panel GameObject as a child of the Canvas
        var panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(_canvasObj.transform, false);

        // copy the RectTransform properties
        var rectTransform = panelObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = Vector2.zero;

        // copy the Image properties
        var image = panelObj.AddComponent<Image>();
        image.sprite = SpriteLoader.LoadFromFile("Panel.png");
        image.color = new Color(0, 0, 0, 0.703f);
        image.type = Image.Type.Sliced;
        image.raycastTarget = false;
    }
}
