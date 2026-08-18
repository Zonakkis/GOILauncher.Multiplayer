using System.Collections.Generic;
using GOILauncher.Multiplayer.Extensions;
using GOILauncher.Multiplayer.UI.Pages;
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

        public override int MinHeight => 800;

        public override Vector2 DefaultAnchorMin => new Vector2(0.5f, 0.5f);

        public override Vector2 DefaultAnchorMax => new Vector2(0.5f, 0.5f);

        public override Vector2 DefaultPosition => new Vector2(-MinWidth * 0.5f, MinHeight * 0.5f);

        public override bool CanDragAndResize => true;

        private readonly List<PageEntry> _pages;
        private GameObject _pagesContainer;

        public MultiplayerUI(UIBase owner, ClientPage clientPage, ServerPage serverPage, SettingsPage settingsPage) : base(owner)
        {
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

        protected override void ConstructPanelContent()
        {
        }

        protected override PanelDragger CreatePanelDragger()
        {
            return new ResponsivePanelDragger(this);
        }

        private void CreateContent()
        {
            GameObject buttonRow = UIFactory.CreateHorizontalGroup(ContentRoot, "ButtonRow", false, false, true, true, 5, new Vector4(5, 5, 5, 5));
            UIFactory.SetLayoutElement(buttonRow, minHeight: 40, flexibleHeight: 0);
            buttonRow.AddComponent<ToggleGroup>();

            foreach (PageEntry page in _pages)
            {
                PageEntry pageEntry = page;
                ButtonRef button = UIFactory.CreateButton(buttonRow, pageEntry.Id + "Button", pageEntry.ButtonText);
                UIFactory.SetLayoutElement(button.Component.gameObject, minHeight: 30, minWidth: 100, flexibleWidth: 9999);
                button.OnClick += () => ShowPage(pageEntry.Id);
                pageEntry.Button = button;
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
                page.Button.SetTabActive(active);
                page.Page.SetActive(active);
            }
        }
    }
}
