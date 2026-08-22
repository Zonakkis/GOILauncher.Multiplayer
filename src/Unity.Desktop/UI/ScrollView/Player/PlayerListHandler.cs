using System;
using System.Collections.Generic;
using GOILauncher.Multiplayer.Unity.Player;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Models;
using UniverseLib.UI.Widgets;

namespace GOILauncher.Multiplayer.UI.ScrollView.Player
{
    /// <summary>
    /// 玩家列表的行渲染。刷新分两种节奏，不要混在一起：
    ///
    /// - <see cref="SetPlayers"/> 是结构刷新，只在名单变化时调用，会增删行；
    /// - <see cref="RefreshDistances"/> 是数值刷新，面板可见时按帧调用，只改已有行上的文本。
    ///
    /// 距离每帧都在变，如果沿用"清空重建全部行"的做法，按住 Tab 期间每帧都要
    /// Destroy + Instantiate + 重建布局。
    ///
    /// 传送按钮不直接调门面：点击只发 <see cref="TeleportRequested"/>，由 PlayerListUI
    /// 接到 IUnityClient.TeleportTo 上，本类因此仍然只是个视图，不认识任何门面类型。
    /// </summary>
    public class PlayerListHandler
    {
        private const int ContentPadding = 4;
        private const int RowHorizontalPadding = 6;
        private const int HeaderHorizontalPadding = ContentPadding + RowHorizontalPadding;
        private const string NoDistanceText = "-";

        // 变化小于这个值就不重写 Text：每帧写字符串会持续产生垃圾并触发布局重建。
        private const float DistanceEpsilon = 0.05f;

        private readonly Dictionary<int, PlayerRow> _rows = new Dictionary<int, PlayerRow>();
        private readonly List<int> _staleIds = new List<int>();
        private readonly List<PlayerView> _ordered = new List<PlayerView>();

        private GameObject playerListContent;
        private AutoSliderScrollbar playerListScrollbar;

        /// <summary>
        /// 某一行的传送按钮被点击，参数是那名玩家的 Id。
        /// </summary>
        public event Action<int> TeleportRequested;

        public void Setup(GameObject parent, Color backgroundColor, Color headerBackgroundColor)
        {
            GameObject body = UIFactory.CreateVerticalGroup(
                parent,
                "PlayerListBody",
                false,
                false,
                true,
                true,
                0,
                new Vector4(0, 0, 0, 0),
                backgroundColor);
            UIFactory.SetLayoutElement(body, minHeight: 120, flexibleHeight: 9999, flexibleWidth: 9999);

            CreateTableHeader(body, headerBackgroundColor);

            GameObject playerScroll = UIFactory.CreateScrollView(
                body,
                "PlayerListScroll",
                out playerListContent,
                out playerListScrollbar,
                backgroundColor);
            HideScrollbar(playerScroll);
            UIFactory.SetLayoutElement(playerScroll, minHeight: 120, flexibleHeight: 9999, flexibleWidth: 9999);
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(
                playerListContent,
                false,
                false,
                true,
                true,
                3,
                ContentPadding,
                ContentPadding,
                ContentPadding,
                ContentPadding,
                TextAnchor.UpperLeft);
        }

        /// <summary>
        /// 按玩家 Id 复用已有行：只增删变化的部分，其余行原地更新文本。
        /// </summary>
        public void SetPlayers(IEnumerable<PlayerView> players)
        {
            if (playerListContent == null)
                return;

            _ordered.Clear();
            if (players != null)
            {
                foreach (PlayerView player in players)
                    _ordered.Add(player);
            }

            // 本地玩家置顶，其余按 Id 排；否则行的先后会随字典内部顺序漂移。
            _ordered.Sort((left, right) =>
            {
                if (left.IsLocal != right.IsLocal)
                    return left.IsLocal ? -1 : 1;
                return left.Id.CompareTo(right.Id);
            });

            _staleIds.Clear();
            foreach (KeyValuePair<int, PlayerRow> pair in _rows)
                _staleIds.Add(pair.Key);

            for (int i = 0; i < _ordered.Count; i++)
            {
                PlayerView player = _ordered[i];
                _staleIds.Remove(player.Id);

                PlayerRow row;
                if (!_rows.TryGetValue(player.Id, out row))
                {
                    row = CreatePlayerRow(player.Id);
                    _rows[player.Id] = row;
                }

                // 行上存的是读透视图而不是快照，距离刷新时直接从它取值。
                row.View = player;
                row.Name.text = GetPlayerName(player);
                row.Detail.text = player.Platform.ToString();
                row.Status.text = player.IsInGame ? "游戏中" : "大厅";
                // 自己那行不给传送按钮；外层格子留着，所以列不会错位（见 CreatePlayerRow）。
                row.Teleport.Component.gameObject.SetActive(!player.IsLocal);
                row.Root.transform.SetSiblingIndex(i);
            }

            for (int i = 0; i < _staleIds.Count; i++)
                RemoveRow(_staleIds[i]);

            if (playerListScrollbar != null)
                playerListScrollbar.UpdateSliderHandle();
        }

