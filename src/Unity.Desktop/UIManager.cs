using System;
using GOILauncher.Multiplayer.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GOILauncher.Multiplayer
{
    public class UIManager : MonoBehaviour
    {
        private TMP_FontAsset _sourceHanNormal;
        private TMP_FontAsset _sourceHanBold;

        private void Awake()
        {
            foreach (var font in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
            {
                if (font.name == "SourceHanNormal")
                    _sourceHanNormal = font;
                if (font.name == "SourceHanBold")
                    _sourceHanBold = font;

            }
        }

        public GameObject CreateCanvas()
        {
            // Create a new Canvas GameObject
            var canvasObj = new GameObject("MultiplayerCanvas");
            DontDestroyOnLoad(canvasObj);
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            // Set the sorting order to ensure it appears above other UI elements
            canvas.sortingOrder = 0721;

            // Add a GraphicRaycaster to the Canvas to handle UI interactions
            canvasObj.AddComponent<GraphicRaycaster>();

            // Add a CanvasScaler to ensure the UI scales properly across different screen resolutions
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            return canvasObj;
        }

        public GameObject CreatePanel(Transform parent)
        {
            // Create a new Panel GameObject as a child of the Canvas
            var panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(parent, false);

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
            return panelObj;
        }

        public GameObject CreateButton(Transform parent, string text, Action callback = null)
        {
            // Create a new Button GameObject
            var buttonObj = new GameObject("Button");
            buttonObj.transform.SetParent(parent, false);

            // copy the Image properties
            var image = buttonObj.AddComponent<Image>();
            image.sprite = SpriteLoader.LoadFromFile("Button.png",
                default,
                new Vector2(148, 92),
                new Vector4(92, 92, 92, 92));
            image.pixelsPerUnitMultiplier = 2;
            image.type = Image.Type.Sliced;

            // copy the Button properties
            var button = buttonObj.AddComponent<Button>();
            button.colors = new ColorBlock
            {
                normalColor = Color.white,
                highlightedColor = new Color(0.9608f, 0.9608f, 0.9608f),
                pressedColor = new Color(0.7843f, 0.7843f, 0.7843f),
                selectedColor = new Color(0.9608f, 0.9608f, 0.9608f),
                disabledColor = new Color(0.7843f, 0.7843f, 0.7843f, 0.502f),
                colorMultiplier = 1,
                fadeDuration = 0.1f
            };
            var spriteSelected = SpriteLoader.LoadFromFile("ButtonSelected.png",
                default,
                new Vector2(148, 92),
                new Vector4(92, 92, 92, 92));
            button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState
            {
                highlightedSprite = spriteSelected,
                pressedSprite = spriteSelected,
                disabledSprite = spriteSelected
            };
            button.targetGraphic = image;
            button.onClick.AddListener(() => callback?.Invoke());

            // copy the ContentSizeFitter properties
            var sizeFitter = buttonObj.AddComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            // copy the HorizontalLayoutGroup properties
            var horizontal = buttonObj.AddComponent<HorizontalLayoutGroup>();
            horizontal.childAlignment = TextAnchor.MiddleCenter;
            horizontal.childControlWidth = true;
            horizontal.childControlHeight = false;
            horizontal.childForceExpandWidth = true;
            horizontal.childForceExpandHeight = false;
            horizontal.padding = new RectOffset(30, 30, 15, 0);


            // copy the TextMeshProUGUI properties
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.font = _sourceHanBold;
            tmp.text = text;
            tmp.fontSize = 48;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = false;

            // Set the button size
            RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(0, 92);
            return buttonObj;
        }

        public GameObject CreateText(Transform parent, string text)
        {
            // Create a new Text GameObject
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(parent, false);

            // copy the TextMeshProUGUI properties
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.font = _sourceHanNormal;
            tmp.text = text;
            tmp.fontSize = 48;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Right;
            tmp.enableAutoSizing = false;
            tmp.horizontalMapping = TextureMappingOptions.Character;
            tmp.enableWordWrapping = false;

            // Auto size the text width
            var contentSizeFitter = textObj.AddComponent<ContentSizeFitter>();
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            // Set the text size
            RectTransform rectTransform = textObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 47);
            return textObj;
        }

        public GameObject CreateInputField(Transform parent, string placeholder = "")
        {
            // Create a new InputField GameObject
            var inputFieldObj = new GameObject("InputField");
            inputFieldObj.transform.SetParent(parent, false);

            // copy the Image properties
            var image = inputFieldObj.AddComponent<Image>();
            image.sprite = SpriteLoader.LoadFromFile("InputField.png",
                new Rect(0, 0, 32, 32),
                new Vector2(16, 16),
                new Vector4(10, 10, 10, 10));
            image.pixelsPerUnitMultiplier = 2;
            image.type = Image.Type.Sliced;
            image.color = Color.white;

            // copy the InputField properties
            var inputField = inputFieldObj.AddComponent<TMP_InputField>();
            inputField.targetGraphic = image;

            // create the Text Area
            var textAreaObj = new GameObject("TextArea");
            textAreaObj.transform.SetParent(inputFieldObj.transform, false);
            var textAreaRect = textAreaObj.AddComponent<RectTransform>();

            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = new Vector2(10, 6);
            textAreaRect.offsetMax = new Vector2(-10, -6);
            
            textAreaObj.AddComponent<RectMask2D>();

            // create the Placeholder
            var placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(textAreaObj.transform, false);
            var placeholderRect = placeholderObj.AddComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = Vector2.zero;
            placeholderRect.offsetMax = Vector2.zero;

            var placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholderText.font = _sourceHanNormal;
            placeholderText.text = placeholder;
            placeholderText.fontSize = 48;
            placeholderText.color = new Color(0.1961f, 0.1961f, 0.1961f, 1f);
            placeholderText.alignment = TextAlignmentOptions.Left;
            placeholderText.enableWordWrapping = false;
            placeholderText.extraPadding = true;

            // create the Text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(textAreaObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.font = _sourceHanNormal;
            text.text = "";
            text.fontSize = 48;
            text.color = new Color(0.1961f, 0.1961f, 0.1961f, 1f);
            text.alignment = TextAlignmentOptions.Left;
            text.enableWordWrapping = false;
            text.extraPadding = true;

            inputField.textViewport = textAreaRect;
            inputField.placeholder = placeholderText;
            inputField.textComponent = text;
            inputField.fontAsset = _sourceHanNormal;

            inputField.caretColor = new Color(0.1961f, 0.1961f, 0.1961f, 1f);
            inputField.caretWidth = 1;
            inputField.selectionColor = new Color(0.6588f, 0.8078f, 1f, 0.7529f);
            inputField.colors = new ColorBlock
            {
                normalColor = Color.white,
                highlightedColor = new Color(0.9608f, 0.9608f, 0.9608f),
                pressedColor = new Color(0.7843f, 0.7843f, 0.7843f),
                selectedColor = new Color(0.9608f, 0.9608f, 0.9608f),
                disabledColor = new Color(0.7843f, 0.7843f, 0.7843f, 0.502f),
                colorMultiplier = 1,
                fadeDuration = 0.1f
            };

            var inputFieldRectTransform = inputFieldObj.GetComponent<RectTransform>();
            inputFieldRectTransform.sizeDelta = new Vector2(431.8f, 58.2f);


            return inputFieldObj;
        }
    }
}
