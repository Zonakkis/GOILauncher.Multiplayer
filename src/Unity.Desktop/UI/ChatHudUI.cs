using System;
using System.Collections.Generic;
using GOILauncher.Multiplayer.Client.Models;
using GOILauncher.Multiplayer.UI.ScrollView.Message;
using UniverseLib.UI;
using UniverseLib.UI.Models;
using UniverseLib.UI.Panels;
using UnityEngine;
using UnityEngine.UI;

namespace GOILauncher.Multiplayer.UI
{
    public class ChatHudUI : PanelBase
    {
        private const float PassiveVisibleSeconds = 6f;
        private const float PassiveFadeSeconds = 0.75f;

        private InputFieldRef messageInput;
        private MessageHandler _messageHandler;
        private GameObject inputRow;
        private CanvasGroup canvasGroup;
        private bool isActiveMode;
        private float lastPassiveActivityTime;

        public ChatHudUI(UIBase owner, MessageHandler messageHandler) : base(owner)
        {
            MakeImageTransparent(UIRoot);
            MakeImageTransparent(ContentRoot);
            canvasGroup = UIRoot.GetComponent<CanvasGroup>() ?? UIRoot.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;

            _messageHandler = messageHandler;
            _messageHandler.MessagesUpdated += OnMessagesUpdated;
            _messageHandler.Setup(ContentRoot);
            CreateInputRow();

            var mockData = new List<Message>
            {
                new Message(MessageType.System, "Alice", "Hello, world!Hello, world!Hello, world!Hello, world!Hello, world!Hello, world!Hello, world!Hello, world!Hello, world!Hello, world!Hello, world!Hello, world!Hello, world!Hello, world!"),
                new Message(MessageType.System, "Bob", "Hi, Alice!"),
                new Message(MessageType.System, "Charlie", "Good morning everyone."),
                new Message(MessageType.System, "Alice", "Hello, world!"),
                new Message(MessageType.System, "Bob", "Hi, Alice!"),
                new Message(MessageType.System, "Charlie", "Good morning everyone."),
                new Message(MessageType.System, "Alice", "Hello, world!"),
                new Message(MessageType.System, "Bob", "Hi, Alice!"),
                new Message(MessageType.System, "Charlie", "Good morning everyone."),
                new Message(MessageType.System, "Alice", "Hello, world!"),
                new Message(MessageType.System, "Bob", "Hi, Alice!"),
                new Message(MessageType.System, "Charlie", "Good morning everyone."),
            };
            _messageHandler.Update(mockData);
            SetActiveMode(false);
            LayoutRebuilder.ForceRebuildLayoutImmediate(ContentRoot.GetComponent<RectTransform>());
        }

        public override string Name => "GOILauncher.Chat";

        public override int MinWidth => 600;

        public override int MinHeight => 300;

        public override Vector2 DefaultAnchorMin => new Vector2(0, 0.4f);

        public override Vector2 DefaultAnchorMax => new Vector2(0, 0.4f);

        public override Vector2 DefaultPosition => new Vector2(-Screen.currentResolution.width / 2, 0);

        public override bool CanDragAndResize => true;

        public bool IsActiveMode => isActiveMode;

        public event Action<bool> ActiveModeChanged;

        protected override void ConstructPanelContent()
        {
        }

        public override void Update()
        {
            if (messageInput == null || inputRow == null)
                return;

            if (!isActiveMode)
            {
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
                new Vector4(6, 4, 6, 6),
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

            List<Message> messages = _messageHandler.Messages ?? new List<Message>();
            messages.Add(new Message(MessageType.Player, "\u6211", text));
            _messageHandler.Update(messages);
            messageInput.Text = string.Empty;
            FocusInput();
        }

        private void OnMessagesUpdated(bool hasNewMessages)
        {
            if (!hasNewMessages)
                return;

            ShowPassiveNow();
        }

        private void SetActiveMode(bool active)
        {
            bool changed = isActiveMode != active;
            isActiveMode = active;
            TitleBar.SetActive(active);
            inputRow.SetActive(active);
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = active;

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

        private static void MakeImageTransparent(GameObject gameObject)
        {
            Image image = gameObject.GetComponent<Image>();
            if (image == null)
                return;

            image.color = Color.clear;
            image.raycastTarget = false;
        }
    }
}
