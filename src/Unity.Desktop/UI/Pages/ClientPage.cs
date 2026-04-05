using GOILauncher.Multiplayer.UI.Components;
using UniverseLib;
using UniverseLib.UI;
using UniverseLib.UI.Models;
using UniverseLib.UI.Widgets;
using UnityEngine;
using UnityEngine.UI;
using GOILauncher.Multiplayer.UI.Theme;
using GOILauncher.Multiplayer.Extensions;
using GOILauncher.Multiplayer.Client;

namespace GOILauncher.Multiplayer.UI.Pages
{
    internal class ClientPage : IPage
    {
        private static readonly RoomListItemViewData[] MockRoomItems = new RoomListItemViewData[]
        {
            new RoomListItemViewData("新手休闲房", "1/4"),
            new RoomListItemViewData("双人协作", "2/2"),
            new RoomListItemViewData("速通挑战", "3/4"),
            new RoomListItemViewData("中文交流房", "2/6"),
            new RoomListItemViewData("公开大厅 #1", "5/8"),
            new RoomListItemViewData("公开大厅 #2", "0/8")
        };

        public IUnityClient Client { get; set; }

        private GameObject roomListContent;
        private AutoSliderScrollbar roomListScrollbar;
        private IRoomListUiComponent roomListComponent;
        private InputFieldRef playerNameInput;
        private InputFieldRef serverIpInput;

        public GameObject Root { get; private set; }
        private readonly ITheme _theme = Plugin.Theme;

        public ClientPage(GameObject pagesContainer)
        {
            Root = ConstructClientPage(pagesContainer);
        }

        public void SetActive(bool active)
        {
            if (Root != null)
                Root.SetActive(active);
        }

        public void Bind(IRoomListUiComponent roomListComponent)
        {
            this.roomListComponent = roomListComponent;
            PopulateRoomList();
            SyncConnectionInputs();
        }

