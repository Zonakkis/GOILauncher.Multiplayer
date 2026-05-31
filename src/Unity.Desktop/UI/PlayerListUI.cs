using GOILauncher.Multiplayer.Client;
using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.UI.ScrollView.Player;
using UniverseLib.UI;
using UniverseLib.UI.Panels;
using UnityEngine;
using UnityEngine.UI;

namespace GOILauncher.Multiplayer.UI
{
    public class PlayerListUI : PanelBase
    {
        private static readonly Color PanelBackgroundColor = new Color(0f, 0f, 0f, 0.35f);
        private static readonly Color HeaderBackgroundColor = new Color(0f, 0f, 0f, 0.42f);

        private readonly PlayerListHandler playerListHandler;
        private readonly IUnityClient client;

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

        public override int MinWidth => 240;

        public override int MinHeight => 180;

        public override Vector2 DefaultAnchorMin => new Vector2(0.5f, 0.5f);

        public override Vector2 DefaultAnchorMax => new Vector2(0.5f, 0.5f);

        public override Vector2 DefaultPosition => new Vector2(-MinWidth * 0.5f, MinHeight * 0.5f);

        public override bool CanDragAndResize => false;

        public override void SetActive(bool active)
        {
            base.SetActive(active);

            if (active)
                RefreshPlayers();
        }

        public void RefreshPlayers()
        {
            if (playerListHandler == null || client == null)
                return;

            playerListHandler.Update(client.Players);
        }

        protected override void ConstructPanelContent()
        {
        }

        private void OnPlayerListUpdated(object sender, PlayerListUpdatedEventArgs e)
        {
            if (!Enabled)
                return;

            playerListHandler.Update(e.Players);
        }
    }
}
