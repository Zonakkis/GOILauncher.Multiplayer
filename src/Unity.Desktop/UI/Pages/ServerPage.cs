using UniverseLib.UI;
using UniverseLib.UI.Models;
using UnityEngine;
using UnityEngine.UI;
using GOILauncher.Multiplayer.Unity;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Extensions;
using System;

namespace GOILauncher.Multiplayer.UI.Pages
{
    public class ServerPage : IPage
    {
        private const int DefaultPort = 9027;
        public Toast Toast { get; set; }
        public ILogger<ServerPage> Logger { get; set; }
        public IUnityServer _server;
        public GameObject Root { get; private set; }
        private Text _serverStateText;
        private InputFieldRef portInput;
        private ButtonRef _startButton;
        private ButtonRef _stopButton;

        public ServerPage(IUnityServer unityServer, ILogger<ServerPage> logger, Toast toast)
        {
            _server = unityServer;
            Logger = logger;
            Toast = toast;
        }

        public void SetActive(bool active)
        {
            Root?.SetActive(active);
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

            _serverStateText = UIFactory.CreateLabel(topArea, "ServerStateText", "服务端未启动", TextAnchor.MiddleCenter);
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

            Text portLabel = UIFactory.CreateLabel(portRow, "PortLabel", "启动端口", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(portLabel.gameObject, minWidth: 72, preferredWidth: 80, minHeight: 22, flexibleHeight: 0, flexibleWidth: 0);

            portInput = UIFactory.CreateInputField(portRow, "ServerPortInput", DefaultPort.ToString());
            portInput.Component.contentType = InputField.ContentType.IntegerNumber;
            portInput.Component.text = DefaultPort.ToString();
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

            _startButton = UIFactory.CreateButton(actionRow, "StartServerButton", "启动");
            UIFactory.SetLayoutElement(_startButton.Component.gameObject, minWidth: 100, minHeight: 24, flexibleHeight: 0, flexibleWidth: 9999);
            _startButton.SetConfirm();
            _startButton.OnClick += OnStartClicked;

            _stopButton = UIFactory.CreateButton(actionRow, "StopServerButton", "停止");
            UIFactory.SetLayoutElement(_stopButton.Component.gameObject, minWidth: 100, minHeight: 24, flexibleHeight: 0, flexibleWidth: 9999);
            _stopButton.SetCancel();
            _stopButton.OnClick += OnStopClicked;

            SetConnectionState(false);
        }

        private void SetConnectionState(bool connected)
        {
            _startButton.Component.interactable = !connected;
            _stopButton.Component.interactable = connected;
            _serverStateText.text = connected ? $"服务端已启动，监听端口 {GetListenPort()}" : "服务端未启动";
        }

        private void OnStartClicked()
        {
            try
            {
                int port = GetListenPort();
                _server.Start(port);
                Toast.Show($"服务端已启动，监听端口 {port}");
                SetConnectionState(true);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to start server", ex);
                Toast.Show($"服务端启动失败: {ex.Message}");
            }
        }

        private void OnStopClicked()
        {
            try
            {
                _server.Stop();
                Toast.Show("服务端已停止");
                SetConnectionState(false);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to stop server", ex);
                Toast.Show($"服务端停止失败: {ex.Message}");
            }
        }

        private int GetListenPort()
        {
            if (portInput == null)
                return DefaultPort;

            string text = portInput.Text == null ? string.Empty : portInput.Text.Trim();
            if (!int.TryParse(text, out int port))
                port = DefaultPort;

            if (port < 1 || port > 65535)
                port = DefaultPort;

            portInput.Text = port.ToString();
            return port;
        }
    }
}