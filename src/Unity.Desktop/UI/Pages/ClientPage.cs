using System;
using System.Collections.Generic;
using GOILauncher.Multiplayer;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Extensions;
using GOILauncher.Multiplayer.UI.Components;
using GOILauncher.Multiplayer.UI.Theme;
using GOILauncher.Multiplayer.Unity;
using GOILauncher.Multiplayer.Unity.Config;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;
using UniverseLib.UI.Models;
using UniverseLib.UI.Widgets;

namespace GOILauncher.Multiplayer.UI.Pages
{
    public class ClientPage : IPage
    {
        private const string DefaultPlayerName = "\u73a9\u5bb6";

        private static readonly RoomListItemViewData[] MockRoomItems = new RoomListItemViewData[]
        {
            new RoomListItemViewData("\u65b0\u624b\u4f11\u95f2\u623f", "1/4"),
            new RoomListItemViewData("\u53cc\u4eba\u534f\u4f5c", "2/2"),
            new RoomListItemViewData("\u901f\u901a\u6311\u6218", "3/4"),
            new RoomListItemViewData("\u4e2d\u6587\u4ea4\u6d41\u623f", "2/6"),
            new RoomListItemViewData("\u516c\u5f00\u5927\u5385 #1", "5/8"),
            new RoomListItemViewData("\u516c\u5f00\u5927\u5385 #2", "0/8")
        };

        private readonly IUnityClient _client;
        private readonly MultiplayerSettings _settings;
        private readonly ILogger<ClientPage> _logger;
        private readonly Toast _toast;
        private readonly ITheme _theme = Plugin.Theme;

        private GameObject roomListContent;
        private AutoSliderScrollbar roomListScrollbar;
        private IRoomListUiComponent roomListComponent;
        private InputFieldRef playerNameInput;
        private InputFieldRef serverHostInput;
        private InputFieldRef serverPortInput;
        private ButtonRef connectButton;
        private ButtonRef disconnectButton;
        private ButtonRef refreshButton;
        private bool isConnecting;
        private bool disconnectRequested;
        private string lastServerHost;
        private int lastServerPort;

        public ClientPage(
            IUnityClient unityClient,
            MultiplayerSettings settings,
            ILogger<ClientPage> logger,
            Toast toast)
        {
            _client = unityClient;
            _settings = settings;
            _logger = logger;
            _toast = toast;
            lastServerHost = DefaultServerHost;
            lastServerPort = DefaultServerPort;
            _client.Connected += OnServerConnected;
            _client.Disconnected += OnServerDisconnected;
            _settings.EnabledChanged += OnMultiplayerEnabledChanged;
            _settings.ClientHostChanged += OnDefaultHostChanged;
            _settings.ClientPortChanged += OnDefaultPortChanged;
        }

        public GameObject Root { get; private set; }

        public void SetActive(bool active)
        {
            Root?.SetActive(active);

            if (active)
                RefreshClientState();
        }

        public void Bind(IRoomListUiComponent roomListComponent)
        {
            this.roomListComponent = roomListComponent;
            PopulateRoomList();
            SyncConnectionInputs();
        }

