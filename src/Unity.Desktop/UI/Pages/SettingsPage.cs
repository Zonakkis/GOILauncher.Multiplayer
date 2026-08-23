using GOILauncher.Multiplayer.Unity.Config;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;

namespace GOILauncher.Multiplayer.UI.Pages
{
    public class SettingsPage : IPage
    {
        private readonly MultiplayerSettings _settings;
        private Toggle _enabledToggle;

        public SettingsPage(MultiplayerSettings settings)
        {
            _settings = settings;
            _settings.EnabledChanged += OnEnabledChanged;
        }

        public GameObject Root { get; private set; }

        public void SetActive(bool active)
        {
            Root?.SetActive(active);

            if (active)
                RefreshToggle();
        }

        public void CreateContent(GameObject pagesContainer)
        {
            Root = UIFactory.CreateVerticalGroup(
                pagesContainer,
                "SettingsPage",
                false,
                false,
                true,
                true,
                6,
                new Vector4(8, 8, 8, 8),
                new Color(0.12f, 0.12f, 0.12f, 0.95f));
            UIFactory.SetLayoutElement(Root, flexibleHeight: 9999, flexibleWidth: 9999);

            GameObject settingRow = UIFactory.CreateHorizontalGroup(
                Root,
                "MultiplayerEnabledRow",
                false,
                false,
                true,
                true,
                6,
                new Vector4(8, 6, 8, 6),
                new Color(0.16f, 0.16f, 0.16f, 1f),
                TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(settingRow, minHeight: 38, flexibleHeight: 0, flexibleWidth: 9999);

            Text toggleText;
            GameObject toggleObject = UIFactory.CreateToggle(
                settingRow,
                "EnableMultiplayerToggle",
                out _enabledToggle,
                out toggleText,
                new Color(0.22f, 0.22f, 0.22f, 1f));
            UIFactory.SetLayoutElement(toggleObject, minHeight: 28, flexibleHeight: 0, flexibleWidth: 9999);
            toggleText.text = "\u542f\u7528\u8054\u673a";

            _enabledToggle.isOn = _settings.Enabled;
            _enabledToggle.onValueChanged.AddListener(OnToggleChanged);
        }

        private void OnToggleChanged(bool enabled)
        {
            _settings.SetEnabled(enabled);
            RefreshToggle();
        }

        private void OnEnabledChanged(bool enabled)
        {
            RefreshToggle();
        }

        private void RefreshToggle()
        {
            if (_enabledToggle == null)
                return;

            _enabledToggle.onValueChanged.RemoveListener(OnToggleChanged);
            _enabledToggle.isOn = _settings.Enabled;
            _enabledToggle.onValueChanged.AddListener(OnToggleChanged);
            _enabledToggle.interactable = true;
        }
    }
}
