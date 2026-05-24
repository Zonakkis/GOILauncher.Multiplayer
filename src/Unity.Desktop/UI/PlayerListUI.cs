using System.Collections.Generic;
using GOILauncher.Multiplayer.UI.Components;
using UniverseLib.UI;
using UniverseLib.UI.Models;
using UniverseLib.UI.Panels;
using UniverseLib.UI.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace GOILauncher.Multiplayer.UI
{
    public class PlayerListUI : PanelBase
    {
        private static readonly Color PanelBackgroundColor = new Color(0f, 0f, 0f, 0.35f);
        private static readonly Color HeaderBackgroundColor = new Color(0f, 0f, 0f, 0.42f);

        private static readonly PlayerListItemViewData[] DefaultPlayers = new PlayerListItemViewData[]
        {
            new PlayerListItemViewData("玩家A", "PC"),
            new PlayerListItemViewData("玩家B", "PC"),
            new PlayerListItemViewData("玩家C", "SteamDeck")
        };

        private IPlayerListUiComponent playerListComponent;
        private GameObject playerListContent;
        private AutoSliderScrollbar playerListScrollbar;

        public PlayerListUI(UIBase owner) : base(owner)
        {
            ImageUtility.MakeTransparent(UIRoot);
            ImageUtility.MakeTransparent(ContentRoot);
        }

        public override string Name => "GOILauncher.PlayerList";

        public override int MinWidth => 240;

        public override int MinHeight => 180;

        public override Vector2 DefaultAnchorMin => new Vector2(0.5f, 0.5f);

        public override Vector2 DefaultAnchorMax => new Vector2(0.5f, 0.5f);

        public override Vector2 DefaultPosition => new Vector2(-MinWidth * 0.5f, MinHeight * 0.5f);

        public override bool CanDragAndResize => false;

        public void RefreshPlayers()
        {
            if (playerListContent == null)
                return;

            for (int i = playerListContent.transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(playerListContent.transform.GetChild(i).gameObject);
            }

            int rowIndex = 0;
            foreach (PlayerListItemViewData player in GetPlayers())
            {
                CreatePlayerRow(player, rowIndex);
                rowIndex++;
            }

            if (playerListScrollbar != null)
                playerListScrollbar.UpdateSliderHandle();
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

            ButtonRef refreshButton = UIFactory.CreateButton(headerRow, "PlayerListRefresh", "刷新");
            UIFactory.SetLayoutElement(refreshButton.Component.gameObject, minWidth: 70, minHeight: 22, flexibleWidth: 0, flexibleHeight: 0);
            refreshButton.OnClick += RefreshPlayers;

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

            GameObject playerScroll = UIFactory.CreateScrollView(
                ContentRoot,
                "PlayerListScroll",
                out playerListContent,
                out playerListScrollbar,
                PanelBackgroundColor);
            HideScrollbar(playerScroll);
            UIFactory.SetLayoutElement(playerScroll, minHeight: 120, flexibleHeight: 9999, flexibleWidth: 9999);
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(playerListContent, false, false, true, true, 3, 4, 4, 4, 4, TextAnchor.UpperLeft);

            RefreshPlayers();
        }

        private PlayerListItemViewData[] GetPlayers()
        {
            if (playerListComponent == null)
                return DefaultPlayers;

            List<PlayerListItemViewData> players = new List<PlayerListItemViewData>();
            foreach (PlayerListItemViewData player in playerListComponent.GetPlayers())
            {
                players.Add(player);
            }

            if (players.Count == 0)
                return DefaultPlayers;

            return players.ToArray();
        }

        private void CreatePlayerRow(PlayerListItemViewData player, int rowIndex)
        {
            GameObject row = UIFactory.CreateHorizontalGroup(
                playerListContent,
                "PlayerRow_" + rowIndex,
                false,
                false,
                true,
                true,
                4,
                new Vector4(6, 3, 6, 3),
                Color.clear,
                TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(row, minHeight: 26, flexibleHeight: 0, flexibleWidth: 9999);
            ImageUtility.MakeTransparent(row);

            Text nameText = UIFactory.CreateLabel(row, "PlayerName", player.PlayerName, TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(nameText.gameObject, minHeight: 20, flexibleHeight: 0, flexibleWidth: 9999);

            Text detailText = UIFactory.CreateLabel(row, "PlayerDetail", player.Detail, TextAnchor.MiddleCenter);
            UIFactory.SetLayoutElement(detailText.gameObject, minWidth: 80, preferredWidth: 90, minHeight: 20, flexibleHeight: 0, flexibleWidth: 0);
        }

        private static void HideScrollbar(GameObject scrollView)
        {
            Transform scrollbar = scrollView.transform.Find("AutoSliderScrollbar");
            if (scrollbar != null)
                scrollbar.gameObject.SetActive(false);

            RectTransform viewport = scrollView.transform.Find("Viewport")?.GetComponent<RectTransform>();
            if (viewport == null)
                return;

            viewport.offsetMax = Vector2.zero;
        }
    }
}
