using GOILauncher.Multiplayer.UI.Components;
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
        private readonly IPlayerListUiComponent playerListComponent;

        public PlayerListUI(UIBase owner, PlayerListHandler playerListHandler, IPlayerListUiComponent playerListComponent) : base(owner)
        {
            this.playerListHandler = playerListHandler;
            this.playerListComponent = playerListComponent;
            ImageUtility.MakeTransparent(UIRoot);
            ImageUtility.MakeTransparent(ContentRoot);
            this.playerListHandler.Setup(ContentRoot, PanelBackgroundColor);
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
            if (playerListHandler == null || playerListComponent == null)
                return;

            playerListHandler.Update(playerListComponent.GetPlayers());
        }

        protected override void ConstructPanelContent()
        {
            GameObject headerRow = UIFactory.CreateHorizontalGroup(
                ContentRoot,
                "PlayerListHeader",
                false,
                false,
                true,
                true,
                6,
                new Vector4(8, 6, 8, 6),
                PanelBackgroundColor);
            UIFactory.SetLayoutElement(headerRow, minHeight: 32, flexibleHeight: 0);

            Text headerText = UIFactory.CreateLabel(headerRow, "PlayerListHeaderText", "在线玩家", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(headerText.gameObject, minHeight: 20, flexibleHeight: 0, flexibleWidth: 9999);

            GameObject tableHeader = UIFactory.CreateHorizontalGroup(
                ContentRoot,
                "PlayerTableHeader",
                false,
                false,
                true,
                true,
                4,
                new Vector4(8, 4, 8, 4),
                HeaderBackgroundColor);
            UIFactory.SetLayoutElement(tableHeader, minHeight: 28, flexibleHeight: 0);

            Text playerNameHeader = UIFactory.CreateLabel(tableHeader, "PlayerNameHeader", "玩家", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(playerNameHeader.gameObject, minHeight: 20, flexibleHeight: 0, flexibleWidth: 9999);

            Text detailHeader = UIFactory.CreateLabel(tableHeader, "PlayerDetailHeader", "信息", TextAnchor.MiddleCenter);
            UIFactory.SetLayoutElement(detailHeader.gameObject, minWidth: 80, preferredWidth: 90, minHeight: 20, flexibleHeight: 0, flexibleWidth: 0);
        }
    }
}