        /// <summary>
        /// 只更新距离列和传送按钮的可用性。没有场景实例的玩家（大厅里、实例池已满、
        /// 首个状态包未到）显示占位符，这是正常状态而不是错误。
        /// </summary>
        public void RefreshDistances()
        {
            foreach (KeyValuePair<int, PlayerRow> pair in _rows)
            {
                PlayerRow row = pair.Value;
                float? distance = row.View.Distance;

                // 有距离 == 有远端实例 == 能传送过去，判据只此一处（见 docs/ui-facade.md）。
                // 自己那行的按钮由 SetPlayers 直接隐藏，这里写不写它都无所谓。
                SetTeleportEnabled(row, distance.HasValue);

                if (!distance.HasValue)
                {
                    if (row.HasDistance)
                    {
                        row.Distance.text = NoDistanceText;
                        row.HasDistance = false;
                    }
                    continue;
                }

                float meters = distance.Value;
                if (row.HasDistance && Mathf.Abs(meters - row.LastDistance) < DistanceEpsilon)
                    continue;

                row.Distance.text = meters.ToString("0.0") + "m";
                row.LastDistance = meters;
                row.HasDistance = true;
            }
        }

        private void RemoveRow(int playerId)
        {
            PlayerRow row;
            if (!_rows.TryGetValue(playerId, out row))
                return;

            _rows.Remove(playerId);
            if (row.Root != null)
                UnityEngine.Object.Destroy(row.Root);
        }