        private GameObject ConstructClientPage(GameObject pagesContainer)
        {
            GameObject clientPage = UIFactory.CreateVerticalGroup(
                pagesContainer,
                "ClientPage",
                false,
                false,
                true,
                true,
                6,
                new Vector4(8, 8, 8, 8),
                new Color(0.12f, 0.12f, 0.12f, 0.95f));
            UIFactory.SetLayoutElement(clientPage, flexibleHeight: 9999, flexibleWidth: 9999);

            GameObject titleRow = UIFactory.CreateHorizontalGroup(
                clientPage,
                "ClientTitleRow",
                false,
                false,
                true,
                true,
                6,
                new Vector4(8, 4, 8, 4),
                new Color(0.16f, 0.16f, 0.16f, 1f));
            UIFactory.SetLayoutElement(titleRow, minHeight: 34, flexibleHeight: 0);

            Text roomListTitle = UIFactory.CreateLabel(titleRow, "RoomListTitle", "可用房间列表", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(roomListTitle.gameObject, minHeight: 24, flexibleHeight: 0, flexibleWidth: 9999);

            ButtonRef refreshButton = UIFactory.CreateButton(titleRow, "RefreshRoomList", "刷新");
            UIFactory.SetLayoutElement(refreshButton.Component.gameObject, minHeight: 24, minWidth: 80, flexibleWidth: 0, flexibleHeight: 0);
            refreshButton.OnClick += OnRefreshClicked;

            GameObject tableHeader = UIFactory.CreateHorizontalGroup(
                clientPage,
                "RoomTableHeader",
                false,
                false,
                true,
                true,
                6,
                new Vector4(8, 5, 8, 5),
                new Color(0.2f, 0.2f, 0.2f, 1f));
            UIFactory.SetLayoutElement(tableHeader, minHeight: 32, flexibleHeight: 0);

            Text roomNameHeader = UIFactory.CreateLabel(tableHeader, "RoomNameHeader", "房间名", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(roomNameHeader.gameObject, minHeight: 22, flexibleHeight: 0, flexibleWidth: 9999);

            Text playerCountHeader = UIFactory.CreateLabel(tableHeader, "PlayerCountHeader", "玩家数", TextAnchor.MiddleCenter);
            UIFactory.SetLayoutElement(playerCountHeader.gameObject, minWidth: 100, preferredWidth: 110, minHeight: 22, flexibleHeight: 0, flexibleWidth: 0);

            Text actionHeader = UIFactory.CreateLabel(tableHeader, "ActionHeader", "操作", TextAnchor.MiddleCenter);
            UIFactory.SetLayoutElement(actionHeader.gameObject, minWidth: 90, preferredWidth: 100, minHeight: 22, flexibleHeight: 0, flexibleWidth: 0);

            GameObject roomListScroll = UIFactory.CreateScrollView(
                clientPage,
                "RoomListScrollView",
                out roomListContent,
                out roomListScrollbar,
                new Color(0.09f, 0.09f, 0.09f, 1f));
            UIFactory.SetLayoutElement(roomListScroll, minHeight: 180, flexibleHeight: 9999, flexibleWidth: 9999);
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(roomListContent, false, false, true, true, 4, 4, 4, 4, 4, TextAnchor.UpperLeft);

            GameObject nameRow = UIFactory.CreateHorizontalGroup(
                clientPage,
                "NameRow",
                false,
                false,
                true,
                true,
                6,
                new Vector4(6, 4, 6, 4),
                new Color(0.16f, 0.16f, 0.16f, 1f));
            UIFactory.SetLayoutElement(nameRow, minHeight: 30, flexibleHeight: 0);

            Text nameLabel = UIFactory.CreateLabel(nameRow, "NameLabel", "名字", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(nameLabel.gameObject, minWidth: 72, preferredWidth: 80, minHeight: 22, flexibleHeight: 0, flexibleWidth: 0);

            playerNameInput = UIFactory.CreateInputField(nameRow, "PlayerNameInput", "输入名字");
            UIFactory.SetLayoutElement(playerNameInput.GameObject, minHeight: 24, flexibleHeight: 0, flexibleWidth: 9999);

            GameObject ipRow = UIFactory.CreateHorizontalGroup(
                clientPage,
                "ServerIpRow",
                false,
                false,
                true,
                true,
                6,
                new Vector4(6, 4, 6, 4),
                new Color(0.16f, 0.16f, 0.16f, 1f));
            UIFactory.SetLayoutElement(ipRow, minHeight: 30, flexibleHeight: 0);

            Text ipLabel = UIFactory.CreateLabel(ipRow, "ServerIpLabel", "服务器IP", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(ipLabel.gameObject, minWidth: 72, preferredWidth: 80, minHeight: 22, flexibleHeight: 0, flexibleWidth: 0);

            serverIpInput = UIFactory.CreateInputField(ipRow, "ServerIpInput", "127.0.0.1:7777");
            UIFactory.SetLayoutElement(serverIpInput.GameObject, minHeight: 24, flexibleHeight: 0, flexibleWidth: 9999);

            GameObject connectRow = UIFactory.CreateHorizontalGroup(
                clientPage,
                "ConnectRow",
                false,
                false,
                true,
                true,
                6,
                new Vector4(6, 4, 6, 4),
                new Color(0.16f, 0.16f, 0.16f, 1f));
            UIFactory.SetLayoutElement(connectRow, minHeight: 32, flexibleHeight: 0);

            ButtonRef connectButton = UIFactory.CreateButton(connectRow, "ConnectButton", "连接");
            UIFactory.SetLayoutElement(connectButton.Component.gameObject, minWidth: 100, minHeight: 24, flexibleWidth: 9999, flexibleHeight: 0);
            connectButton.SetColor(_theme.ConfirmButtonColor);
            connectButton.OnClick += OnConnectClicked;

            ButtonRef disconnectButton = UIFactory.CreateButton(connectRow, "DisconnectButton", "断开");
            UIFactory.SetLayoutElement(disconnectButton.Component.gameObject, minWidth: 100, minHeight: 24, flexibleWidth: 9999, flexibleHeight: 0);
            disconnectButton.SetColor(_theme.CancelButtonColor);
            disconnectButton.OnClick += OnDisconnectClicked;

            PopulateRoomList();
            SyncConnectionInputs();

            return clientPage;
        }

        private void OnRefreshClicked()
        {
            if (roomListComponent != null)
                roomListComponent.RefreshRooms();

            PopulateRoomList();
        }

        private void PopulateRoomList()
        {
            if (roomListContent == null)
                return;

            for (int i = roomListContent.transform.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.Destroy(roomListContent.transform.GetChild(i).gameObject);
            }

            int rowIndex = 0;
            foreach (RoomListItemViewData roomItem in GetRoomItems())
            {
                CreateRoomListRow(roomItem, rowIndex);
                rowIndex++;
            }

            if (roomListScrollbar != null)
                roomListScrollbar.UpdateSliderHandle();
        }

        private RoomListItemViewData[] GetRoomItems()
        {
            if (roomListComponent == null)
                return MockRoomItems;

            var items = new System.Collections.Generic.List<RoomListItemViewData>();
            foreach (RoomListItemViewData item in roomListComponent.GetRooms())
            {
                items.Add(item);
            }

            if (items.Count == 0)
                return MockRoomItems;

            return items.ToArray();
        }

        private void CreateRoomListRow(RoomListItemViewData item, int rowIndex)
        {
            Color rowColor = rowIndex % 2 == 0
                ? new Color(0.14f, 0.14f, 0.14f, 1f)
                : new Color(0.11f, 0.11f, 0.11f, 1f);

            GameObject row = UIFactory.CreateHorizontalGroup(
                roomListContent,
                "RoomRow_" + rowIndex,
                false,
                false,
                true,
                true,
                6,
                new Vector4(8, 4, 8, 4),
                rowColor,
                TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(row, minHeight: 34, flexibleHeight: 0, flexibleWidth: 9999);

            Text roomNameText = UIFactory.CreateLabel(row, "RoomName", item.RoomName, TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(roomNameText.gameObject, minHeight: 24, flexibleHeight: 0, flexibleWidth: 9999);

            Text playersText = UIFactory.CreateLabel(row, "RoomPlayers", item.PlayerCountText, TextAnchor.MiddleCenter);
            UIFactory.SetLayoutElement(playersText.gameObject, minWidth: 100, preferredWidth: 110, minHeight: 24, flexibleHeight: 0, flexibleWidth: 0);

            ButtonRef joinButton = UIFactory.CreateButton(row, "JoinRoomButton", "加入");
            UIFactory.SetLayoutElement(joinButton.Component.gameObject, minWidth: 90, preferredWidth: 100, minHeight: 24, flexibleWidth: 0, flexibleHeight: 0);
            RuntimeHelper.SetColorBlock(
                joinButton.Component,
                new Color(0.22f, 0.38f, 0.28f),
                new Color(0.26f, 0.44f, 0.32f),
                new Color(0.14f, 0.24f, 0.18f),
                new Color(0.2f, 0.2f, 0.2f));

            joinButton.OnClick += () =>
            {
                OnJoinClicked(item);
            };
        }

        private void OnJoinClicked(RoomListItemViewData item)
        {
            if (roomListComponent != null)
            {
                roomListComponent.JoinRoom(item.RoomName);
                return;
            }

            Plugin.Logger.LogInfo($"Join clicked for room: {item.RoomName}");
        }

        private void OnConnectClicked()
        {
            string playerName = playerNameInput != null ? playerNameInput.Text : string.Empty;
            string serverIp = serverIpInput != null ? serverIpInput.Text : string.Empty;

            if (roomListComponent != null)
            {
                roomListComponent.Connect(playerName, serverIp);
                SyncConnectionInputs();
                return;
            }

            Plugin.Logger.LogInfo("Connect clicked: " + playerName + " @ " + serverIp);
        }

        private void OnDisconnectClicked()
        {
            if (roomListComponent != null)
            {
                roomListComponent.Disconnect();
                return;
            }

            Plugin.Logger.LogInfo("Disconnect clicked.");
        }

        private void SyncConnectionInputs()
        {
            if (playerNameInput == null || serverIpInput == null)
                return;

            if (roomListComponent == null)
            {
                if (string.IsNullOrEmpty(playerNameInput.Text))
                    playerNameInput.Text = "玩家";

                if (string.IsNullOrEmpty(serverIpInput.Text))
                    serverIpInput.Text = "127.0.0.1:7777";

                return;
            }

            playerNameInput.Text = roomListComponent.GetPlayerName() ?? string.Empty;
            serverIpInput.Text = roomListComponent.GetServerIp() ?? string.Empty;
        }
    }
}