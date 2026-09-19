using GOILauncher.Multiplayer.UI.ScrollView.Player;
using UniverseLib.UI;
using UniverseLib.UI.Panels;
using UnityEngine;
using UnityEngine.UI;
using GOILauncher.Multiplayer.Unity;

namespace GOILauncher.Multiplayer.UI
{
    public class PlayerListUI : PanelBase
    {
        private static readonly Color PanelBackgroundColor = new Color(0f, 0f, 0f, 0.35f);
        private static readonly Color HeaderBackgroundColor = new Color(0f, 0f, 0f, 0.42f);

        // 距离刷新节流：10 Hz 肉眼已经看不出和逐帧的差别，但省掉了大部分字符串分配。
        private const float DistanceRefreshInterval = 0.1f;

        private readonly PlayerListHandler playerListHandler;
        private readonly IUnityClient client;

        private Text currentRoomText;
        private Text roomOwnerText;
        private float _distanceTimer;

        public PlayerListUI(UIBase owner, PlayerListHandler playerListHandler, IUnityClient client) : base(owner)
        {
            this.playerListHandler = playerListHandler;
            this.client = client;
            this.client.PlayerListUpdated += OnPlayerListUpdated;
            this.client.CurrentRoomChanged += OnCurrentRoomChanged;
            // 行上的按钮只发 Id，接到门面上这一步由这里做：PlayerListHandler 因此不用认识 IUnityClient。
            this.playerListHandler.TeleportRequested += this.client.TeleportTo;
            ImageUtility.MakeTransparent(UIRoot);
            ImageUtility.MakeTransparent(ContentRoot);
            CreateRoomInfoRow();
            this.playerListHandler.Setup(ContentRoot, PanelBackgroundColor, HeaderBackgroundColor);
            RefreshPlayers();
            LayoutRebuilder.ForceRebuildLayoutImmediate(ContentRoot.GetComponent<RectTransform>());
        }

        public override string Name => "GOILauncher.PlayerList";

        // 够放下"玩家 / 信息 / 状态 / 距离 / 操作"五列：后四列是固定宽度，
        // 加起来已经占掉约 350，剩下的才归玩家名那列（flexibleWidth）。
        public override int MinWidth => 450;

        public override int MinHeight => 214;

        public override Vector2 DefaultAnchorMin => new Vector2(0.5f, 0.5f);

        public override Vector2 DefaultAnchorMax => new Vector2(0.5f, 0.5f);

        public override Vector2 DefaultPosition => new Vector2(-MinWidth * 0.5f, MinHeight * 0.5f);

        public override bool CanDragAndResize => false;

        public override void SetActive(bool active)
        {
            base.SetActive(active);

            if (!active)
                return;

            RefreshPlayers();
            playerListHandler.RefreshDistances();
            _distanceTimer = 0f;
        }

        public override void Update()
        {
            if (!Enabled || playerListHandler == null)
                return;

            _distanceTimer += Time.unscaledDeltaTime;
            if (_distanceTimer < DistanceRefreshInterval)
                return;

            _distanceTimer = 0f;
            playerListHandler.RefreshDistances();
        }

        public void RefreshPlayers()
        {
            if (playerListHandler == null || client == null)
                return;

            RefreshRoomInfo();
            playerListHandler.SetPlayers(client.Players);
        }

        private void CreateRoomInfoRow()
        {
            // PanelBase defaults to a 2px content gap; the room row and player table should touch.
            ContentRoot.GetComponent<VerticalLayoutGroup>().spacing = 0f;
            var row = UIFactory.CreateHorizontalGroup(
                ContentRoot, "RoomInfoRow", false, false, true, true, 8,
                new Vector4(4, 4, 10, 10), HeaderBackgroundColor);
            UIFactory.SetLayoutElement(row, minHeight: 30, flexibleHeight: 0, flexibleWidth: 9999);

            currentRoomText = UIFactory.CreateLabel(row, "CurrentRoom", string.Empty, TextAnchor.MiddleLeft);
            roomOwnerText = UIFactory.CreateLabel(row, "RoomOwner", string.Empty, TextAnchor.MiddleLeft);
            foreach (var label in new[] { currentRoomText, roomOwnerText })
            {
                label.supportRichText = false;
                // Two equal-width columns; room/player names are read only through the facade.
                UIFactory.SetLayoutElement(label.gameObject, minWidth: 0, preferredWidth: 0,
                    minHeight: 22, flexibleHeight: 0, flexibleWidth: 1);
            }
        }

        private void RefreshRoomInfo()
        {
            if (currentRoomText == null || roomOwnerText == null || client == null)
                return;

            var room = client.CurrentRoom;
            currentRoomText.text = "当前房间：" + (room == null ? "未加入" : room.Name);
            string ownerName = room == null ? "-" : "无";
            if (room != null && room.OwnerPlayerId.HasValue)
            {
                ownerName = client.TryGetPlayer(room.OwnerPlayerId.Value, out var owner) && !string.IsNullOrEmpty(owner.Name)
                    ? owner.Name : "未知";
            }
            roomOwnerText.text = "房主：" + ownerName;
        }

        private void OnCurrentRoomChanged()
        {
            if (Enabled)
                RefreshRoomInfo();
        }

        protected override void ConstructPanelContent()
        {
        }

        private void OnPlayerListUpdated()
        {
            if (!Enabled)
                return;

            RefreshPlayers();
        }
    }
}
