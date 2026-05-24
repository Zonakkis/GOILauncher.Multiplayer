using UnityEngine;
using UniverseLib.UI.Panels;

namespace GOILauncher.Multiplayer.UI
{
    internal sealed class ResponsivePanelDragger : PanelDragger
    {
        private Vector2 lastDragPosition;
        private Vector2 lastResizePosition;
        private ResizeTypes currentResizeType = ResizeTypes.NONE;

        public ResponsivePanelDragger(PanelBase uiPanel) : base(uiPanel)
        {
        }

        public override void OnBeginDrag()
        {
            base.OnBeginDrag();
            lastDragPosition = GetMousePositionInRoot();
        }

        public override void OnDrag()
        {
            Vector2 mousePosition = GetMousePositionInRoot();
            Vector2 diff = mousePosition - lastDragPosition;
            lastDragPosition = mousePosition;

            Rect.localPosition = Rect.localPosition + (Vector3)diff;
            UIPanel.EnsureValidPosition();
        }

        public override void OnBeginResize(ResizeTypes resizeType)
        {
            base.OnBeginResize(resizeType);
            currentResizeType = resizeType;
            lastResizePosition = GetMousePositionInCanvas();
            UpdateResizeCursorPosition();
        }

        public override void OnHoverResize(ResizeTypes resizeType)
        {
            base.OnHoverResize(resizeType);
            UpdateResizeCursorPosition();
        }

        public override void OnResize()
        {
            ResponsivePanelManager manager = UIPanel.Owner.Panels as ResponsivePanelManager;
            if (manager == null)
            {
                base.OnResize();
                return;
            }

            Vector2 mousePosition = manager.MousePositionInCanvas;
            Vector2 diff = lastResizePosition - mousePosition;
            if (mousePosition == lastResizePosition)
            {
                manager.UpdateResizeCursorPosition();
                return;
            }

            Vector2 dimensions = manager.CanvasDimensions;
            if (dimensions.x <= 0f || dimensions.y <= 0f)
            {
                manager.UpdateResizeCursorPosition();
                return;
            }

            if (mousePosition.x < 0f || mousePosition.y < 0f || mousePosition.x > dimensions.x || mousePosition.y > dimensions.y)
            {
                manager.UpdateResizeCursorPosition();
                return;
            }

            lastResizePosition = mousePosition;

            float diffX = diff.x / dimensions.x;
            float diffY = diff.y / dimensions.y;

            Vector2 anchorMin = Rect.anchorMin;
            Vector2 anchorMax = Rect.anchorMax;

            if (currentResizeType.HasFlag(ResizeTypes.Left))
                anchorMin.x -= diffX;
            else if (currentResizeType.HasFlag(ResizeTypes.Right))
                anchorMax.x -= diffX;

            if (currentResizeType.HasFlag(ResizeTypes.Top))
                anchorMax.y -= diffY;
            else if (currentResizeType.HasFlag(ResizeTypes.Bottom))
                anchorMin.y -= diffY;

            Vector2 previousMin = Rect.anchorMin;
            Vector2 previousMax = Rect.anchorMax;

            Rect.anchorMin = anchorMin;
            Rect.anchorMax = anchorMax;

            if (Rect.rect.width < UIPanel.MinWidth)
            {
                Rect.anchorMin = new Vector2(previousMin.x, Rect.anchorMin.y);
                Rect.anchorMax = new Vector2(previousMax.x, Rect.anchorMax.y);
            }

            if (Rect.rect.height < UIPanel.MinHeight)
            {
                Rect.anchorMin = new Vector2(Rect.anchorMin.x, previousMin.y);
                Rect.anchorMax = new Vector2(Rect.anchorMax.x, previousMax.y);
            }

            manager.UpdateResizeCursorPosition();
        }

        private Vector2 GetMousePositionInRoot()
        {
            ResponsivePanelManager manager = UIPanel.Owner.Panels as ResponsivePanelManager;
            return manager != null ? manager.MousePositionInRoot : (Vector2)Input.mousePosition;
        }

        private Vector2 GetMousePositionInCanvas()
        {
            ResponsivePanelManager manager = UIPanel.Owner.Panels as ResponsivePanelManager;
            return manager != null ? manager.MousePositionInCanvas : (Vector2)Input.mousePosition;
        }

        private void UpdateResizeCursorPosition()
        {
            ResponsivePanelManager manager = UIPanel.Owner.Panels as ResponsivePanelManager;
            manager?.UpdateResizeCursorPosition();
        }
    }
}
