using System.Collections.Generic;
using GOILauncher.Multiplayer.UI.Components;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Widgets;

namespace GOILauncher.Multiplayer.UI.ScrollView.Player
{
    public class PlayerListHandler
    {
        private GameObject playerListContent;
        private AutoSliderScrollbar playerListScrollbar;

        public void Setup(GameObject parent, Color backgroundColor)
        {
            GameObject playerScroll = UIFactory.CreateScrollView(
                parent,
                "PlayerListScroll",
                out playerListContent,
                out playerListScrollbar,
                backgroundColor);
            HideScrollbar(playerScroll);
            UIFactory.SetLayoutElement(playerScroll, minHeight: 120, flexibleHeight: 9999, flexibleWidth: 9999);
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(playerListContent, false, false, true, true, 3, 4, 4, 4, 4, TextAnchor.UpperLeft);
        }

        public void Update(IEnumerable<PlayerListItemViewData> players)
        {
            if (playerListContent == null)
                return;

            ClearRows();
            if (players == null)
                return;

            int rowIndex = 0;
            foreach (PlayerListItemViewData player in players)
            {
                CreatePlayerRow(player, rowIndex);
                rowIndex++;
            }

            if (playerListScrollbar != null)
                playerListScrollbar.UpdateSliderHandle();
        }

        private void ClearRows()
        {
            for (int i = playerListContent.transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(playerListContent.transform.GetChild(i).gameObject);
            }
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
