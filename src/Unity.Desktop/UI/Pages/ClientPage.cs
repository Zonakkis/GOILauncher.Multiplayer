using System;
using System.Collections.Generic;
using GOILauncher.Multiplayer;
using GOILauncher.Multiplayer.Extensions;
using GOILauncher.Multiplayer.Core.Data.Models;
using System.Linq;
using GOILauncher.Multiplayer.UI.Theme;
using GOILauncher.Multiplayer.Unity;
using GOILauncher.Multiplayer.UI.Config;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace GOILauncher.Multiplayer.UI.Pages
{
    /// <summary>
    /// 客户端页。结构分两段，从上到下是"房间列表 → 连接"：
    ///
    /// - 目录段是主要工作区，占到全部剩余高度；
    /// - 连接段是低频操作，放底部固定高度。
    ///
    /// 所有底色、间距、列宽都走 <see cref="Layout"/> 和 <see cref="UiKit"/>，
    /// 不在本文件里写 <c>new Color(...)</c>——散落的字面量改不动，等于没有主题。
    /// </summary>
    public class ClientPage : IPage
    {
        // 门面每轮联机都是新造的，所以它不是 readonly：Bind 时才有，Unbind 时清空。
        private IUnityClient _client;
        private readonly MultiplayerSettings _settings;
        private readonly Toast _toast;
        private readonly RoomDialogUI _roomDialog;

        private GameObject roomListContent;
        private readonly List<RoomRow> roomRows = new List<RoomRow>();
        private ButtonRef createRoomButton, editRoomButton, refreshButton;
        private Text roomListEmptyText;
        private int joiningRoomId;
        private InputFieldRef playerNameInput;
        private InputFieldRef serverHostInput;
        private InputFieldRef serverPortInput;
        private ButtonRef connectButton;
        private ButtonRef disconnectButton;
        private bool isConnecting;
        private bool disconnectRequested;
        private string lastServerHost;
        private int lastServerPort;

        public ClientPage(MultiplayerSettings settings, Toast toast, RoomDialogUI roomDialog)
        {
            _settings = settings;
            _toast = toast;
            _roomDialog = roomDialog;
            _roomDialog.ActiveChanged += active => RefreshClientState();
            lastServerHost = DefaultServerHost;
            lastServerPort = DefaultServerPort;
            _settings.PlayerNameChanged += OnDefaultNameChanged;
            _settings.ClientHostChanged += OnDefaultHostChanged;
            _settings.ClientPortChanged += OnDefaultPortChanged;
        }

        /// <summary>
        /// 绑定这一轮的客户端门面。页面在联机没加载时也要能构造（UniverseLib 的窗口按 id 注册，
        /// 建不出第二个），所以门面不是构造参数，而是一次周期开始时才递进来。
        /// <para>
        /// 订阅和退订必须成对、只在这一对方法里做：漏一次 -= 不会报错，只会让下一次点按钮响应两遍。
        /// </para>
        /// </summary>
        public void Bind(IUnityClient client)
        {
            if (client == null || _client != null)
                throw new InvalidOperationException("ClientPage: Bind and Unbind must alternate.");

            _client = client;
            _client.RoomListUpdated += OnRoomsUpdated;
            _client.CurrentRoomChanged += OnCurrentRoomChanged;
            _client.RoomOperationCompleted += OnRoomOperationCompleted;
            _client.Connected += OnServerConnected;
            _client.Disconnected += OnServerDisconnected;
            PopulateRoomList();
            RefreshClientState();
        }

        public void Unbind()
        {
            if (_client == null)
                return;

            _client.RoomListUpdated -= OnRoomsUpdated;
            _client.CurrentRoomChanged -= OnCurrentRoomChanged;
            _client.RoomOperationCompleted -= OnRoomOperationCompleted;
            _client.Connected -= OnServerConnected;
            _client.Disconnected -= OnServerDisconnected;
            _client = null;

            // 关闭时是先退订再断线，所以这一页收不到 Disconnected 通知，得自己把状态归零：
            // 不然"正在连接"和上一轮的目录会一直留在屏幕上。
            isConnecting = false;
            disconnectRequested = false;
            joiningRoomId = 0;
            PopulateRoomList();
            RefreshClientState();
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
                pagesContainer, "ClientPage", false, false, true, true,
                Layout.SpaceSm,
                new Vector4(Layout.SpaceMd, Layout.SpaceMd, Layout.SpaceMd, Layout.SpaceMd),
                Plugin.Theme.SurfaceBase);
            UIFactory.SetLayoutElement(Root, flexibleHeight: 9999, flexibleWidth: 9999);

            CreateRoomDirectorySection();
            CreateConnectionSection();

            PopulateRoomList();
            SyncConnectionInputs();
            RefreshClientState();
        }

        #region 房间列表

        private void CreateRoomDirectorySection()
        {
            GameObject section = UIFactory.CreateVerticalGroup(
                Root, "RoomDirectorySection", false, false, true, true,
                0, new Vector4(0, 0, 0, 0), Plugin.Theme.SurfaceBase);
            UIFactory.SetLayoutElement(section, minHeight: 200, flexibleHeight: 9999, flexibleWidth: 9999);

            // 三个动作都只对"已入房"有意义，所以和它们作用的对象（目录）放在同一个区块头里，
            // 不再和连接按钮挤在一起。
            GameObject header = UiKit.CreateSectionHeader(section, "RoomDirectoryHeader", "房间列表");
            createRoomButton = UiKit.CreateButton(header, "CreateRoom", "创建房间", 96);
            createRoomButton.SetConfirm();
            createRoomButton.OnClick += () => _roomDialog.ShowCreate();

            editRoomButton = UiKit.CreateButton(header, "EditRoom", "房间设置", 88);
            editRoomButton.OnClick += () => _roomDialog.ShowEdit();

            refreshButton = UiKit.CreateButton(header, "RefreshRoomList", "刷新", 64);
            refreshButton.OnClick += OnRefreshClicked;

            CreateRoomTable(section);
        }

        /// <summary>
        /// 表头 + 分隔线 + 数据区。列宽全部来自 <see cref="Layout.RoomColumns"/>，
        /// 数据行也引用同一份常量——两边各写一遍数字正是错位的来源。
        /// </summary>
        private void CreateRoomTable(GameObject parent)
        {
            const int padding = Layout.RoomColumns.RowPadding;
            int gutter = Layout.SpaceXs;

            GameObject tableHeader = UIFactory.CreateHorizontalGroup(
                parent, "RoomTableHeader", false, false, true, true, Layout.SpaceSm,
                // UIFactory 把这个 Vector4 原样按 (top, bottom, left, right) 的顺序塞进 LayoutGroup，
                // 不是直觉上的 (left, top, right, bottom)——写反了上下会多留出一整行的高度，
                // 单元格被挤出表头背景之外，看着就像字被切掉了。
                // 上下各留 4（正好是 Layout.TableHeaderHeight 里扣掉的那份）；
                // 左右 12 = 数据行自己的 8 + 滚动内容的 4，两边对得上列才不歪。
                new Vector4(Layout.SpaceXs, Layout.SpaceXs, padding + gutter, padding + gutter),
                Plugin.Theme.SurfaceRaised, TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(tableHeader, minHeight: Layout.TableHeaderHeight,
                preferredHeight: Layout.TableHeaderHeight, flexibleHeight: 0, flexibleWidth: 9999);

            const TextAnchor left = TextAnchor.MiddleLeft;
            const TextAnchor center = TextAnchor.MiddleCenter;
            Color headerColor = Plugin.Theme.TextSecondary;
            int headerFont = Layout.FontTableHeader;

            UiKit.CreateTextCell(tableHeader, "RoomNameHeader", "房间名", null, left, headerColor, headerFont);
            UiKit.CreateTextCell(tableHeader, "RoomStatusHeader", "状态", Layout.RoomColumns.StatusWidth, center, headerColor, headerFont);
            UiKit.CreateTextCell(tableHeader, "RoomCountHeader", "人数", Layout.RoomColumns.CountWidth, center, headerColor, headerFont);
            UiKit.CreateTextCell(tableHeader, "RoomActionHeader", string.Empty, Layout.RoomColumns.ActionWidth, center, headerColor, headerFont);

            UiKit.CreateDivider(parent, Plugin.Theme.DividerColor);

            GameObject scroll = UIFactory.CreateScrollView(
                parent, "RoomListScrollView", out roomListContent, out var scrollbar,
                Plugin.Theme.SurfaceSunken);
            // 保留滚轮/拖动，但隐藏滑块并收回它占的那条槽。
            scrollbar.UIRoot.SetActive(false);
            scrollbar.ViewportRect.offsetMax = Vector2.zero;
            UIFactory.SetLayoutElement(scroll, minHeight: 160, flexibleHeight: 9999, flexibleWidth: 9999);
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(roomListContent, false, false, true, true,
                gutter, gutter, gutter, gutter, gutter, TextAnchor.UpperLeft);

            roomListEmptyText = UIFactory.CreateLabel(roomListContent, "EmptyRooms", string.Empty,
                TextAnchor.MiddleCenter, Plugin.Theme.TextSecondary, false, Layout.FontDetail);
            UIFactory.SetLayoutElement(roomListEmptyText.gameObject, minHeight: 60, flexibleWidth: 9999);
            roomListEmptyText.gameObject.SetActive(false);
        }

        private void PopulateRoomList()
        {
            if (roomListContent == null) return;

            for (int i = roomRows.Count - 1; i >= 0; i--)
            {
                if (roomRows[i].Root != null)
                {
                    roomRows[i].Root.SetActive(false);
                    UnityEngine.Object.Destroy(roomRows[i].Root);
                }
            }
            roomRows.Clear();

            // 没加载时目录就是空的：上一轮的行已经清掉了，这里没有可读的来源。
            if (_client == null)
                return;

            // 目录里永远有大厅，所以"快照已到但列表为空"只可能是服务端异常，
            // 文案按真实的空来写，不用"正在等待"掩饰。
            foreach (var room in _client.Rooms)
                roomRows.Add(CreateRoomRow(room));
        }

        private RoomRow CreateRoomRow(RoomInfo room)
        {
            const int padding = Layout.RoomColumns.RowPadding;

            GameObject row = UIFactory.CreateHorizontalGroup(
                roomListContent, "Room_" + room.Id, false, false, true, true, Layout.SpaceSm,
                // 上下都不留：行高 30 比单元格高，垂直居中会自己分出相等的余量。
                // 写成 (padding, 0, padding, 0) 看着像"左右 8、上下 0"，但 UIFactory 取的是
                // (top, bottom, left, right)，实际得到上 8 下 0——上间距比下间距大就是这么来的。
                new Vector4(0, 0, padding, padding), Plugin.Theme.SurfaceBase, TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(row, minHeight: Layout.RoomRowHeight, preferredHeight: Layout.RoomRowHeight,
                flexibleHeight: 0, flexibleWidth: 9999);

            Text name = UiKit.CreateTextCell(row, "Name", room.Name, null, TextAnchor.MiddleLeft,
                Plugin.Theme.TextPrimary);
            Text status = UiKit.CreateTextCell(row, "Status", string.Empty, Layout.RoomColumns.StatusWidth,
                TextAnchor.MiddleCenter, Plugin.Theme.TextSecondary, Layout.FontDetail);
            Text count = UiKit.CreateTextCell(row, "Players", FormatCount(room), Layout.RoomColumns.CountWidth,
                TextAnchor.MiddleCenter, Plugin.Theme.TextSecondary);

            // 按钮外面套一格固定宽度的容器：当前房间和已满的房间会把按钮藏掉，
            // 但格子必须留着，否则那一行的房名列会吃掉空位、后面几列和别人对不齐。
            GameObject actionCell = UIFactory.CreateHorizontalGroup(
                row, "ActionCell", true, true, true, true, 0,
                new Vector4(0, 0, 0, 0), Color.clear);
            UIFactory.SetLayoutElement(actionCell, minWidth: Layout.RoomColumns.ActionWidth,
                preferredWidth: Layout.RoomColumns.ActionWidth, minHeight: 20, flexibleHeight: 0, flexibleWidth: 0);

            ButtonRef join = UiKit.CreateConfirmButton(actionCell, "Join", "加入");
            join.OnClick += () => OnJoinClicked(room);

            return new RoomRow
            {
                Root = row,
                Name = name,
                Status = status,
                Count = count,
                Join = join,
                Room = room
            };
        }

        private static string FormatCount(RoomInfo room)
        {
            return room.PlayerCount + " / " + (room.MaxPlayers == 0 ? "∞" : room.MaxPlayers.ToString());
        }

        /// <summary>
        /// 刷新每一行的状态、文字与可点性。
        ///
        /// 四种状态只用一种视觉手段表达是不够的（以前"已满"和"当前房间"都是同一个灰按钮），
        /// 所以拆开：状态列用颜色说明"这是什么"，操作列只负责"能不能点"。
        /// 优先级是 当前房间 &gt; 已满 &gt; 有密码——已满解释了为什么进不去，比有没有密码更该被看到。
        /// </summary>
        private void RefreshRoomRows()
        {
            if (_client == null)
                return;

            var current = _client.CurrentRoom;
            bool canOperate = CanOperateRooms;

            foreach (RoomRow row in roomRows)
            {
                bool isCurrent = current != null && row.Room.Id == current.Id;

                if (isCurrent)
                {
                    row.Status.text = "当前房间";
                    row.Status.color = Plugin.Theme.TextAccent;
                    row.Name.color = Plugin.Theme.TextAccent;
                    row.Root.GetComponent<Image>().color = Plugin.Theme.SurfaceRaised;
                }
                else if (row.Room.IsFull)
                {
                    row.Status.text = "已满";
                    row.Status.color = Plugin.Theme.TextSecondary;
                    row.Name.color = Plugin.Theme.TextPrimary;
                    row.Root.GetComponent<Image>().color = Plugin.Theme.SurfaceBase;
                }
                else if (row.Room.HasPassword)
                {
                    row.Status.text = "有密码";
                    row.Status.color = Plugin.Theme.WarningChipColor;
                    row.Name.color = Plugin.Theme.TextPrimary;
                    row.Root.GetComponent<Image>().color = Plugin.Theme.SurfaceBase;
                }
                else
                {
                    row.Status.text = string.Empty;
                    row.Name.color = Plugin.Theme.TextPrimary;
                    row.Root.GetComponent<Image>().color = Plugin.Theme.SurfaceBase;
                }

                row.Count.text = FormatCount(row.Room);
                // 状态列已经把"当前房间""已满"说清楚了，按钮只留能点的那一种。
                row.Join.GameObject.SetActive(!isCurrent && !row.Room.IsFull);
                row.Join.Component.interactable = canOperate;
            }
        }

        private void RefreshEmptyState()
        {
            if (roomListEmptyText == null) return;

            string message = DescribeEmptyRoomList();
            roomListEmptyText.gameObject.SetActive(!string.IsNullOrEmpty(message));
            roomListEmptyText.text = message ?? string.Empty;
        }

        /// <summary>
        /// 目录为空时该显示哪一句话。
        ///
        /// 这里问的不是"列表空了"，而是"用户现在该做什么"。同样是空列表，联机没开、
        /// 还没连上、连上了但首份房间快照没到，是三件完全不同的事；给同一句"暂无房间"
        /// 会让人以为服务器上真的没有房间，然后反复点刷新。
        ///
        /// 注意判据是业务就绪（首份房间快照，即 <see cref="IUnityClient.CurrentRoom"/> 非空），
        /// 不是 socket 的 IsConnected——见 docs/agent/room-system.md 的连接与入房流程。
        /// 另外目录里永远有大厅，所以真的走到最后一种情形只可能是服务端异常。
        ///
        /// 返回 null 或空串表示不显示这一行。
        /// </summary>
        private string DescribeEmptyRoomList()
        {
            if (!IsLoaded)
                return "联机未启用";
            else if (!_client.IsConnected)
                return "未连接到服务器";
            return null;
        }

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

        private void OnJoinClicked(RoomInfo room)
        {
            if (!CanOperateRooms) return;
            joiningRoomId = room.Id;
            if (room.HasPassword) _roomDialog.ShowJoin(room);
            else _client.JoinRoom(room.Id, null);
            RefreshClientState();
        }

        #endregion

        #region 连接

        private void CreateConnectionSection()
        {
            GameObject section = UIFactory.CreateVerticalGroup(
                Root, "ConnectionSection", false, false, true, true,
                0, new Vector4(0, 0, 0, 0), Plugin.Theme.SurfaceBase);
            UIFactory.SetLayoutElement(section, minHeight: 0, flexibleHeight: 0, flexibleWidth: 9999);

            UiKit.CreateSectionHeader(section, "ConnectionHeader", "连接");

            GameObject nameRow = UiKit.CreateFieldRow(section, "NameRow");
            UiKit.CreateFieldLabel(nameRow, "NameLabel", "名字");
            playerNameInput = UiKit.CreateInputField(nameRow, "PlayerNameInput", "联机时显示的名字");
            // 名字允许为空——那是"还没填"，不是错误，所以不显示占位提示。
            playerNameInput.HidePlaceholder();
            playerNameInput.Text = _settings.PlayerName;

            GameObject addressRow = UiKit.CreateFieldRow(section, "ServerAddressRow");
            UiKit.CreateFieldLabel(addressRow, "ServerHostLabel", "服务器地址");
            serverHostInput = UiKit.CreateInputField(addressRow, "ServerHostInput", DefaultServerHost);

            UiKit.CreateFieldLabel(addressRow, "ServerPortLabel", "端口", 36);
            serverPortInput = UiKit.CreateInputField(addressRow, "ServerPortInput",
                InputFieldExtensions.FormatPort(DefaultServerPort), 96);
            serverPortInput.Component.contentType = InputField.ContentType.IntegerNumber;

            GameObject actionRow = UiKit.CreateFieldRow(section, "ConnectionActionRow", Layout.PrimaryButtonHeight + Layout.SpaceMd);
            connectButton = UiKit.CreateConfirmButton(actionRow, "ConnectButton", "连接");
            connectButton.OnClick += OnConnectClicked;
            disconnectButton = UiKit.CreateCancelButton(actionRow, "DisconnectButton", "断开");
            disconnectButton.OnClick += OnDisconnectClicked;
        }

        #endregion

        #region 连接状态

        private bool CanOperateRooms => IsLoaded && _client.IsConnected && _client.CurrentRoom != null
            && !_client.IsRoomOperationPending && !_roomDialog.Enabled;

        private void OnConnectClicked()
        {
            if (!IsLoaded || isConnecting || _client.IsConnected)
            {
                RefreshClientState();
                return;
            }

            string playerName = GetPlayerName();
            if (string.IsNullOrWhiteSpace(playerName))
            {
                _toast.Show("名字不能为空");
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
                _toast.Show($"正在连接 {serverHost}:{serverPort}");
            }
            catch (Exception ex)
            {
                isConnecting = false;
                Plugin.Logger.LogError("Failed to connect to server: " + ex);
                _toast.Show($"连接失败: {ex.Message}");
                RefreshClientState();
            }
        }

        private void OnDisconnectClicked()
        {
            if (!IsLoaded || (!isConnecting && !_client.IsConnected))
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
                Plugin.Logger.LogError("Failed to disconnect from server: " + ex);
                _toast.Show($"断开失败: {ex.Message}");
                RefreshClientState();
            }
        }

        private void OnServerConnected()
        {
            // 这里不需要再问一次"联机还开着吗"：能收到这个通知说明门面还绑着，
            // 而关闭时先退订再断线，通知根本不会到达。
            isConnecting = false;
            disconnectRequested = false;
            _toast.Show($"已连接 {lastServerHost}:{lastServerPort}");
            RefreshClientState();
        }

        private void OnServerDisconnected(string reason)
        {
            bool wasConnecting = isConnecting;
            bool wasDisconnectRequested = disconnectRequested;
            isConnecting = false;
            disconnectRequested = false;

            // 关闭联机不会走到这里：Unbind 先退订，Disconnect 后发生，通知发不出去。
            // 所以剩下的三种情况都是真实的连接事件，不用再读一个开关来分辨意图。
            if (wasDisconnectRequested)
                _toast.Show("已断开连接");
            else if (wasConnecting)
                _toast.Show($"连接失败: {reason}");
            else
                _toast.Show($"连接已断开: {reason}");

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

            bool loaded = IsLoaded;
            bool connected = loaded && _client.IsConnected && !isConnecting;
            bool canEditConnection = loaded && !isConnecting && !connected;

            connectButton.Component.interactable = canEditConnection;
            disconnectButton.Component.interactable = loaded && (isConnecting || connected);

            bool canOperate = CanOperateRooms;
            if (refreshButton != null) refreshButton.Component.interactable = canOperate;
            if (createRoomButton != null) createRoomButton.Component.interactable = canOperate;

            var room = _client == null ? null : _client.CurrentRoom;
            if (editRoomButton != null)
            {
                editRoomButton.GameObject.SetActive(room != null && !room.IsLobby && room.OwnerPlayerId == _client.LocalPlayer.Id);
                editRoomButton.Component.interactable = canOperate;
            }

            RefreshRoomRows();
            RefreshEmptyState();

            // 连接建立后字段就该只读而不是变灰：灰掉的文字看着像被禁用/出错了，
            // 但这里的事实是"它正连着哪儿"，仍然值得看清楚。
            SetConnectionInputReadOnly(!canEditConnection);
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
            get { return _settings.ClientHost; }
        }

        /// <summary>The port the page connects to when nothing else was typed, owned by the settings page.</summary>
        private int DefaultServerPort
        {
            get { return _settings.ClientPort; }
        }

        // Nothing is in flight, so the address fields describe an intent rather than a live connection.
        private bool IsConnectionIdle
        {
            get { return !isConnecting && (_client == null || !_client.IsConnected); }
        }

        private void SetConnectionInputReadOnly(bool readOnly)
        {
            // 三个都判空：这个方法在页面构造完成前就可能被状态刷新调到，
            // 只守住第一个等于没守——后面两个照样会空引用。
            if (playerNameInput != null)
                playerNameInput.SetReadOnly(readOnly);
            if (serverHostInput != null)
                serverHostInput.SetReadOnly(readOnly);
            if (serverPortInput != null)
                serverPortInput.SetReadOnly(readOnly);
        }
        /// <summary>这一轮的门面在不在。不在就是联机没加载，这一页只能看不能操作。</summary>
        private bool IsLoaded
        {
            get { return _client != null; }
        }

        #endregion

        /// <summary>目录里的一行。状态、文字都要在刷新时改，所以整行的引用留着。</summary>
        private sealed class RoomRow
        {
            public GameObject Root;
            public Text Name;
            public Text Status;
            public Text Count;
            public ButtonRef Join;
            public RoomInfo Room;
        }
    }
}