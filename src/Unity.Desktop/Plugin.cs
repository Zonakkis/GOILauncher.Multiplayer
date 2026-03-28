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
        canvas.worldCamera = Camera.main;
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
        rectTransform.anchorMin = new Vector2(0.08f, 0f);
        rectTransform.anchorMax = new Vector2(0.92f, 1f);
        rectTransform.anchoredPosition = new Vector2(-0.5f, 0f);
        rectTransform.anchoredPosition3D = new Vector3(-0.5f, 0f, 0f);
        rectTransform.sizeDelta = new Vector2(-5f, 0f);
        rectTransform.offsetMin = new Vector2(12f, 14f);
        rectTransform.offsetMax = new Vector2(-13f, -8f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // copy the Image properties
        var image = panelObj.AddComponent<Image>();
        image.sprite = SpriteLoader.LoadFromFile("Panel.png",
            new Rect(11.0129f, 13.0761f, 193.9742f, 178.847f),
            new Vector2(107.5f, 100.5f),
            new Vector4(23, 59, 79, 52));
        image.color = new Color(0, 0, 0, 0.703f);
        image.type = Image.Type.Sliced;
        image.raycastTarget = false;
    }
}
