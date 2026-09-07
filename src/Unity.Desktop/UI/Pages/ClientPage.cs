using System;
using System.Collections.Generic;
using GOILauncher.Multiplayer;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Extensions;
using GOILauncher.Multiplayer.Core.Data.Models;
using System.Linq;
using GOILauncher.Multiplayer.UI.Theme;
using GOILauncher.Multiplayer.Unity;
using GOILauncher.Multiplayer.Unity.Config;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace GOILauncher.Multiplayer.UI.Pages
{
    public class ClientPage : IPage
    {
        private readonly IUnityClient _client;
        private readonly MultiplayerSettings _settings;
        private readonly ILogger<ClientPage> _logger;
        private readonly Toast _toast;
        private readonly ITheme _theme = Plugin.Theme;

        private GameObject roomListContent;
        private readonly RoomDialogUI _roomDialog;
        private readonly List<KeyValuePair<RoomInfo, ButtonRef>> roomButtons = new List<KeyValuePair<RoomInfo, ButtonRef>>();
        private ButtonRef createRoomButton, editRoomButton;
        private int joiningRoomId;
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
            Toast toast,
            RoomDialogUI roomDialog)
        {
            _client = unityClient;
            _settings = settings;
            _logger = logger;
            _toast = toast;
            _roomDialog = roomDialog;
            _roomDialog.ActiveChanged += active => RefreshClientState();
            _client.RoomListUpdated += OnRoomsUpdated;
            _client.CurrentRoomChanged += OnCurrentRoomChanged;
            _client.RoomOperationCompleted += OnRoomOperationCompleted;
            lastServerHost = DefaultServerHost;
            lastServerPort = DefaultServerPort;
            _client.Connected += OnServerConnected;
            _client.Disconnected += OnServerDisconnected;
            _settings.EnabledChanged += OnMultiplayerEnabledChanged;
            _settings.PlayerNameChanged += OnDefaultNameChanged;
            _settings.ClientHostChanged += OnDefaultHostChanged;
            _settings.ClientPortChanged += OnDefaultPortChanged;
        }

        public GameObject Root { get; private set; }

        public void SetActive(bool active)
        {
            Root?.SetActive(active);
            if (!active) _roomDialog.SetActive(false);

            if (active)
                RefreshClientState();
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

            Text roomListTitle = UIFactory.CreateLabel(titleRow, "RoomListTitle", "\u623f\u95f4\u5217\u8868", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(roomListTitle.gameObject, minHeight: 24, flexibleHeight: 0, flexibleWidth: 9999);

            editRoomButton = UIFactory.CreateButton(titleRow, "EditRoom", "房间设置");
            UIFactory.SetLayoutElement(editRoomButton.Component.gameObject, minHeight: 24, minWidth: 96, flexibleWidth: 0);
            editRoomButton.OnClick += () => _roomDialog.ShowEdit();

            createRoomButton = UIFactory.CreateButton(titleRow, "CreateRoom", "创建房间");
            UIFactory.SetLayoutElement(createRoomButton.Component.gameObject, minHeight: 24, minWidth: 96, flexibleWidth: 0);
            createRoomButton.OnClick += () => _roomDialog.ShowCreate();

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
                // Match the rows' 8px padding plus the scroll content's 4px padding.
                new Vector4(12, 5, 12, 5),
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
                out var roomListScrollbar,
                new Color(0.09f, 0.09f, 0.09f, 1f));
            // Keep wheel/drag scrolling, but hide the slider and reclaim its viewport gutter.
            roomListScrollbar.UIRoot.SetActive(false);
            roomListScrollbar.ViewportRect.offsetMax = Vector2.zero;
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

            playerNameInput = UIFactory.CreateInputField(nameRow, "PlayerNameInput", string.Empty);
            playerNameInput.HidePlaceholder();
            playerNameInput.Text = _settings.PlayerName;
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

        private bool CanOperateRooms => IsMultiplayerEnabled && _client.IsConnected && _client.CurrentRoom != null
            && !_client.IsRoomOperationPending && !_roomDialog.Enabled;

        private void OnRefreshClicked()
        {
            if (!CanOperateRooms) return;
            _client.RefreshRooms();
            RefreshClientState();
        }
        private void OnRoomsUpdated() { PopulateRoomList(); RefreshClientState(); }
        private void OnCurrentRoomChanged() { RefreshClientState(); }
        private void OnRoomOperationCompleted(RoomOperationResult result)
        {
            RefreshClientState();
            if (result.IsSuccess) return;
            if (result.Operation == RoomOperation.Join && result.Error == RoomError.IncorrectPassword && !_roomDialog.Enabled)
            {
                var target = _client.Rooms.FirstOrDefault(r => r.Id == joiningRoomId);
                if (target != null) { _roomDialog.ShowJoin(target); return; }
            }
            if (!_roomDialog.Enabled) _toast.Show(RoomUiText.Error(result.Error));
        }
        private void PopulateRoomList()
        {
            if (roomListContent == null) return;
            for (int i = roomListContent.transform.childCount - 1; i >= 0; i--)
            {
                var row = roomListContent.transform.GetChild(i).gameObject;
                row.SetActive(false);
                UnityEngine.Object.Destroy(row);
            }
            roomButtons.Clear();
            int rowIndex = 0;
            foreach (var room in _client.Rooms) CreateRoomListRow(room, rowIndex++);
            if (rowIndex == 0)
            {
                var empty = UIFactory.CreateLabel(roomListContent, "EmptyRooms", _client.IsConnected ? "正在等待服务器房间信息…" : "连接服务器后显示房间", TextAnchor.MiddleCenter);
                UIFactory.SetLayoutElement(empty.gameObject, minHeight: 40, flexibleWidth: 9999);
            }
        }
        private void CreateRoomListRow(RoomInfo room, int rowIndex)
        {
            // Equal cell heights keep short numeric labels on the same line as names and buttons.
            const int cellHeight = 26;
            var row = UIFactory.CreateHorizontalGroup(roomListContent, "Room_" + room.Id, false, false, true, true,
                6, new Vector4(8, 4, 8, 4), rowIndex % 2 == 0 ? new Color(0.14f, 0.14f, 0.14f) : new Color(0.11f, 0.11f, 0.11f),
                TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(row, minHeight: 34, flexibleWidth: 9999, flexibleHeight: 0);
            var name = UIFactory.CreateLabel(row, "Name", room.Name, TextAnchor.MiddleLeft);
            name.supportRichText = false;
            UIFactory.SetLayoutElement(name.gameObject, minHeight: cellHeight, flexibleHeight: 0, flexibleWidth: 9999);
            var count = UIFactory.CreateLabel(row, "Players", room.PlayerCount + "/" + (room.MaxPlayers == 0 ? "∞" : room.MaxPlayers.ToString()), TextAnchor.MiddleCenter);
            UIFactory.SetLayoutElement(count.gameObject, minWidth: 100, preferredWidth: 110, minHeight: cellHeight, flexibleHeight: 0, flexibleWidth: 0);
            var join = UIFactory.CreateButton(row, "Join", "加入");
            UIFactory.SetLayoutElement(join.Component.gameObject, minWidth: 90, preferredWidth: 100, minHeight: cellHeight, flexibleHeight: 0, flexibleWidth: 0);
            join.SetConfirm();
            join.OnClick += () => OnJoinClicked(room);
            roomButtons.Add(new KeyValuePair<RoomInfo, ButtonRef>(room, join));
        }
        private void OnJoinClicked(RoomInfo room)
        {
            if (!CanOperateRooms) return;
            joiningRoomId = room.Id;
            if (room.HasPassword) _roomDialog.ShowJoin(room);
            else _client.JoinRoom(room.Id, null);
            RefreshClientState();
        }

        private void OnConnectClicked()
        {
            if (!IsMultiplayerEnabled || _client == null || isConnecting || _client.IsConnected)
            {
                RefreshClientState();
                return;
            }

            string playerName = GetPlayerName();
            if (string.IsNullOrWhiteSpace(playerName))
            {
                _toast.Show("\u540d\u5b57\u4e0d\u80fd\u4e3a\u7a7a");
                RefreshClientState();
                return;
            }

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
        private void OnDefaultNameChanged(string name)
        {
            if (playerNameInput != null && IsConnectionIdle)
                playerNameInput.Text = name;
        }

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
            var room = _client.CurrentRoom;
            if (refreshButton != null) refreshButton.Component.interactable = CanOperateRooms;
            if (createRoomButton != null) createRoomButton.Component.interactable = CanOperateRooms;
            if (editRoomButton != null)
            {
                editRoomButton.Component.gameObject.SetActive(room != null && !room.IsLobby && room.OwnerPlayerId == _client.LocalPlayer.Id);
                editRoomButton.Component.interactable = CanOperateRooms;
            }
            foreach (var entry in roomButtons)
            {
                bool current = room != null && entry.Key.Id == room.Id;
                entry.Value.ButtonText.text = current ? "当前房间" : entry.Key.IsFull ? "已满" : "加入";
                entry.Value.Component.interactable = CanOperateRooms && !current && !entry.Key.IsFull;
            }
            SetInputInteractable(playerNameInput, canEditConnection);
            SetInputInteractable(serverHostInput, canEditConnection);
            SetInputInteractable(serverPortInput, canEditConnection);
        }

        private void SyncConnectionInputs()
        {
            if (serverHostInput == null || serverPortInput == null)
                return;

            if (string.IsNullOrEmpty(serverHostInput.Text))
                serverHostInput.Text = DefaultServerHost;

            if (string.IsNullOrEmpty(serverPortInput.Text))
                serverPortInput.Text = InputFieldExtensions.FormatPort(DefaultServerPort);
        }

        private string GetPlayerName()
        {
            // Blank stays blank here; OnConnectClicked refuses to connect with one.
            return playerNameInput != null ? playerNameInput.Text?.Trim() : string.Empty;
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
