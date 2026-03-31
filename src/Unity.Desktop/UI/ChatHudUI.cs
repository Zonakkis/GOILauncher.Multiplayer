using System.Collections.Generic;
using GOILauncher.Multiplayer.UI.Components;
using UniverseLib.UI;
using UniverseLib.UI.Models;
using UniverseLib.UI.Panels;
using UniverseLib.UI.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace GOILauncher.Multiplayer.UI
{
    public class ChatHudUI : PanelBase
    {
        private static readonly ChatMessageViewData[] DefaultMessages = new ChatMessageViewData[]
        {
            new ChatMessageViewData("系统", "聊天区域已就绪。"),
            new ChatMessageViewData("系统", "你可以注入自己的聊天组件。")
        };

        private IChatUiComponent chatComponent;
        private GameObject messageListContent;
        private AutoSliderScrollbar messageListScrollbar;
        private InputFieldRef messageInput;

        public ChatHudUI(UIBase owner) : base(owner)
        {
        }

        public ChatHudUI(UIBase owner, IChatUiComponent chatComponent) : this(owner)
        {
            BindChatComponent(chatComponent);
        }

        public override string Name => "GOILauncher.Chat";

        public override int MinWidth => 360;

        public override int MinHeight => 180;

        public override Vector2 DefaultAnchorMin => new Vector2(0.01f, 0.01f);

        public override Vector2 DefaultAnchorMax => new Vector2(0.33f, 0.28f);

        public override bool CanDragAndResize => true;

        public void BindChatComponent(IChatUiComponent chatComponent)
        {
            this.chatComponent = chatComponent;
            RefreshMessages();
        }

        public void RefreshMessages()
        {
            if (messageListContent == null)
                return;

            for (int i = messageListContent.transform.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.Destroy(messageListContent.transform.GetChild(i).gameObject);
            }

            int rowIndex = 0;
            foreach (ChatMessageViewData message in GetMessages())
            {
                CreateMessageRow(message, rowIndex);
                rowIndex++;
            }

            if (messageListScrollbar != null)
                messageListScrollbar.UpdateSliderHandle();
        }

        protected override void ConstructPanelContent()
        {
            GameObject headerRow = UIFactory.CreateHorizontalGroup(
                ContentRoot,
                "ChatHeaderRow",
                false,
                false,
                true,
                true,
                6,
                new Vector4(8, 4, 8, 4),
                new Color(0.16f, 0.16f, 0.16f, 1f));
            UIFactory.SetLayoutElement(headerRow, minHeight: 30, flexibleHeight: 0);

            Text headerText = UIFactory.CreateLabel(headerRow, "ChatHeaderText", "聊天", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(headerText.gameObject, minHeight: 22, flexibleHeight: 0, flexibleWidth: 9999);

            ButtonRef refreshButton = UIFactory.CreateButton(headerRow, "ChatRefreshButton", "刷新");
            UIFactory.SetLayoutElement(refreshButton.Component.gameObject, minWidth: 70, minHeight: 22, flexibleWidth: 0, flexibleHeight: 0);
            refreshButton.OnClick += RefreshMessages;

            GameObject messageScroll = UIFactory.CreateScrollView(
                ContentRoot,
                "ChatMessageScroll",
                out messageListContent,
                out messageListScrollbar,
                new Color(0.09f, 0.09f, 0.09f, 1f));
            UIFactory.SetLayoutElement(messageScroll, minHeight: 110, flexibleHeight: 9999, flexibleWidth: 9999);
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(messageListContent, false, false, true, true, 3, 4, 4, 4, 4, TextAnchor.UpperLeft);

            GameObject inputRow = UIFactory.CreateHorizontalGroup(
                ContentRoot,
                "ChatInputRow",
                false,
                false,
                true,
                true,
                6,
                new Vector4(8, 4, 8, 6),
                new Color(0.14f, 0.14f, 0.14f, 1f));
            UIFactory.SetLayoutElement(inputRow, minHeight: 34, flexibleHeight: 0);

            messageInput = UIFactory.CreateInputField(inputRow, "ChatInput", "输入消息...");
            UIFactory.SetLayoutElement(messageInput.GameObject, minHeight: 24, flexibleHeight: 0, flexibleWidth: 9999);

            ButtonRef sendButton = UIFactory.CreateButton(inputRow, "ChatSendButton", "发送");
            UIFactory.SetLayoutElement(sendButton.Component.gameObject, minWidth: 80, minHeight: 24, flexibleWidth: 0, flexibleHeight: 0);
            sendButton.OnClick += OnSendClicked;

            RefreshMessages();
        }

        private void OnSendClicked()
        {
            if (messageInput == null)
                return;

            string message = messageInput.Text;
            if (string.IsNullOrWhiteSpace(message))
                return;

            if (chatComponent != null)
            {
                chatComponent.SendMessage(message);
            }
            else
            {
                Plugin.Logger.LogInfo("Send chat: " + message);
            }

            messageInput.Text = string.Empty;
            RefreshMessages();
        }

        private ChatMessageViewData[] GetMessages()
        {
            if (chatComponent == null)
                return DefaultMessages;

            List<ChatMessageViewData> messages = new List<ChatMessageViewData>();
            foreach (ChatMessageViewData message in chatComponent.GetMessages())
            {
                messages.Add(message);
            }

            if (messages.Count == 0)
                return DefaultMessages;

            return messages.ToArray();
        }

        private void CreateMessageRow(ChatMessageViewData message, int rowIndex)
        {
            Color rowColor = rowIndex % 2 == 0
                ? new Color(0.13f, 0.13f, 0.13f, 1f)
                : new Color(0.1f, 0.1f, 0.1f, 1f);

            GameObject row = UIFactory.CreateHorizontalGroup(
                messageListContent,
                "ChatRow_" + rowIndex,
                false,
                false,
                true,
                true,
                0,
                new Vector4(6, 3, 6, 3),
                rowColor,
                TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(row, minHeight: 24, flexibleHeight: 0, flexibleWidth: 9999);

            string display = "[" + message.Sender + "] " + message.Message;
            Text text = UIFactory.CreateLabel(row, "ChatRowText", display, TextAnchor.MiddleLeft);
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            UIFactory.SetLayoutElement(text.gameObject, minHeight: 18, flexibleHeight: 0, flexibleWidth: 9999);
        }
    }
}