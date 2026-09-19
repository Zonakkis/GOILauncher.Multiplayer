using System;
using GOILauncher.Multiplayer.Extensions;
using GOILauncher.Multiplayer.Unity.Config;
using GOILauncher.Multiplayer.UI.Theme;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Models;
using UniverseLib.Utility;

namespace GOILauncher.Multiplayer.UI.Pages
{
    /// <summary>
    /// Edits the persisted settings. The fields here are defaults: the client and server pages seed
    /// their inputs from them, and changing one over there is for that one connection only, so this
    /// page is the only writer.
    ///
    /// 布局按"设置块"分组：每块一个区块头 + 一张卡片，卡片里是若干字段行。
    /// 以前三块之间没有视觉边界，看不出哪些字段属于同一类。
    /// </summary>
    public class SettingsPage : IPage
    {
        private readonly MultiplayerSettings _settings;
        private readonly Toast _toast;

        private Toggle _enabledToggle;
        private InputFieldRef _playerNameInput;
        private InputFieldRef _clientHostInput;
        private InputFieldRef _clientPortInput;
        private InputFieldRef _serverPortInput;

        public SettingsPage(MultiplayerSettings settings, Toast toast)
        {
            _settings = settings;
            _toast = toast;
            _settings.EnabledChanged += OnEnabledChanged;
        }

        public GameObject Root { get; private set; }

        public void SetActive(bool active)
        {
            // Leaving the page commits once more: switching pages or closing the panel does not always
            // end an edit, and a value that did not change makes the setter a no-op, so this is free.
            if (!active)
                CommitInputs();

            Root?.SetActive(active);

            if (active)
                RefreshFromSettings();
        }

        public void CreateContent(GameObject pagesContainer)
        {
            Root = UIFactory.CreateVerticalGroup(
                pagesContainer, "SettingsPage", false, false, true, true,
                Layout.SpaceSm,
                new Vector4(Layout.SpaceMd, Layout.SpaceMd, Layout.SpaceMd, Layout.SpaceMd),
                Plugin.Theme.SurfaceBase);
            UIFactory.SetLayoutElement(Root, flexibleHeight: 9999, flexibleWidth: 9999);

            CreateTogglesSection();
            CreateClientSection();
            CreateServerSection();
        }

        /// <summary>
        /// 开关单独成块并放在最上面：它不是"默认值"之一，而是决定整个联机功能开不开的总闸，
        /// 混在下面那张卡片里会和"默认主机"之类的设置看起来一样重要。
        /// </summary>
        private void CreateTogglesSection()
        {
            UiKit.CreateSectionHeader(Root, "GeneralHeader", "常规");

            GameObject card = CreateCard("GeneralCard");
            GameObject row = UiKit.CreateFieldRow(card, "MultiplayerEnabledRow", 40);

            Text toggleText;
            GameObject toggleObject = UIFactory.CreateToggle(
                row, "EnableMultiplayerToggle", out _enabledToggle, out toggleText,
                Plugin.Theme.SurfaceHover);
            UIFactory.SetLayoutElement(toggleObject, minHeight: Layout.InlineButtonHeight,
                preferredHeight: Layout.InlineButtonHeight, flexibleHeight: 0, flexibleWidth: 9999);
            toggleText.text = "启用联机";
            toggleText.color = Plugin.Theme.TextPrimary;
            toggleText.fontSize = Layout.FontBody;

            _enabledToggle.isOn = _settings.Enabled;
            _enabledToggle.onValueChanged.AddListener(OnToggleChanged);
        }

        /// <summary>
        /// 客户端设置。默认值是"下一次连接用什么"，所以关掉联机也照样能改——
        /// 先配置好再打开才是正常顺序。
        /// </summary>
        private void CreateClientSection()
        {
            UiKit.CreateSectionHeader(Root, "ClientSectionHeader", "客户端默认值");

            GameObject card = CreateCard("ClientCard");

            GameObject nameRow = UiKit.CreateFieldRow(card, "PlayerNameRow");
            UiKit.CreateFieldLabel(nameRow, "PlayerNameLabel", "名字");
            _playerNameInput = UiKit.CreateInputField(nameRow, "PlayerNameInput", "联机时显示的名字");
            // 空名字是合法的"还没填"，所以不显示占位提示。
            _playerNameInput.HidePlaceholder();
            _playerNameInput.Text = _settings.PlayerName;
            _playerNameInput.Component.GetOnEndEdit().AddListener(OnPlayerNameEndEdit);

            GameObject addressRow = UiKit.CreateFieldRow(card, "ClientDefaultAddressRow");
            UiKit.CreateFieldLabel(addressRow, "ClientDefaultHostLabel", "默认主机");
            _clientHostInput = UiKit.CreateInputField(addressRow, "ClientDefaultHostInput", MultiplayerSettings.DefaultClientHost);
            _clientHostInput.Text = _settings.ClientHost;
            _clientHostInput.Component.GetOnEndEdit().AddListener(OnClientHostEndEdit);

            // 端口和主机同属"连到哪"，分两行只是各占半行空白。并排放一行，
            // 端口用固定宽度（见 UiKit.CreateFieldLabel 的 width 参数）。
            UiKit.CreateFieldLabel(addressRow, "ClientDefaultPortLabel", "端口", 36);
            _clientPortInput = CreatePortInput(addressRow, "ClientDefaultPortInput", _settings.ClientPort);
            _clientPortInput.Component.GetOnEndEdit().AddListener(OnClientPortEndEdit);
        }

