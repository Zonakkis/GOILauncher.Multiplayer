using System;
using GOILauncher.Multiplayer.Client;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Extensions;
using GOILauncher.Multiplayer.UI.Pages;
using GOILauncher.Multiplayer.Unity;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Models;
using UniverseLib.UI.Panels;

namespace GOILauncher.Multiplayer.UI
{
    public class MultiplayerUI : PanelBase
    {
        private const string ClientPageName = "Client";
        private const string ServerPageName = "Server";
        public override string Name => "连接配置";

        public override int MinWidth => 600;

        public override int MinHeight => 800;

        public override Vector2 DefaultAnchorMin => new Vector2(0.5f, 0.5f);
        public override Vector2 DefaultAnchorMax => new Vector2(0.5f, 0.5f);
        public override Vector2 DefaultPosition => new Vector2(-300f, 400f);

        public override bool CanDragAndResize => true;

        private ButtonRef clientButton;
        private ButtonRef serverButton;
        private ClientPage clientPage;
        private ServerPage serverPage;

        public MultiplayerUI(UIBase owner, IUnityServer unityServer, IUnityClient unityClient,
            Toast toast,
            ILogger<ServerPage> serverPageLogger) : base(owner)
        {
            clientPage.Client = unityClient;
            serverPage.Toast = toast;
            serverPage.Logger = serverPageLogger;
            serverPage.Server = unityServer;
        }

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

            clientPage = new ClientPage(pagesContainer);

            serverPage = new ServerPage(pagesContainer);

            // 默认选中客户端
            ShowPage(ClientPageName);
        }

        private void ShowPage(string pageName)
        {
            bool isClientPage = string.Equals(pageName, ClientPageName, StringComparison.Ordinal);
            bool isServerPage = string.Equals(pageName, ServerPageName, StringComparison.Ordinal);

            clientButton.SetTabActive(isClientPage);
            serverButton.SetTabActive(isServerPage);
            clientPage.SetActive(isClientPage);
            serverPage.SetActive(isServerPage);

            Plugin.Logger.LogInfo($"Switched to {pageName} page.");
        }
    }
}
