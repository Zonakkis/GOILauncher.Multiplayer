using GOILauncher.Multiplayer.UI.Components;
using UniverseLib;
using UniverseLib.UI;
using UniverseLib.UI.Models;
using UnityEngine;
using UnityEngine.UI;

namespace GOILauncher.Multiplayer.UI.Pages
{
    internal class ServerPageView : IPageView
    {
        private IServerControlUiComponent serverControlComponent;
        private InputFieldRef portInput;

        public string PageName => "Server";

        public GameObject Root { get; private set; }

        public ServerPageView(GameObject pagesContainer)
        {
            Root = ConstructServerPage(pagesContainer);
        }

        public void SetActive(bool active)
        {
            if (Root != null)
                Root.SetActive(active);
        }

        public void Bind(IServerControlUiComponent serverControlComponent)
        {
            this.serverControlComponent = serverControlComponent;
            SyncPortInput();
        }

        private GameObject ConstructServerPage(GameObject pagesContainer)
        {
            GameObject serverPage = UIFactory.CreateVerticalGroup(
                pagesContainer,
                "ServerPage",
                false,
                false,
                true,
                true,
                6,
                new Vector4(8, 8, 8, 8),
                new Color(0.12f, 0.12f, 0.12f, 0.95f));
            UIFactory.SetLayoutElement(serverPage, flexibleHeight: 9999, flexibleWidth: 9999);

            GameObject titleRow = UIFactory.CreateHorizontalGroup(
                serverPage,
                "ServerTitleRow",
                false,
                false,
                true,
                true,
                6,
                new Vector4(8, 4, 8, 4),
                new Color(0.16f, 0.16f, 0.16f, 1f));
            UIFactory.SetLayoutElement(titleRow, minHeight: 34, flexibleHeight: 0);

            Text titleText = UIFactory.CreateLabel(titleRow, "ServerTitleText", "服务端控制", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(titleText.gameObject, minHeight: 24, flexibleHeight: 0, flexibleWidth: 9999);

            GameObject spacer = UIFactory.CreateUIObject("ServerBottomSpacer", serverPage);
            UIFactory.SetLayoutElement(spacer, minHeight: 0, flexibleHeight: 9999, flexibleWidth: 9999);

            GameObject controlSection = UIFactory.CreateVerticalGroup(
                serverPage,
                "ServerControlSection",
                false,
                false,
                true,
                true,
                4,
                new Vector4(8, 6, 8, 6),
                new Color(0.14f, 0.14f, 0.14f, 1f));
            UIFactory.SetLayoutElement(controlSection, minHeight: 80, flexibleHeight: 0, flexibleWidth: 9999);

            GameObject portRow = UIFactory.CreateHorizontalGroup(
                controlSection,
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

            portInput = UIFactory.CreateInputField(portRow, "ServerPortInput", "9027");
            UIFactory.SetLayoutElement(portInput.GameObject, minHeight: 24, flexibleHeight: 0, flexibleWidth: 9999);

            GameObject actionRow = UIFactory.CreateHorizontalGroup(
                controlSection,
                "ServerActionRow",
                false,
                false,
                true,
                true,
                6,
                new Vector4(6, 4, 6, 4),
                new Color(0.16f, 0.16f, 0.16f, 1f));
            UIFactory.SetLayoutElement(actionRow, minHeight: 32, flexibleHeight: 0);

            ButtonRef startButton = UIFactory.CreateButton(actionRow, "StartServerButton", "启动");
            UIFactory.SetLayoutElement(startButton.Component.gameObject, minWidth: 100, minHeight: 24, flexibleHeight: 0, flexibleWidth: 9999);
            RuntimeHelper.SetColorBlock(
                startButton.Component,
                new Color(0.22f, 0.38f, 0.28f),
                new Color(0.26f, 0.44f, 0.32f),
                new Color(0.14f, 0.24f, 0.18f),
                new Color(0.2f, 0.2f, 0.2f));
            startButton.OnClick += OnStartClicked;

            ButtonRef stopButton = UIFactory.CreateButton(actionRow, "StopServerButton", "停止");
            UIFactory.SetLayoutElement(stopButton.Component.gameObject, minWidth: 100, minHeight: 24, flexibleHeight: 0, flexibleWidth: 9999);
            RuntimeHelper.SetColorBlock(
                stopButton.Component,
                new Color(0.38f, 0.22f, 0.22f),
                new Color(0.44f, 0.26f, 0.26f),
                new Color(0.24f, 0.14f, 0.14f),
                new Color(0.2f, 0.2f, 0.2f));
            stopButton.OnClick += OnStopClicked;

            SyncPortInput();

            return serverPage;
        }

        private void OnStartClicked()
        {
            int port = ParsePortOrDefault();

            if (serverControlComponent != null)
            {
                serverControlComponent.StartServer(port);
                return;
            }

            Plugin.Logger.LogInfo("Server start clicked on port: " + port);
        }

        private void OnStopClicked()
        {
            if (serverControlComponent != null)
            {
                serverControlComponent.StopServer();
                return;
            }

            Plugin.Logger.LogInfo("Server stop clicked.");
        }

        private int ParsePortOrDefault()
        {
            int defaultPort = 9027;
            if (portInput == null)
                return defaultPort;

            string text = portInput.Text == null ? string.Empty : portInput.Text.Trim();
            if (!int.TryParse(text, out int port))
                port = defaultPort;

            if (port < 1 || port > 65535)
                port = defaultPort;

            portInput.Text = port.ToString();
            return port;
        }

        private void SyncPortInput()
        {
            if (portInput == null)
                return;

            int port = 9027;
            if (serverControlComponent != null)
            {
                port = serverControlComponent.GetListenPort();
                if (port < 1 || port > 65535)
                    port = 9027;
            }

            portInput.Text = port.ToString();
        }
    }
}