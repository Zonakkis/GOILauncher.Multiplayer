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

        private float _distanceTimer;

        public PlayerListUI(UIBase owner, PlayerListHandler playerListHandler, IUnityClient client) : base(owner)
        {
            this.playerListHandler = playerListHandler;
            this.client = client;
            this.client.PlayerListUpdated += OnPlayerListUpdated;
            ImageUtility.MakeTransparent(UIRoot);
            ImageUtility.MakeTransparent(ContentRoot);
            this.playerListHandler.Setup(ContentRoot, PanelBackgroundColor, HeaderBackgroundColor);
            RefreshPlayers();
            LayoutRebuilder.ForceRebuildLayoutImmediate(ContentRoot.GetComponent<RectTransform>());
        }

        public override string Name => "GOILauncher.PlayerList";

        public override int MinWidth => 320;

        public override int MinHeight => 180;

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

            playerListHandler.SetPlayers(client.Players);
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