        public void CreateContent(GameObject pagesContainer)
        {
            Root = UIFactory.CreateVerticalGroup(
                pagesContainer,
                "ClientPage",
                false,
                false,
                true,
                true,
                6,
                new Vector4(8, 8, 8, 8),
                new Color(0.12f, 0.12f, 0.12f, 0.95f));
            UIFactory.SetLayoutElement(Root, flexibleHeight: 9999, flexibleWidth: 9999);

            GameObject titleRow = UIFactory.CreateHorizontalGroup(
                Root,
                "ClientTitleRow",
                false,
                false,
                true,
                true,
                6,
                new Vector4(8, 4, 8, 4),
                new Color(0.16f, 0.16f, 0.16f, 1f));
            UIFactory.SetLayoutElement(titleRow, minHeight: 34, flexibleHeight: 0);

            Text roomListTitle = UIFactory.CreateLabel(titleRow, "RoomListTitle", "\u53ef\u7528\u623f\u95f4\u5217\u8868", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(roomListTitle.gameObject, minHeight: 24, flexibleHeight: 0, flexibleWidth: 9999);

            refreshButton = UIFactory.CreateButton(titleRow, "RefreshRoomList", "\u5237\u65b0");
            UIFactory.SetLayoutElement(refreshButton.Component.gameObject, minHeight: 24, minWidth: 80, flexibleWidth: 0, flexibleHeight: 0);
            refreshButton.OnClick += OnRefreshClicked;

            GameObject tableHeader = UIFactory.CreateHorizontalGroup(
                Root,
                "RoomTableHeader",
                false,
                false,
                true,
                true,
                6,
                new Vector4(8, 5, 8, 5),
                new Color(0.2f, 0.2f, 0.2f, 1f));
            UIFactory.SetLayoutElement(tableHeader, minHeight: 32, flexibleHeight: 0);

            Text roomNameHeader = UIFactory.CreateLabel(tableHeader, "RoomNameHeader", "\u623f\u95f4\u540d", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(roomNameHeader.gameObject, minHeight: 22, flexibleHeight: 0, flexibleWidth: 9999);

            Text playerCountHeader = UIFactory.CreateLabel(tableHeader, "PlayerCountHeader", "\u73a9\u5bb6\u6570", TextAnchor.MiddleCenter);
            UIFactory.SetLayoutElement(playerCountHeader.gameObject, minWidth: 100, preferredWidth: 110, minHeight: 22, flexibleHeight: 0, flexibleWidth: 0);

            Text actionHeader = UIFactory.CreateLabel(tableHeader, "ActionHeader", "\u64cd\u4f5c", TextAnchor.MiddleCenter);
            UIFactory.SetLayoutElement(actionHeader.gameObject, minWidth: 90, preferredWidth: 100, minHeight: 22, flexibleHeight: 0, flexibleWidth: 0);

            GameObject roomListScroll = UIFactory.CreateScrollView(
                Root,
                "RoomListScrollView",
                out roomListContent,
                out roomListScrollbar,
                new Color(0.09f, 0.09f, 0.09f, 1f));
            UIFactory.SetLayoutElement(roomListScroll, minHeight: 180, flexibleHeight: 9999, flexibleWidth: 9999);
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(roomListContent, false, false, true, true, 4, 4, 4, 4, 4, TextAnchor.UpperLeft);

            GameObject nameRow = UIFactory.CreateHorizontalGroup(
                Root,
                "NameRow",
                false,
                false,
                true,
                true,
                6,
                new Vector4(6, 4, 6, 4),
                new Color(0.16f, 0.16f, 0.16f, 1f));
            UIFactory.SetLayoutElement(nameRow, minHeight: 30, flexibleHeight: 0);

            Text nameLabel = UIFactory.CreateLabel(nameRow, "NameLabel", "\u540d\u5b57", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(nameLabel.gameObject, minWidth: 72, preferredWidth: 80, minHeight: 22, flexibleHeight: 0, flexibleWidth: 0);

            playerNameInput = UIFactory.CreateInputField(nameRow, "PlayerNameInput", "\u8f93\u5165\u540d\u5b57");
            UIFactory.SetLayoutElement(playerNameInput.GameObject, minHeight: 24, flexibleHeight: 0, flexibleWidth: 9999);

            GameObject addressRow = UIFactory.CreateHorizontalGroup(
                Root,
                "ServerAddressRow",
                false,
                false,
                true,
                true,
                6,
                new Vector4(6, 4, 6, 4),
                new Color(0.16f, 0.16f, 0.16f, 1f));
            UIFactory.SetLayoutElement(addressRow, minHeight: 30, flexibleHeight: 0);

            Text hostLabel = UIFactory.CreateLabel(addressRow, "ServerHostLabel", "\u670d\u52a1\u5668IP", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(hostLabel.gameObject, minWidth: 72, preferredWidth: 80, minHeight: 22, flexibleHeight: 0, flexibleWidth: 0);

            serverHostInput = UIFactory.CreateInputField(addressRow, "ServerHostInput", DefaultServerHost);
            UIFactory.SetLayoutElement(serverHostInput.GameObject, minHeight: 24, flexibleHeight: 0, flexibleWidth: 9999);

            Text portLabel = UIFactory.CreateLabel(addressRow, "ServerPortLabel", "\u7aef\u53e3", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(portLabel.gameObject, minWidth: 40, preferredWidth: 44, minHeight: 22, flexibleHeight: 0, flexibleWidth: 0);

            serverPortInput = UIFactory.CreateInputField(addressRow, "ServerPortInput", InputFieldExtensions.FormatPort(DefaultServerPort));
            serverPortInput.Component.contentType = InputField.ContentType.IntegerNumber;
            UIFactory.SetLayoutElement(serverPortInput.GameObject, minWidth: 90, preferredWidth: 110, minHeight: 24, flexibleHeight: 0, flexibleWidth: 0);

            GameObject connectRow = UIFactory.CreateHorizontalGroup(
                Root,
                "ConnectRow",
                false,
                false,
                true,
                true,
                6,
                new Vector4(6, 4, 6, 4),
                new Color(0.16f, 0.16f, 0.16f, 1f));
            UIFactory.SetLayoutElement(connectRow, minHeight: 32, flexibleHeight: 0);

            connectButton = UIFactory.CreateButton(connectRow, "ConnectButton", "\u8fde\u63a5");
            UIFactory.SetLayoutElement(connectButton.Component.gameObject, minWidth: 100, minHeight: 24, flexibleWidth: 9999, flexibleHeight: 0);
            connectButton.SetColor(_theme.ConfirmButtonColor);
            connectButton.OnClick += OnConnectClicked;

            disconnectButton = UIFactory.CreateButton(connectRow, "DisconnectButton", "\u65ad\u5f00");
            UIFactory.SetLayoutElement(disconnectButton.Component.gameObject, minWidth: 100, minHeight: 24, flexibleWidth: 9999, flexibleHeight: 0);
            disconnectButton.SetColor(_theme.CancelButtonColor);
            disconnectButton.OnClick += OnDisconnectClicked;

            PopulateRoomList();
            SyncConnectionInputs();
            RefreshClientState();
        }

        private void OnRefreshClicked()
        {
            if (!IsMultiplayerEnabled)
            {
                RefreshClientState();
                return;
            }

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

            var items = new List<RoomListItemViewData>();
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

            ButtonRef joinButton = UIFactory.CreateButton(row, "JoinRoomButton", "\u52a0\u5165");
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
            if (!IsMultiplayerEnabled)
                return;

            if (roomListComponent != null)
            {
                roomListComponent.JoinRoom(item.RoomName);
                return;
            }

            Plugin.Logger.LogInfo($"Join clicked for room: {item.RoomName}");
        }

        private void OnConnectClicked()
        {
            if (!IsMultiplayerEnabled || _client == null || isConnecting || _client.IsConnected)
            {
                RefreshClientState();
                return;
            }

            string playerName = GetPlayerName();
            string serverHost = GetServerHost();
            if (!TryGetServerPort(out int serverPort))
                return;

            lastServerHost = serverHost;
            lastServerPort = serverPort;
            isConnecting = true;
            disconnectRequested = false;
            RefreshClientState();

            try
            {
                _client.Connect(serverHost, serverPort, playerName);
                _toast.Show($"\u6b63\u5728\u8fde\u63a5 {serverHost}:{serverPort}");
            }
            catch (Exception ex)
            {
                isConnecting = false;
                _logger.Error(ex, "Failed to connect to server");
                _toast.Show($"\u8fde\u63a5\u5931\u8d25: {ex.Message}");
                RefreshClientState();
            }
        }

        private void OnDisconnectClicked()
        {
            if (!IsMultiplayerEnabled || _client == null || (!isConnecting && !_client.IsConnected))
            {
                RefreshClientState();
                return;
            }

            disconnectRequested = true;

            try
            {
                _client.Disconnect();
            }
            catch (Exception ex)
            {
                disconnectRequested = false;
                _logger.Error(ex, "Failed to disconnect from server");
                _toast.Show($"\u65ad\u5f00\u5931\u8d25: {ex.Message}");
                RefreshClientState();
            }
        }

        private void OnServerConnected()
        {
            if (!IsMultiplayerEnabled)
            {
                _client?.Disconnect();
                RefreshClientState();
                return;
            }

            isConnecting = false;
            disconnectRequested = false;
            _toast.Show($"\u5df2\u8fde\u63a5 {lastServerHost}:{lastServerPort}");
            RefreshClientState();
        }

        private void OnServerDisconnected(string reason)
        {
            bool wasConnecting = isConnecting;
            bool wasDisconnectRequested = disconnectRequested;
            isConnecting = false;
            disconnectRequested = false;

            // Turning the switch off is a deliberate disconnect too; the request just came from
            // MultiplayerLifecycleController instead of this page. Read the switch rather than keep a
            // flag: the setting is written before any listener is told, so this is always the current
            // value and it does not depend on who is notified first.
            if (!IsMultiplayerEnabled)
                _toast.Show("\u8054\u673a\u5df2\u5173\u95ed\uff0c\u8fde\u63a5\u5df2\u65ad\u5f00");
            else if (wasDisconnectRequested)
                _toast.Show("\u5df2\u65ad\u5f00\u8fde\u63a5");
            else if (wasConnecting)
                _toast.Show($"\u8fde\u63a5\u5931\u8d25: {reason}");
            else
                _toast.Show($"\u8fde\u63a5\u5df2\u65ad\u5f00: {reason}");

            RefreshClientState();
        }

        private void OnMultiplayerEnabledChanged(bool enabled)
        {
            RefreshClientState();
        }

        // The settings page owns the defaults. Overwrite the fields only while the connection is idle:
        // during one they show where this connection is actually going, and they are not editable anyway.
        private void OnDefaultHostChanged(string host)
        {
            if (serverHostInput != null && IsConnectionIdle)
                serverHostInput.Text = host;
        }

        private void OnDefaultPortChanged(int port)
        {
            if (serverPortInput != null && IsConnectionIdle)
                serverPortInput.Text = InputFieldExtensions.FormatPort(port);
        }

        private void RefreshClientState()
        {
            if (connectButton == null || disconnectButton == null)
                return;

            bool multiplayerEnabled = IsMultiplayerEnabled;
            bool connected = multiplayerEnabled && _client != null && _client.IsConnected && !isConnecting;
            bool canEditConnection = multiplayerEnabled && !isConnecting && !connected;

            connectButton.Component.interactable = canEditConnection;
            disconnectButton.Component.interactable = multiplayerEnabled && (isConnecting || connected);
            if (refreshButton != null)
                refreshButton.Component.interactable = multiplayerEnabled;
            SetInputInteractable(playerNameInput, canEditConnection);
            SetInputInteractable(serverHostInput, canEditConnection);
            SetInputInteractable(serverPortInput, canEditConnection);
        }

        private void SyncConnectionInputs()
        {
            if (playerNameInput == null || serverHostInput == null || serverPortInput == null)
                return;

            if (string.IsNullOrEmpty(playerNameInput.Text))
                playerNameInput.Text = DefaultPlayerName;

            if (string.IsNullOrEmpty(serverHostInput.Text))
                serverHostInput.Text = DefaultServerHost;

            if (string.IsNullOrEmpty(serverPortInput.Text))
                serverPortInput.Text = InputFieldExtensions.FormatPort(DefaultServerPort);
        }

        private string GetPlayerName()
        {
            string playerName = playerNameInput != null ? playerNameInput.Text?.Trim() : string.Empty;
            if (string.IsNullOrWhiteSpace(playerName))
                playerName = DefaultPlayerName;

            if (playerNameInput != null)
                playerNameInput.Text = playerName;

            return playerName;
        }

        private string GetServerHost()
        {
            string serverHost = serverHostInput != null ? serverHostInput.Text?.Trim() : string.Empty;
            if (string.IsNullOrWhiteSpace(serverHost))
                serverHost = DefaultServerHost;

            if (serverHostInput != null)
                serverHostInput.Text = serverHost;

            return serverHost;
        }

        private bool TryGetServerPort(out int serverPort)
        {
            if (serverPortInput.TryReadPort(out serverPort))
                return true;

            _toast.Show(InputFieldExtensions.InvalidPortMessage);
            return false;
        }

        /// <summary>The host the page connects to when nothing else was typed, owned by the settings page.</summary>
        private string DefaultServerHost
        {
            get { return _settings == null ? MultiplayerSettings.DefaultClientHost : _settings.ClientHost; }
        }

        /// <summary>The port the page connects to when nothing else was typed, owned by the settings page.</summary>
        private int DefaultServerPort
        {
            get { return _settings == null ? MultiplayerSettings.DefaultClientPort : _settings.ClientPort; }
        }

        // Nothing is in flight, so the address fields describe an intent rather than a live connection.
        private bool IsConnectionIdle
        {
            get { return !isConnecting && (_client == null || !_client.IsConnected); }
        }

        private static void SetInputInteractable(InputFieldRef input, bool interactable)
        {
            if (input != null)
                input.Component.interactable = interactable;
        }

        private bool IsMultiplayerEnabled
        {
            get { return _settings == null || _settings.Enabled; }
        }
    }
}