        private void CreateServerSection()
        {
            UiKit.CreateSectionHeader(Root, "ServerSectionHeader", "服务端默认值");

            GameObject card = CreateCard("ServerCard");

            GameObject portRow = UiKit.CreateFieldRow(card, "ServerDefaultPortRow");
            UiKit.CreateFieldLabel(portRow, "ServerDefaultPortLabel", "默认端口");
            _serverPortInput = CreatePortInput(portRow, "ServerDefaultPortInput", _settings.ServerPort);
            _serverPortInput.Component.GetOnEndEdit().AddListener(OnServerPortEndEdit);
        }

        private GameObject CreateCard(string name)
        {
            GameObject card = UIFactory.CreateVerticalGroup(
                Root, name, false, false, true, true,
                Layout.SpaceSm,
                new Vector4(Layout.SpaceSm, Layout.SpaceSm, Layout.SpaceSm, Layout.SpaceSm),
                Plugin.Theme.SurfaceRaised);
            UIFactory.SetLayoutElement(card, minHeight: 0, flexibleHeight: 0, flexibleWidth: 9999);
            return card;
        }

        private static InputFieldRef CreatePortInput(GameObject row, string name, int port)
        {
            string text = InputFieldExtensions.FormatPort(port);
            InputFieldRef input = UiKit.CreateInputField(row, name, text, 96);
            input.Component.contentType = InputField.ContentType.IntegerNumber;
            input.Text = text;
            return input;
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

        // Persisting happens when the edit ends, not per keystroke: every write rewrites the whole
        // settings file.
        // A blank name is a valid "not chosen yet" state, so clearing the field does not restore
        // anything and the field stays blank; the client page is where an empty name is refused at
        // connect time.
        private void OnPlayerNameEndEdit(string value)
        {
            _settings.SetPlayerName(value);
            SetInputText(_playerNameInput, _settings.PlayerName);
        }

        private void OnClientHostEndEdit(string value)
        {
            _settings.SetClientHost(value);

            // Clearing the field means "back to the default", so show what was actually stored.
            SetInputText(_clientHostInput, _settings.ClientHost);
        }

        private void OnClientPortEndEdit(string value)
        {
            CommitPort(_clientPortInput, _settings.ClientPort, _settings.SetClientPort);
        }

        private void OnServerPortEndEdit(string value)
        {
            CommitPort(_serverPortInput, _settings.ServerPort, _settings.SetServerPort);
        }

        private void CommitPort(InputFieldRef input, int currentValue, Action<int> apply)
        {
            int port;
            if (input.TryReadPort(out port))
            {
                apply(port);
                return;
            }

            _toast.Show(InputFieldExtensions.InvalidPortMessage);
            SetInputText(input, InputFieldExtensions.FormatPort(currentValue));
        }

        private void CommitInputs()
        {
            if (_playerNameInput != null)
                OnPlayerNameEndEdit(_playerNameInput.Text);

            if (_clientHostInput != null)
                OnClientHostEndEdit(_clientHostInput.Text);

            if (_clientPortInput != null)
                OnClientPortEndEdit(_clientPortInput.Text);

            if (_serverPortInput != null)
                OnServerPortEndEdit(_serverPortInput.Text);
        }

        private void RefreshFromSettings()
        {
            RefreshToggle();
            SetInputText(_playerNameInput, _settings.PlayerName);
            SetInputText(_clientHostInput, _settings.ClientHost);
            SetInputText(_clientPortInput, InputFieldExtensions.FormatPort(_settings.ClientPort));
            SetInputText(_serverPortInput, InputFieldExtensions.FormatPort(_settings.ServerPort));
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

        private static void SetInputText(InputFieldRef input, string text)
        {
            if (input != null)
                input.Text = text;
        }
    }
}