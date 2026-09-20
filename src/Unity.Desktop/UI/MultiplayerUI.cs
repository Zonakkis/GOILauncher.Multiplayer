using System.Collections.Generic;
using GOILauncher.Multiplayer.Extensions;
using GOILauncher.Multiplayer.UI.Pages;
using GOILauncher.Multiplayer.UI.Theme;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Models;
using UniverseLib.UI.Panels;

namespace GOILauncher.Multiplayer.UI
{
    public partial class MultiplayerUI : PanelBase
    {

        public override string Name => "\u8fde\u63a5\u914d\u7f6e";

        public override int MinWidth => 600;

        public override int MinHeight => 640;

        public override Vector2 DefaultAnchorMin => new Vector2(0.5f, 0.5f);

        public override Vector2 DefaultAnchorMax => new Vector2(0.5f, 0.5f);

        public override Vector2 DefaultPosition => new Vector2(-MinWidth * 0.5f, MinHeight * 0.5f);

        public override bool CanDragAndResize => true;

        private readonly List<PageEntry> _pages;
        private readonly RoomDialogUI _roomDialog;
        private GameObject _pagesContainer;
        private ButtonRef _serverButton;

        public MultiplayerUI(UIBase owner, ClientPage clientPage, ServerPage serverPage, SettingsPage settingsPage, RoomDialogUI roomDialog) : base(owner)
        {
            _roomDialog = roomDialog;
            _pages = new List<PageEntry>
            {
                new PageEntry(MultiplayerPage.Client, "\u5ba2\u6237\u7aef", clientPage),
                new PageEntry(MultiplayerPage.Server, "\u670d\u52a1\u7aef", serverPage),
                new PageEntry(MultiplayerPage.Settings, "\u8bbe\u7f6e", settingsPage)
            };

            CreateContent();

            foreach (PageEntry page in _pages)
                page.Page.CreateContent(_pagesContainer);

            ShowPage(MultiplayerPage.Client);
        }

        public override void SetActive(bool active)
        {
            if (!active) _roomDialog?.SetActive(false);
            base.SetActive(active);
        }

        /// <summary>
        /// 收起或放出"服务端"页签。只关按钮的显示：服务端页面照旧建出来、照旧被 Bind，
        /// 所以开着的内嵌服务端不会因为切了个开关就断掉。
        /// <para>
        /// 无需处理"玩家正站在服务端页面上"：这个勾选框在设置页里，勾它的时候玩家不可能在别的页面。
        /// </para>
        /// </summary>
        public void SetServerPageVisible(bool visible)
        {
            if (_serverButton != null)
                _serverButton.GameObject.SetActive(visible);
        }

        protected override void ConstructPanelContent()
        {
        }

        protected override PanelDragger CreatePanelDragger()
        {
            return new ResponsivePanelDragger(this);
        }

        private void CreateContent()
        {
            GameObject buttonRow = UIFactory.CreateHorizontalGroup(ContentRoot, "ButtonRow", true, false, true, true,
                Layout.SpaceSm,
                // UIFactory 按 (top, bottom, left, right) 取这个 Vector4，不是直觉的
                // (left, top, right, bottom)。上下各 4 是有数的：行高 32 扣掉这 8，
                // 剩下的 24 正好等于页签按钮自己的高度；给多了按钮就会被裁掉一截。
                new Vector4(Layout.SpaceXs, Layout.SpaceXs, Layout.SpaceMd, Layout.SpaceMd),
                Plugin.Theme.SurfaceHeader);
            UIFactory.SetLayoutElement(buttonRow, minHeight: Layout.SectionHeaderHeight, preferredHeight: Layout.SectionHeaderHeight,
                flexibleHeight: 0);
            buttonRow.AddComponent<ToggleGroup>();

            foreach (PageEntry page in _pages)
            {
                PageEntry pageEntry = page;
                ButtonRef button = UIFactory.CreateButton(buttonRow, pageEntry.Id + "Button", pageEntry.ButtonText);
                button.ButtonText.color = Plugin.Theme.TextPrimary;
                button.ButtonText.fontSize = Layout.FontBody;
                UIFactory.SetLayoutElement(button.Component.gameObject, minHeight: Layout.InlineButtonHeight,
                    preferredHeight: Layout.InlineButtonHeight, minWidth: 100, flexibleWidth: 9999, flexibleHeight: 0);
                button.OnClick += () => ShowPage(pageEntry.Id);
                pageEntry.Button = button;

                if (pageEntry.Id == MultiplayerPage.Server)
                    _serverButton = button;
            }

            _pagesContainer = UIFactory.CreateUIObject("PagesContainer", ContentRoot);
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(_pagesContainer, false, false, true, true, 0);
            UIFactory.SetLayoutElement(_pagesContainer, flexibleHeight: 9999, flexibleWidth: 9999);
        }

        private void ShowPage(MultiplayerPage pageId)
        {
            foreach (PageEntry page in _pages)
            {
                bool active = page.Id == pageId;
                page.Button.SetSelected(active);
                page.Page.SetActive(active);
            }
        }
    }
}
