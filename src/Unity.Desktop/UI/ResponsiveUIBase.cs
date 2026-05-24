using System;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.Input;
using UniverseLib.UI;
using UniverseLib.UI.Panels;

namespace GOILauncher.Multiplayer.UI
{
    public sealed class ResponsiveUIBase : UIBase
    {
        public const float ReferenceWidth = 1920f;
        public const float ReferenceHeight = 1080f;

        private static readonly Vector2 ReferenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);

        public ResponsiveUIBase(string id, Action updateMethod) : base(id, updateMethod)
        {
            CanvasScaler scaler = RootObject.GetComponent<CanvasScaler>() ?? RootObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        }

        protected override PanelManager CreatePanelManager()
        {
            return new ResponsivePanelManager(this);
        }
    }

    internal sealed class ResponsivePanelManager : PanelManager
    {
        public ResponsivePanelManager(UIBase owner) : base(owner)
        {
        }

        protected override Vector3 MousePosition
        {
            get
            {
                RectTransform rootRect = Owner?.RootRect;
                Vector3 screenPoint = InputManager.MousePosition;
                if (rootRect != null &&
                    RectTransformUtility.ScreenPointToWorldPointInRectangle(rootRect, screenPoint, GetCanvasCamera(), out Vector3 worldPoint))
                {
                    return worldPoint;
                }

                return base.MousePosition;
            }
        }

        internal Vector2 MousePositionInRoot
        {
            get
            {
                RectTransform rootRect = Owner?.RootRect;
                Vector3 screenPoint = InputManager.MousePosition;
                if (rootRect != null &&
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(rootRect, screenPoint, GetCanvasCamera(), out Vector2 localPoint))
                    return localPoint;

                Vector2 dimensions = CanvasDimensions;
                return new Vector2(screenPoint.x - dimensions.x * 0.5f, screenPoint.y - dimensions.y * 0.5f);
            }
        }

        internal Vector2 MousePositionInCanvas
        {
            get
            {
                Vector2 localPoint = MousePositionInRoot;
                Vector2 dimensions = CanvasDimensions;
                return new Vector2(localPoint.x + dimensions.x * 0.5f, localPoint.y + dimensions.y * 0.5f);
            }
        }

        internal Vector2 CanvasDimensions => ScreenDimensions;

        protected override Vector2 ScreenDimensions
        {
            get
            {
                RectTransform rootRect = Owner?.RootRect;
                if (rootRect != null)
                {
                    Vector2 size = rootRect.rect.size;
                    if (size.x > 0f && size.y > 0f)
                        return size;
                }

                return base.ScreenDimensions;
            }
        }

        private Camera GetCanvasCamera()
        {
            return GetCanvasCamera(Owner?.Canvas);
        }

        internal void UpdateResizeCursorPosition()
        {
            if (!resizeCursor || resizeCursorUIBase == null || resizeCursorUIBase.RootRect == null)
                return;

            Vector2 screenPoint = InputManager.MousePosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    resizeCursorUIBase.RootRect,
                    screenPoint,
                    GetCanvasCamera(resizeCursorUIBase.Canvas),
                    out Vector2 localPoint))
            {
                resizeCursor.transform.localPosition = localPoint;
            }
        }

        private static Camera GetCanvasCamera(Canvas canvas)
        {
            if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                return null;

            return canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
        }
    }
}
