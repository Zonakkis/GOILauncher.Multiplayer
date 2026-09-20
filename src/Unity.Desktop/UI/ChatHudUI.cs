using System;
using GOILauncher.Multiplayer.Client.Models;
using GOILauncher.Multiplayer.UI.ScrollView.Message;
using UniverseLib.UI;
using UniverseLib.UI.Models;
using UniverseLib.UI.Panels;
using UnityEngine;
using UnityEngine.UI;
using GOILauncher.Multiplayer.Unity;

namespace GOILauncher.Multiplayer.UI
{
    public class ChatHudUI : PanelBase
    {
        private const float PassiveVisibleSeconds = 6f;
        private const float PassiveFadeSeconds = 0.75f;

        private InputFieldRef messageInput;
        // 门面每轮联机都是新造的：Bind 时才有，Unbind 时清空。
        private IUnityClient _client;
        private readonly RoomDialogUI _roomDialog;
        private MessageHandler _messageHandler;
        private GameObject inputRow;
        private CanvasGroup canvasGroup;
        private bool isActiveMode;
        private float lastPassiveActivityTime;

        public ChatHudUI(UIBase owner, MessageHandler messageHandler, RoomDialogUI roomDialog) : base(owner)
        {
            ImageUtility.MakeTransparent(UIRoot);
            ImageUtility.MakeTransparent(ContentRoot);
            canvasGroup = UIRoot.GetComponent<CanvasGroup>() ?? UIRoot.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;

            _roomDialog = roomDialog;
            _roomDialog.ActiveChanged += active => { if (active && isActiveMode) SetActiveMode(false); };
            _messageHandler = messageHandler;
            _messageHandler.MessagesUpdated += OnMessagesUpdated;
            _messageHandler.Setup(ContentRoot);
            CreateInputRow();
            RefreshMessages();
            SetActiveMode(false);
            LayoutRebuilder.ForceRebuildLayoutImmediate(ContentRoot.GetComponent<RectTransform>());
        }

        /// <summary>绑定这一轮的客户端门面；聊天消息和记录清空都从它身上订。</summary>
        public void Bind(IUnityClient client)
        {
            if (client == null || _client != null)
                throw new InvalidOperationException("ChatHudUI: Bind and Unbind must alternate.");

            _client = client;
            _client.ChatMessageReceived += OnChatMessageReceived;
            _client.ChatHistoryReset += OnChatHistoryReset;
            RefreshMessages();
        }

        public void Unbind()
        {
            if (_client == null)
                return;

            _client.ChatMessageReceived -= OnChatMessageReceived;
            _client.ChatHistoryReset -= OnChatHistoryReset;
            _client = null;
            // 上一轮的聊天记录不属于这一轮，清空而不是留着：面板展示的始终是"当前连接的记录"。
            RefreshMessages();
        }

        public override string Name => "GOILauncher.Chat";

        public override int MinWidth => 600;

        public override int MinHeight => 300;

        public override Vector2 DefaultAnchorMin => new Vector2(0, 0.4f);

        public override Vector2 DefaultAnchorMax => new Vector2(0, 0.4f);

        public override Vector2 DefaultPosition => new Vector2(-CanvasWidth * 0.5f, 0);

        public override bool CanDragAndResize => isActiveMode;

        public bool IsActiveMode => isActiveMode;

        public event Action<bool> ActiveModeChanged;

        public override void SetActive(bool active)
        {
            if (!active && isActiveMode)
                SetActiveMode(false);

            base.SetActive(active);
        }

        private float CanvasWidth
        {
            get
            {
                float width = Screen.width > 0 ? Screen.width : ResponsiveUIBase.ReferenceWidth;
                float height = Screen.height > 0 ? Screen.height : ResponsiveUIBase.ReferenceHeight;
                float scaleFactor = Mathf.Min(width / ResponsiveUIBase.ReferenceWidth, height / ResponsiveUIBase.ReferenceHeight);
                if (scaleFactor > 0f)
                    return width / scaleFactor;

                return ResponsiveUIBase.ReferenceWidth;
            }
        }

        protected override void ConstructPanelContent()
        {
        }

        protected override PanelDragger CreatePanelDragger()
        {
            return new ResponsivePanelDragger(this);
        }

