using System;
using GOILauncher.Multiplayer.Extensions;
using GOILauncher.Multiplayer.Unity;
using GOILauncher.Multiplayer.UI.Config;
using GOILauncher.Multiplayer.UI.Theme;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace GOILauncher.Multiplayer.UI.Pages
{
    /// <summary>
    /// 内嵌服务端页。结构跟客户端页对齐，也是"状态 → 操作"两段：
    /// 状态段在上、吃掉全部剩余高度，操作段（端口 + 启动/停止）固定在底部。
    /// 两页的按钮落在同一个位置，切页时手不用重新找。
    /// </summary>
    public class ServerPage : IPage
    {
        // 门面每轮联机都是新造的：Bind 时才有，Unbind 时清空。
        private IUnityServer _server;
        private readonly MultiplayerSettings _settings;
        private readonly Toast _toast;

        private Text _serverStateText;
        private InputFieldRef portInput;
        private ButtonRef _startButton;
        private ButtonRef _stopButton;
        private int _lastListenPort;

        public ServerPage(MultiplayerSettings settings, Toast toast)
        {
            _settings = settings;
            _toast = toast;
            _lastListenPort = DefaultPort;
            _settings.ServerPortChanged += OnDefaultPortChanged;
        }

        /// <summary>
        /// 绑定这一轮的内嵌服务端。服务端门面只有命令没有事件，所以这里只需要换引用，
        /// 不需要成对退订——但 Unbind 仍然要刷一次状态，否则界面会停在"运行中"。
        /// </summary>
        public void Bind(IUnityServer server)
        {
            if (server == null || _server != null)
                throw new InvalidOperationException("ServerPage: Bind and Unbind must alternate.");

            _server = server;
            RefreshServerState();
        }

        public void Unbind()
        {
            if (_server == null)
                return;

            _server = null;
            _lastListenPort = DefaultPort;
            RefreshServerState();
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
                pagesContainer, "ServerPage", false, false, true, true,
                0,
                new Vector4(Layout.SpaceMd, Layout.SpaceMd, Layout.SpaceMd, Layout.SpaceMd),
                Plugin.Theme.SurfaceBase);
            UIFactory.SetLayoutElement(Root, flexibleHeight: 9999, flexibleWidth: 9999);

            CreateStatusBlock();

            // 操作段贴底，和客户端页一样：状态段在上面撑满，这里只放"改端口 + 启动/停止"。
            GameObject section = UIFactory.CreateVerticalGroup(
                Root, "ServerActionSection", false, false, true, true,
                0, new Vector4(0, 0, 0, 0), Plugin.Theme.SurfaceBase);
            UIFactory.SetLayoutElement(section, minHeight: 0, flexibleHeight: 0, flexibleWidth: 9999);

            CreatePortRow(section);
            CreateActionRow(section);

            RefreshServerState();
        }

        /// <summary>
        /// 状态块。它回答"现在怎么样了"，是整个页面的结论，所以用最深的底色沉下去。
        /// 它撑满状态段，文字垂直居中——结论居中是读起来最舒服的位置。
        /// </summary>
        private void CreateStatusBlock()
        {
            GameObject block = UIFactory.CreateVerticalGroup(
                Root, "ServerStatusBlock", false, false, true, true,
                0, new Vector4(Layout.SpaceMd, Layout.SpaceMd, Layout.SpaceMd, Layout.SpaceMd),
                Plugin.Theme.SurfaceSunken);
            UIFactory.SetLayoutElement(block, minHeight: 64, flexibleHeight: 9999, flexibleWidth: 9999);
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(block, false, false, true, true, 0,
                childAlignment: TextAnchor.MiddleCenter);

            _serverStateText = UIFactory.CreateLabel(block, "ServerStateText", string.Empty,
                TextAnchor.MiddleCenter, Plugin.Theme.TextPrimary, false, Layout.FontSection);
            UIFactory.SetLayoutElement(_serverStateText.gameObject, minHeight: 24, preferredHeight: 24,
                flexibleHeight: 0, flexibleWidth: 9999);
        }

        /// <summary>
        /// 端口行和按钮行同属"操作段"。端口行始终在，运行时只是变成只读——
        /// 隐藏它会让整段矮 32，按钮跟着跳（见 RefreshServerState）。
        /// </summary>
        private void CreatePortRow(GameObject parent)
        {
            GameObject row = UiKit.CreateFieldRow(parent, "ServerPortRow");
            UiKit.CreateFieldLabel(row, "PortLabel", "启动端口");
            portInput = UiKit.CreateInputField(row, "ServerPortInput",
                InputFieldExtensions.FormatPort(DefaultPort));
            portInput.Component.contentType = InputField.ContentType.IntegerNumber;
            portInput.Text = InputFieldExtensions.FormatPort(DefaultPort);
        }

        private void CreateActionRow(GameObject parent)
        {
            GameObject actionRow = UIFactory.CreateHorizontalGroup(
                parent, "ServerActionRow", false, false, true, true, Layout.SpaceSm,
                new Vector4(0, 0, 0, 0), Plugin.Theme.SurfaceRaised);
            UIFactory.SetLayoutElement(actionRow, minHeight: Layout.PrimaryButtonHeight,
                preferredHeight: Layout.PrimaryButtonHeight, flexibleHeight: 0, flexibleWidth: 9999);

            _startButton = UiKit.CreateConfirmButton(actionRow, "StartServerButton", "启动");
            _startButton.OnClick += OnStartClicked;

            _stopButton = UiKit.CreateCancelButton(actionRow, "StopServerButton", "停止");
            _stopButton.OnClick += OnStopClicked;
        }

        private void OnStartClicked()
        {
            if (!IsLoaded || _server.IsRunning)
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
                _toast.Show($"服务端已启动，监听端口 {port}");
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError("Failed to start server: " + ex);
                _toast.Show($"服务端启动失败: {ex.Message}");
            }
            finally
            {
                RefreshServerState();
            }
        }

        private void OnStopClicked()
        {
            if (!IsLoaded || !_server.IsRunning)
            {
                RefreshServerState();
                return;
            }

            try
            {
                _server.Stop();
                _toast.Show("服务端已停止");
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError("Failed to stop server: " + ex);
                _toast.Show($"服务端停止失败: {ex.Message}");
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

            bool loaded = IsLoaded;
            bool running = loaded && _server.IsRunning;
            _startButton.Component.interactable = loaded && !running;
            _stopButton.Component.interactable = loaded && running;

            // 端口只在启动前能改。运行中不是"灰掉"也不是"藏起来"，而是只读：
            // 它此刻显示的是服务端真正监听的端口，值得看清楚（见 InputFieldExtensions.SetReadOnly）。
            //
            // 别改成 SetActive(false)：操作段的高度是"端口行 + 按钮行"算出来的，
            // 抽掉一行会让整段缩掉 32，按钮跟着往上跳、贴着上一块被裁掉一截。
            // 而且端口行消失后，这一段的底色也跟着少一块，看着像没画完。
            portInput.SetReadOnly(!loaded || running);

            _serverStateText.text = !loaded
                ? "联机未启用"
                : running
                ? $"运行中 · 监听端口 {_lastListenPort}"
                : "未启动";
            _serverStateText.color = running ? Plugin.Theme.TextAccent
                : loaded ? Plugin.Theme.TextPrimary : Plugin.Theme.TextSecondary;
        }

        // The settings page owns the default. Overwrite the field only while the server is idle: once it
        // is running the field shows the port it is actually listening on, and it is not editable anyway.
        private void OnDefaultPortChanged(int port)
        {
            // 空闲时（包括联机没加载）端口框就是默认值的展示位；开着服时它显示真正监听的端口，不动。
            if (portInput == null || (_server != null && _server.IsRunning))
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
            get { return _settings.ServerPort; }
        }

        /// <summary>这一轮的门面在不在。不在就是联机没加载，这一页只能看不能操作。</summary>
        private bool IsLoaded
        {
            get { return _server != null; }
        }
    }
}