        private static void CreateTableHeader(GameObject parent, Color backgroundColor)
        {
            GameObject tableHeader = UIFactory.CreateHorizontalGroup(
                parent,
                "PlayerTableHeader",
                false,
                false,
                true,
                true,
                4,
                new Vector4(HeaderHorizontalPadding, 4, HeaderHorizontalPadding, 4),
                backgroundColor);
            UIFactory.SetLayoutElement(tableHeader, minHeight: 28, flexibleHeight: 0);

            Text playerNameHeader = UIFactory.CreateLabel(tableHeader, "PlayerNameHeader", "玩家", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(playerNameHeader.gameObject, minHeight: 20, flexibleHeight: 0, flexibleWidth: 9999);

            Text detailHeader = UIFactory.CreateLabel(tableHeader, "PlayerDetailHeader", "信息", TextAnchor.MiddleCenter);
            UIFactory.SetLayoutElement(detailHeader.gameObject, minWidth: 80, preferredWidth: 90, minHeight: 20, flexibleHeight: 0, flexibleWidth: 0);

            Text statusHeader = UIFactory.CreateLabel(tableHeader, "PlayerStatusHeader", "状态", TextAnchor.MiddleCenter);
            UIFactory.SetLayoutElement(statusHeader.gameObject, minWidth: 60, preferredWidth: 70, minHeight: 20, flexibleHeight: 0, flexibleWidth: 0);

            Text distanceHeader = UIFactory.CreateLabel(tableHeader, "PlayerDistanceHeader", "距离", TextAnchor.MiddleCenter);
            UIFactory.SetLayoutElement(distanceHeader.gameObject, minWidth: 60, preferredWidth: 70, minHeight: 20, flexibleHeight: 0, flexibleWidth: 0);

            // 这一格不写字（"操作"没什么信息量），但格子必须留着：撤掉它表头就少一列，
            // 玩家名那列的 flexibleWidth 会吃掉空位，表头和行就对不齐了。
            Text actionHeader = UIFactory.CreateLabel(tableHeader, "PlayerActionHeader", string.Empty, TextAnchor.MiddleCenter);
            UIFactory.SetLayoutElement(actionHeader.gameObject, minWidth: 56, preferredWidth: 64, minHeight: 20, flexibleHeight: 0, flexibleWidth: 0);
        }

        private PlayerRow CreatePlayerRow(int playerId)
        {
            GameObject row = UIFactory.CreateHorizontalGroup(
                playerListContent,
                "PlayerRow_" + playerId,
                false,
                false,
                true,
                true,
                4,
                new Vector4(RowHorizontalPadding, 3, RowHorizontalPadding, 3),
                Color.clear,
                TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(row, minHeight: 26, flexibleHeight: 0, flexibleWidth: 9999);
            ImageUtility.MakeTransparent(row);

            Text nameText = UIFactory.CreateLabel(row, "PlayerName", string.Empty, TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(nameText.gameObject, minHeight: 20, flexibleHeight: 0, flexibleWidth: 9999);

            Text detailText = UIFactory.CreateLabel(row, "PlayerDetail", string.Empty, TextAnchor.MiddleCenter);
            UIFactory.SetLayoutElement(detailText.gameObject, minWidth: 80, preferredWidth: 90, minHeight: 20, flexibleHeight: 0, flexibleWidth: 0);

            Text statusText = UIFactory.CreateLabel(row, "PlayerStatus", string.Empty, TextAnchor.MiddleCenter);
            UIFactory.SetLayoutElement(statusText.gameObject, minWidth: 60, preferredWidth: 70, minHeight: 20, flexibleHeight: 0, flexibleWidth: 0);

            Text distanceText = UIFactory.CreateLabel(row, "PlayerDistance", NoDistanceText, TextAnchor.MiddleCenter);
            UIFactory.SetLayoutElement(distanceText.gameObject, minWidth: 60, preferredWidth: 70, minHeight: 20, flexibleHeight: 0, flexibleWidth: 0);

            // 按钮外面套一格固定宽度的容器：自己那行会把按钮整个隐藏，但格子得留着，
            // 否则那一行少一列，玩家名那列的 flexibleWidth 会吃掉空位，后面几列就和别人对不齐了。
            GameObject actionCell = UIFactory.CreateHorizontalGroup(
                row,
                "PlayerActionCell",
                true,
                true,
                true,
                true,
                0,
                new Vector4(0, 0, 0, 0),
                Color.clear);
            UIFactory.SetLayoutElement(actionCell, minWidth: 56, preferredWidth: 64, minHeight: 20, flexibleHeight: 0, flexibleWidth: 0);
            ImageUtility.MakeTransparent(actionCell);

            ButtonRef teleportButton = UIFactory.CreateButton(actionCell, "PlayerTeleport", "传送");
            UIFactory.SetLayoutElement(teleportButton.Component.gameObject, minHeight: 20, flexibleHeight: 0, flexibleWidth: 9999);
            teleportButton.Component.interactable = false;
            // 行按 Id 建、也按 Id 销毁，所以闭包捕获 playerId 不会串到别人身上。
            teleportButton.OnClick += () => OnTeleportClicked(playerId);

            return new PlayerRow
            {
                Root = row,
                Name = nameText,
                Distance = distanceText,
                Detail = detailText,
                Status = statusText,
                Teleport = teleportButton
            };
        }

        private void OnTeleportClicked(int playerId)
        {
            TeleportRequested?.Invoke(playerId);
        }

        private static void SetTeleportEnabled(PlayerRow row, bool enabled)
        {
            // 和 DistanceEpsilon 同一个理由：写 interactable 会触发按钮的状态过渡，
            // 10 Hz 无条件重写没必要。
            if (row.CanTeleport == enabled)
                return;

            row.CanTeleport = enabled;
            row.Teleport.Component.interactable = enabled;
        }

        private static string GetPlayerName(PlayerView player)
        {
            return string.IsNullOrEmpty(player.Name) ? "Player " + player.Id : player.Name;
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

        private sealed class PlayerRow
        {
            public GameObject Root;
            public Text Name;
            public Text Distance;
            public Text Detail;
            public Text Status;
            public ButtonRef Teleport;
            public PlayerView View;
            public float LastDistance;
            public bool HasDistance;
            public bool CanTeleport;
        }
    }
}
