using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Widgets.ScrollView;

namespace GOILauncher.Multiplayer.UI.ScrollView.Message
{
    public class MessageHandler : ICellPoolDataSource<MessageCell>
    {
        private static readonly Color MessageAreaBackgroundColor = new Color(0f, 0f, 0f, 0.4f);

        public List<Client.Models.Message> Messages { get; set; } = new List<Client.Models.Message>();
        public ScrollPool<MessageCell> _scrollPool;
        private RectTransform _scrollViewRect;
        private int _lastItemCount;
        public int ItemCount => Messages.Count;
        public event Action<bool> MessagesUpdated;

        public void Setup(GameObject parent)
        {
            _scrollPool = UIFactory.CreateScrollPool<MessageCell>(
                parent,
                "MessageScrollView",
                out GameObject scrollView,
                out GameObject scrollContent,
                MessageAreaBackgroundColor);
            _scrollViewRect = scrollView.GetComponent<RectTransform>();
            HideMaskGraphic(scrollView.transform.Find("Viewport")?.gameObject);
            UIFactory.SetLayoutElement(scrollView, minWidth: 400, minHeight: 120, flexibleWidth: 9999, flexibleHeight: 9999);

            _scrollPool.Initialize(this);
            HideScrollbar(scrollView);
        }

        private static void HideMaskGraphic(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            Mask mask = gameObject.GetComponent<Mask>();
            if (mask != null)
                mask.showMaskGraphic = false;
        }

        private static void HideScrollbar(GameObject scrollView)
        {
            Transform sliderContainer = scrollView.transform.Find("SliderContainer");
            if (sliderContainer != null)
                sliderContainer.gameObject.SetActive(false);

            RectTransform viewport = scrollView.transform.Find("Viewport")?.GetComponent<RectTransform>();
            if (viewport == null)
                return;

            viewport.offsetMax = Vector2.zero;
        }

        public void OnCellBorrowed(MessageCell cell)
        {
        }

        public void SetCell(MessageCell cell, int index)
        {
            if (index < 0 || index >= Messages.Count)
            {
                cell.Disable();
                return;
            }

            cell.ConfigureCell(Messages[index]);
            cell.Enable();
        }

        public void Update(IList<Client.Models.Message> messages, bool scrollToBottom = false)
        {
            // 拷贝一份，防止外部只读集合/共享列表被后续修改影响渲染
            Messages = messages != null
                ? new List<Client.Models.Message>(messages)
                : new List<Client.Models.Message>();
            int currentCount = Messages.Count;
            bool hasNewMessages = currentCount > _lastItemCount;
            _scrollPool.Refresh(true);
            _lastItemCount = currentCount;
            if (hasNewMessages && scrollToBottom)
                ScrollToBottom();
            MessagesUpdated?.Invoke(hasNewMessages);
        }

        public bool IsPointerInside()
        {
            return _scrollViewRect != null &&
                   RectTransformUtility.RectangleContainsScreenPoint(_scrollViewRect, Input.mousePosition);
        }

        private void ScrollToBottom()
        {
            if (Messages.Count == 0 || _scrollPool == null || _scrollPool.CellPool.Count == 0)
                return;

            _scrollPool.JumpToIndex(Messages.Count - 1, null);
        }
    }
}
