using System;
using GOILauncher.Multiplayer.UI.Components;
using GOILauncher.Multiplayer.UI.Pages;
using UniverseLib.UI;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI.Models;
using UniverseLib.UI.Panels;

namespace GOILauncher.Multiplayer.UI
{
    public class MultiplayerUI : PanelBase
    {
        private const string ClientPageName = "Client";
        private const string ServerPageName = "Server";

        public MultiplayerUI(UIBase owner) : base(owner)
        {
        }

        public override string Name => "GOILauncher.Multiplayer";

        public override int MinWidth => 600;

        public override int MinHeight => 800;

        public override Vector2 DefaultAnchorMin => new Vector2(0.3f, 0.3f);
        public override Vector2 DefaultAnchorMax => new Vector2(0.7f, 0.7f);

        public override bool CanDragAndResize => true;

        private ButtonRef clientButton;
        private ButtonRef serverButton;
        private ClientPageView clientPage;
        private ServerPageView serverPage;
        private IRoomListUiComponent roomListComponent;
        private IServerControlUiComponent serverControlComponent;

        public MultiplayerUI(UIBase owner, IRoomListUiComponent roomListComponent) : this(owner)
        {
            BindRoomListComponent(roomListComponent);
        }

        public MultiplayerUI(UIBase owner, IRoomListUiComponent roomListComponent, IServerControlUiComponent serverControlComponent) : this(owner)
        {
            BindRoomListComponent(roomListComponent);
            BindServerControlComponent(serverControlComponent);
        }

        public void BindRoomListComponent(IRoomListUiComponent roomListComponent)
        {
            this.roomListComponent = roomListComponent;

            if (clientPage != null)
                clientPage.Bind(roomListComponent);
        }

        public void BindServerControlComponent(IServerControlUiComponent serverControlComponent)
        {
            this.serverControlComponent = serverControlComponent;

            if (serverPage != null)
                serverPage.Bind(serverControlComponent);
        }

        private ColorBlock _normalButtonColors = new ColorBlock
        {
            normalColor = new Color(0.25f, 0.25f, 0.25f, 1f),
            highlightedColor = new Color(0.3f, 0.3f, 0.3f, 1.2f),
            pressedColor = new Color(0.175f, 0.175f, 0.175f, 0.75f),
            selectedColor = new Color(0, 0, 0, 0),
            disabledColor = new Color(0, 0, 0, 0),
            colorMultiplier = 1,
            fadeDuration = 0
        };

        private ColorBlock _selectedButtonColors = new ColorBlock
        {
            normalColor = new Color(0.2f, 0.4f, 0.28f, 1f),
            highlightedColor = new Color(0.24f, 0.48f, 0.336f, 1.2f),
            pressedColor = new Color(0.175f, 0.175f, 0.175f, 0.75f),
            selectedColor = new Color(0, 0, 0, 0),
            disabledColor = new Color(0, 0, 0, 0),
            colorMultiplier = 1,
            fadeDuration = 0
        };

        protected override void ConstructPanelContent()
        {
            // Buttons Row
            GameObject buttonRow = UIFactory.CreateHorizontalGroup(ContentRoot, "ButtonRow", false, false, true, true, 5, new Vector4(5, 5, 5, 5));
            UIFactory.SetLayoutElement(buttonRow, minHeight: 40, flexibleHeight: 0);
            buttonRow.AddComponent<ToggleGroup>();

            clientButton = UIFactory.CreateButton(buttonRow, "ClientButton", "客户端");
            UIFactory.SetLayoutElement(clientButton.Component.gameObject, minHeight: 30, minWidth: 100, flexibleWidth: 9999);
            clientButton.OnClick += () => { ShowPage(ClientPageName); };

            serverButton = UIFactory.CreateButton(buttonRow, "ServerButton", "服务端");
            UIFactory.SetLayoutElement(serverButton.Component.gameObject, minHeight: 30, minWidth: 100, flexibleWidth: 9999);
            serverButton.OnClick += () => { ShowPage(ServerPageName); };

            // Pages Container
            GameObject pagesContainer = UIFactory.CreateUIObject("PagesContainer", ContentRoot);
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(pagesContainer, false, false, true, true, 0);
            UIFactory.SetLayoutElement(pagesContainer, flexibleHeight: 9999, flexibleWidth: 9999);
            clientPage = new ClientPageView(pagesContainer);

            if (roomListComponent != null)
                clientPage.Bind(roomListComponent);

            serverPage = new ServerPageView(pagesContainer);

            if (serverControlComponent != null)
                serverPage.Bind(serverControlComponent);

            // 默认选中客户端
            ShowPage(ClientPageName);
        }

        private void ShowPage(string pageName)
        {
            bool isClientPage = string.Equals(pageName, ClientPageName, StringComparison.Ordinal);
            bool isServerPage = string.Equals(pageName, ServerPageName, StringComparison.Ordinal);

            if (isClientPage)
            {
                clientButton.Component.colors = _selectedButtonColors;
                serverButton.Component.colors = _normalButtonColors;
            }
            else if (isServerPage)
            {
                serverButton.Component.colors = _selectedButtonColors;
                clientButton.Component.colors = _normalButtonColors;
            }

            if (clientPage != null)
                clientPage.SetActive(isClientPage);

            if (serverPage != null)
                serverPage.SetActive(isServerPage);

            Plugin.Logger.LogInfo($"Switched to {pageName} page.");
        }
    }
}
