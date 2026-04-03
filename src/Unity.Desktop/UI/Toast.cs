using UniverseLib.UI;
using UniverseLib.UI.Panels;
using UnityEngine;
using UnityEngine.UI;

namespace GOILauncher.Multiplayer.UI
{
    public class Toast : PanelBase
    {
        private const float DefaultDurationSeconds = 2.5f;
        private const float FadeDurationSeconds = 0.35f;

        private Text messageText;
        private CanvasGroup canvasGroup;
        private float hideAtTime;
        private bool showing;

        public Toast(UIBase owner) : base(owner)
        {
            SetActive(false);
        }

        public override string Name => "GOILauncher.Toast";

        public override int MinWidth => 420;

        public override int MinHeight => 56;

        public override Vector2 DefaultAnchorMin => new Vector2(0.5f, 1f);

        public override Vector2 DefaultAnchorMax => new Vector2(0.5f, 1f);

        public override Vector2 DefaultPosition => new Vector2(0f, -30f);

        public override bool CanDragAndResize => false;

        public override void SetDefaultSizeAndPosition()
        {
            // Keep toast pinned to top-center regardless of screen aspect.
            Rect.pivot = new Vector2(0.5f, 1f);
            Rect.anchorMin = DefaultAnchorMin;
            Rect.anchorMax = DefaultAnchorMax;
            Rect.anchoredPosition = DefaultPosition;

            Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, MinWidth);
            Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, MinHeight);

            Dragger.OnEndResize();
        }

        public void ShowToast(string message, float durationSeconds = DefaultDurationSeconds)
        {
            if (string.IsNullOrWhiteSpace(message) || messageText == null)
                return;

            messageText.text = message.Trim();
            hideAtTime = Time.unscaledTime + Mathf.Max(0.4f, durationSeconds);
            showing = true;

            if (!Enabled)
                SetActive(true);

            if (canvasGroup != null)
                canvasGroup.alpha = 1f;
        }

        public override void Update()
        {
            if (!showing)
                return;

            float remaining = hideAtTime - Time.unscaledTime;
            if (remaining <= 0f)
            {
                showing = false;
                SetActive(false);
                return;
            }

            if (canvasGroup == null)
                return;

            if (remaining <= FadeDurationSeconds)
            {
                canvasGroup.alpha = Mathf.Clamp01(remaining / FadeDurationSeconds);
            }
            else if (canvasGroup.alpha < 1f)
            {
                canvasGroup.alpha = 1f;
            }
        }

        protected override void ConstructPanelContent()
        {
            canvasGroup = UIRoot.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = UIRoot.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 1f;

            GameObject body = UIFactory.CreateHorizontalGroup(
                ContentRoot,
                "ToastBody",
                true,
                true,
                true,
                true,
                0,
                new Vector4(14, 10, 14, 10),
                new Color(0, 0, 0, 0.5f),
                TextAnchor.MiddleCenter);
            UIFactory.SetLayoutElement(body, minHeight: 48, flexibleHeight: 9999, flexibleWidth: 9999);

            messageText = UIFactory.CreateLabel(body, "ToastMessage", string.Empty, TextAnchor.MiddleCenter, Color.white, true, 16);
            messageText.horizontalOverflow = HorizontalWrapMode.Wrap;
            messageText.verticalOverflow = VerticalWrapMode.Truncate;
            UIFactory.SetLayoutElement(messageText.gameObject, minHeight: 28, flexibleHeight: 0, flexibleWidth: 9999);
            var images = uiRoot.GetComponentsInChildren<Image>();
            for (int i = 0; i < images.Length; i++)
            {
                var image = images[i];
                if (image.gameObject != body)
                    image.color = new Color(0, 0, 0, 0);
            }
        }
    }
}