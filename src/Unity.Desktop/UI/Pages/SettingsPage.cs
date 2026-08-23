using System;
using GOILauncher.Multiplayer.Extensions;
using GOILauncher.Multiplayer.Unity.Config;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Models;
using UniverseLib.Utility;

namespace GOILauncher.Multiplayer.UI.Pages
{
    /// <summary>
    /// Edits the persisted settings. The three address fields here are defaults: the client and server
    /// pages seed their inputs from them, and changing an address over there is for that one connection
    /// only, so this page is the only writer.
    /// </summary>
    public class SettingsPage : IPage
    {
        private readonly MultiplayerSettings _settings;
        private readonly Toast _toast;

        private Toggle _enabledToggle;
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

            // The address defaults stay editable with multiplayer off: configuring them before turning
            // it on is the normal order.
            CreateSectionLabel("ClientSectionLabel", "\u5ba2\u6237\u7aef\u8bbe\u7f6e");

            GameObject clientRow = CreateRow("ClientDefaultsRow");
            CreateFieldLabel(clientRow, "ClientDefaultHostLabel", "\u9ed8\u8ba4\u4e3b\u673a");
            _clientHostInput = CreateHostInput(clientRow, "ClientDefaultHostInput", _settings.ClientHost);
            _clientHostInput.Component.GetOnEndEdit().AddListener(OnClientHostEndEdit);

            CreateFieldLabel(clientRow, "ClientDefaultPortLabel", "\u9ed8\u8ba4\u7aef\u53e3");
            _clientPortInput = CreatePortInput(clientRow, "ClientDefaultPortInput", _settings.ClientPort);
            _clientPortInput.Component.GetOnEndEdit().AddListener(OnClientPortEndEdit);

            CreateSectionLabel("ServerSectionLabel", "\u670d\u52a1\u7aef\u8bbe\u7f6e");

            GameObject serverRow = CreateRow("ServerDefaultsRow");
            CreateFieldLabel(serverRow, "ServerDefaultPortLabel", "\u9ed8\u8ba4\u7aef\u53e3");
            _serverPortInput = CreatePortInput(serverRow, "ServerDefaultPortInput", _settings.ServerPort);
            _serverPortInput.Component.GetOnEndEdit().AddListener(OnServerPortEndEdit);
        }

        private void CreateSectionLabel(string name, string text)
        {
            Text label = UIFactory.CreateLabel(Root, name, text, TextAnchor.MiddleLeft);
            label.fontStyle = FontStyle.Bold;
            UIFactory.SetLayoutElement(label.gameObject, minHeight: 24, preferredHeight: 24, flexibleHeight: 0, flexibleWidth: 9999);
        }

        private GameObject CreateRow(string name)
        {
            GameObject row = UIFactory.CreateHorizontalGroup(
                Root,
                name,
                false,
                false,
                true,
                true,
                6,
                new Vector4(6, 4, 6, 4),
                new Color(0.16f, 0.16f, 0.16f, 1f));
            UIFactory.SetLayoutElement(row, minHeight: 30, flexibleHeight: 0, flexibleWidth: 9999);
            return row;
        }

        private static void CreateFieldLabel(GameObject row, string name, string text)
        {
            Text label = UIFactory.CreateLabel(row, name, text, TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(label.gameObject, minWidth: 68, preferredWidth: 72, minHeight: 22, flexibleHeight: 0, flexibleWidth: 0);
        }

        private static InputFieldRef CreateHostInput(GameObject row, string name, string host)
        {
            InputFieldRef input = UIFactory.CreateInputField(row, name, host);
            input.Text = host;
            UIFactory.SetLayoutElement(input.GameObject, minHeight: 24, flexibleHeight: 0, flexibleWidth: 9999);
            return input;
        }

        private static InputFieldRef CreatePortInput(GameObject row, string name, int port)
        {
            string text = InputFieldExtensions.FormatPort(port);
            InputFieldRef input = UIFactory.CreateInputField(row, name, text);
            input.Component.contentType = InputField.ContentType.IntegerNumber;
            input.Text = text;
            UIFactory.SetLayoutElement(input.GameObject, minWidth: 90, preferredWidth: 110, minHeight: 24, flexibleHeight: 0, flexibleWidth: 0);
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