        public override void Update()
        {
            if (messageInput == null || inputRow == null)
                return;

            if (_roomDialog.BlocksGameplayShortcuts) { UpdatePassiveFade(); return; }
            if (!isActiveMode)
            {
                if (UiInputFocus.IsEditing) { UpdatePassiveFade(); return; }
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    SetActiveMode(true);
                    return;
                }

                UpdatePassiveFade();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SetActiveMode(false);
                return;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OnSendClicked();
                return;
            }

            if (Input.GetMouseButtonDown(0) && !IsPointerInside(Rect))
                SetActiveMode(false);
        }

        private void CreateInputRow()
        {
            inputRow = UIFactory.CreateHorizontalGroup(
                ContentRoot,
                "ChatInputRow",
                false,
                false,
                true,
                true,
                6,
                // UIFactory 按 (top, bottom, left, right) 取这个 Vector4，不是直觉的
                // (left, top, right, bottom)。行高 34 扣掉上下各 5，正好放得下 24 高的输入框。
                new Vector4(5, 5, 6, 6),
                new Color(0f, 0f, 0f, 0.35f),
                TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(inputRow, minHeight: 34, flexibleHeight: 0, flexibleWidth: 9999);

            messageInput = UIFactory.CreateInputField(inputRow, "ChatInput", "\u8f93\u5165\u6d88\u606f...");
            UIFactory.SetLayoutElement(messageInput.GameObject, minHeight: 24, flexibleHeight: 0, flexibleWidth: 9999);

            ButtonRef sendButton = UIFactory.CreateButton(inputRow, "ChatSendButton", "\u53d1\u9001");
            UIFactory.SetLayoutElement(sendButton.Component.gameObject, minWidth: 78, minHeight: 24, flexibleWidth: 0, flexibleHeight: 0);
            sendButton.OnClick += OnSendClicked;
        }

        private void OnSendClicked()
        {
            if (messageInput == null || _messageHandler == null)
                return;

            string text = messageInput.Text?.Trim();
            if (string.IsNullOrWhiteSpace(text))
                return;

            if (_client == null)
                return;

            _client.SendMessage(MessageType.Player, text);
            messageInput.Text = string.Empty;
            FocusInput();
        }

        private void OnChatHistoryReset()
        {
            messageInput.Text = string.Empty;
            RefreshMessages();
        }

        private void OnChatMessageReceived(Message message)
        {
            if (_messageHandler == null)
                return;

            // 这里用不到 message：聊天面板展示的是完整记录，直接从 ChatMessages 全量重绘。
            // 只关心某几类消息的宿主可以反过来只读 message、自己攒一份记录。
            _messageHandler.Update(_client.ChatMessages, ShouldAutoScrollMessages());
        }

        private void RefreshMessages()
        {
            if (_messageHandler == null)
                return;

            // 没有门面就是一份空记录：Unbind 之后不留上一轮的消息，面板显示的始终是当前连接那份。
            _messageHandler.Update(_client == null ? null : _client.ChatMessages);
        }

        private void OnMessagesUpdated(bool hasNewMessages)
        {
            if (!hasNewMessages)
                return;

            ShowPassiveNow();
        }

        private bool ShouldAutoScrollMessages()
        {
            return _messageHandler != null && !_messageHandler.IsPointerInside();
        }

        private void SetActiveMode(bool active)
        {
            bool changed = isActiveMode != active;
            isActiveMode = active;
            TitleBar.SetActive(active);
            inputRow.SetActive(active);
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = active;
            Dragger.OnEndResize();

            if (active)
                FocusInput();
            else
            {
                messageInput.Component.DeactivateInputField();
                ShowPassiveNow();
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(ContentRoot.GetComponent<RectTransform>());

            if (changed)
                ActiveModeChanged?.Invoke(active);
        }

        private void ShowPassiveNow()
        {
            lastPassiveActivityTime = Time.unscaledTime;
            canvasGroup.alpha = 1f;
        }

        private void UpdatePassiveFade()
        {
            float elapsed = Time.unscaledTime - lastPassiveActivityTime;
            if (elapsed <= PassiveVisibleSeconds)
            {
                canvasGroup.alpha = 1f;
                return;
            }

            float fadeProgress = PassiveFadeSeconds > 0f
                ? Mathf.Clamp01((elapsed - PassiveVisibleSeconds) / PassiveFadeSeconds)
                : 1f;
            canvasGroup.alpha = 1f - fadeProgress;
        }

        private void FocusInput()
        {
            messageInput.Component.ActivateInputField();
            messageInput.Component.Select();
        }

        private static bool IsPointerInside(RectTransform rect)
        {
            return rect != null &&
                   RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition);
        }
    }
}
