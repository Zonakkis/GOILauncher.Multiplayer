using System;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Extensions;
using GOILauncher.Multiplayer.Unity;
using GOILauncher.Multiplayer.Unity.Config;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace GOILauncher.Multiplayer.UI.Pages
{
    public class ServerPage : IPage
    {
        private readonly IUnityServer _server;
        private readonly MultiplayerSettings _settings;
        private readonly ILogger<ServerPage> _logger;
        private readonly Toast _toast;

        private Text _serverStateText;
        private InputFieldRef portInput;
        private ButtonRef _startButton;
        private ButtonRef _stopButton;
        private int _lastListenPort;

        public ServerPage(
            IUnityServer unityServer,
            MultiplayerSettings settings,
            ILogger<ServerPage> logger,
            Toast toast)
        {
            _server = unityServer;
            _settings = settings;
            _logger = logger;
            _toast = toast;
            _lastListenPort = DefaultPort;
            _settings.EnabledChanged += OnMultiplayerEnabledChanged;
            _settings.ServerPortChanged += OnDefaultPortChanged;
        }

        public GameObject Root { get; private set; }

        public void SetActive(bool active)
        {
            Root?.SetActive(active);

            if (active)
                RefreshServerState();
        }

        public void CreateContent(GameObject pagesContainer)
        {
            Root = UIFactory.CreateVerticalGroup(
                pagesContainer,
                "ServerPage",
                false,
                false,
                true,
                true,
                6,
                new Vector4(8, 8, 8, 8),
                new Color(0.12f, 0.12f, 0.12f, 0.95f));
            UIFactory.SetLayoutElement(Root, flexibleHeight: 9999, flexibleWidth: 9999);

            GameObject topArea = UIFactory.CreateVerticalGroup(
                Root,
                "ServerTopArea",
                false,
                false,
                true,
                true,
                0,
                new Vector4(8, 8, 8, 8),
                new Color(0.16f, 0.16f, 0.16f, 1f));
            UIFactory.SetLayoutElement(topArea, minHeight: 34, flexibleHeight: 9999, flexibleWidth: 9999);
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(topArea, false, false, true, true, 0, childAlignment: TextAnchor.MiddleCenter);

            _serverStateText = UIFactory.CreateLabel(topArea, "ServerStateText", "\u670d\u52a1\u7aef\u672a\u542f\u52a8", TextAnchor.MiddleCenter);
            UIFactory.SetLayoutElement(_serverStateText.gameObject, minHeight: 24, preferredHeight: 24, flexibleHeight: 0, flexibleWidth: 9999);

            GameObject portRow = UIFactory.CreateHorizontalGroup(
                Root,
                "ServerPortRow",
                false,
                false,
                true,
                true,
                6,
                new Vector4(6, 4, 6, 4),
                new Color(0.16f, 0.16f, 0.16f, 1f));
            UIFactory.SetLayoutElement(portRow, minHeight: 30, flexibleHeight: 0);

            Text portLabel = UIFactory.CreateLabel(portRow, "PortLabel", "\u542f\u52a8\u7aef\u53e3", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(portLabel.gameObject, minWidth: 72, preferredWidth: 80, minHeight: 22, flexibleHeight: 0, flexibleWidth: 0);

            portInput = UIFactory.CreateInputField(portRow, "ServerPortInput", InputFieldExtensions.FormatPort(DefaultPort));
            portInput.Component.contentType = InputField.ContentType.IntegerNumber;
            portInput.Text = InputFieldExtensions.FormatPort(DefaultPort);
            UIFactory.SetLayoutElement(portInput.GameObject, minHeight: 24, flexibleHeight: 0, flexibleWidth: 9999);

            GameObject actionRow = UIFactory.CreateHorizontalGroup(
                Root,
                "ServerActionRow",
                false,
                false,
                true,
                true,
                6,
                new Vector4(6, 4, 6, 4),
                new Color(0.16f, 0.16f, 0.16f, 1f));
            UIFactory.SetLayoutElement(actionRow, minHeight: 32, flexibleHeight: 0);

            _startButton = UIFactory.CreateButton(actionRow, "StartServerButton", "\u542f\u52a8");
            UIFactory.SetLayoutElement(_startButton.Component.gameObject, minWidth: 100, minHeight: 24, flexibleHeight: 0, flexibleWidth: 9999);
            _startButton.SetConfirm();
            _startButton.OnClick += OnStartClicked;

            _stopButton = UIFactory.CreateButton(actionRow, "StopServerButton", "\u505c\u6b62");
            UIFactory.SetLayoutElement(_stopButton.Component.gameObject, minWidth: 100, minHeight: 24, flexibleHeight: 0, flexibleWidth: 9999);
            _stopButton.SetCancel();
            _stopButton.OnClick += OnStopClicked;

            RefreshServerState();
        }

        private void OnStartClicked()
        {
            if (!IsMultiplayerEnabled || _server.IsRunning)
            {
                RefreshServerState();
                return;
            }

            if (!TryGetListenPort(out int port))
                return;

            try
            {
                _server.Start(port);
                _lastListenPort = port;
                _toast.Show($"\u670d\u52a1\u7aef\u5df2\u542f\u52a8\uff0c\u76d1\u542c\u7aef\u53e3 {port}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to start server");
                _toast.Show($"\u670d\u52a1\u7aef\u542f\u52a8\u5931\u8d25: {ex.Message}");
            }
            finally
            {
                RefreshServerState();
            }
        }

        private void OnStopClicked()
        {
            if (!IsMultiplayerEnabled || !_server.IsRunning)
            {
                RefreshServerState();
                return;
            }

            try
            {
                _server.Stop();
                _toast.Show("\u670d\u52a1\u7aef\u5df2\u505c\u6b62");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to stop server");
                _toast.Show($"\u670d\u52a1\u7aef\u505c\u6b62\u5931\u8d25: {ex.Message}");
            }
            finally
            {
                RefreshServerState();
            }
        }

        private void RefreshServerState()
        {
            if (_serverStateText == null || _startButton == null || _stopButton == null || portInput == null)
                return;

            bool multiplayerEnabled = IsMultiplayerEnabled;
            bool running = multiplayerEnabled && _server.IsRunning;
            _startButton.Component.interactable = multiplayerEnabled && !running;
            _stopButton.Component.interactable = multiplayerEnabled && running;
            portInput.Component.interactable = multiplayerEnabled && !running;
            _serverStateText.text = !multiplayerEnabled
                ? "\u8054\u673a\u5df2\u7981\u7528"
                : running
                ? $"\u670d\u52a1\u7aef\u5df2\u542f\u52a8\uff0c\u76d1\u542c\u7aef\u53e3 {_lastListenPort}"
                : "\u670d\u52a1\u7aef\u672a\u542f\u52a8";
        }

        private void OnMultiplayerEnabledChanged(bool enabled)
        {
            RefreshServerState();
        }

        // The settings page owns the default. Overwrite the field only while the server is idle: once it
        // is running the field shows the port it is actually listening on, and it is not editable anyway.
        private void OnDefaultPortChanged(int port)
        {
            if (portInput == null || _server.IsRunning)
                return;

            portInput.Text = InputFieldExtensions.FormatPort(port);
        }

        private bool TryGetListenPort(out int port)
        {
            if (portInput.TryReadPort(out port))
                return true;

            _toast.Show(InputFieldExtensions.InvalidPortMessage);
            return false;
        }

        /// <summary>The listen port the page starts from, owned by the settings page.</summary>
        private int DefaultPort
        {
            get { return _settings == null ? MultiplayerSettings.DefaultServerPort : _settings.ServerPort; }
        }

        private bool IsMultiplayerEnabled
        {
            get { return _settings == null || _settings.Enabled; }
        }
    }
}
