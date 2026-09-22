using System;
using System.Collections.Generic;
using GOILauncher.Multiplayer.Core.Data.Constants;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Extensions;
using GOILauncher.Multiplayer.Unity;
using GOILauncher.Multiplayer.UI.Theme;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.Input;
using UniverseLib.UI;
using UniverseLib.UI.Models;
using UniverseLib.UI.Panels;

namespace GOILauncher.Multiplayer.UI
{
    /// <summary>
    /// One reusable editor/password window. Only the client facade performs commands.
    ///
    /// 三个形态（创建 / 编辑 / 输入密码）共用这一个窗口，字段靠显隐切换。
    /// 布局上有三条规矩：
    /// - 表单统一是"字段名：输入框"。字段名固定在左边同一列，输入框因此左边缘对齐；
    ///   placeholder 只补充格式（"0 表示不限"），不再重复字段名。
    /// - 密码只有一个输入框，不留空就是不改。以前那组"保持 / 设置新密码 / 移除"
    ///   把一件事拆成三个按钮，而其中两个的默认值本来就是同一个（什么都不填）。
    /// - 标题只说动词，房名和人数这些细节放副标题；副标题只在"加入房间"时用——
    ///   创建和编辑的房名就在下面的输入框里，再说一遍是重复。
    /// </summary>
    public sealed class RoomDialogUI : PanelBase
    {
        // 门面每轮联机都是新造的：Bind 时才有，Unbind 时清空。
        private IUnityClient _client;
        private RoomOperation _operation;
        private int _roomId;
        private bool _submitted;
        private int _keyboardGuardFrame = -1;
        public bool BlocksGameplayShortcuts => Enabled || _keyboardGuardFrame == Time.frameCount;
        private Text _heading;
        private Text _subtitle;
        private Text _error;
        private GameObject _nameRow, _capacityRow;
        private InputFieldRef _name, _capacity, _password;
        private ButtonRef _submit, _cancel;
        public event Action<bool> ActiveChanged;
        public override string Name => "房间操作";
        public override int MinWidth => 440;
        // 弹窗只在尺寸小于 MinHeight 时被撑大、从不收缩（PanelBase.EnsureValidSize），
        // 所以这个值按最高的形态（创建/编辑：三行表单）给，按最矮的给会把表单裁掉。
        // 矮形态多出来的高度由表单和按钮之间那段弹性空白吃掉，按钮始终贴着底边。
        public override int MinHeight => 232;
        public override Vector2 DefaultAnchorMin => new Vector2(0.5f, 0.5f);
        public override Vector2 DefaultAnchorMax => new Vector2(0.5f, 0.5f);
        public override Vector2 DefaultPosition => new Vector2(-220, 150);
        public override bool CanDragAndResize => true;

        public RoomDialogUI(UIBase owner) : base(owner)
        {
            CreateContent();
            SetActive(false);
        }

        public void Bind(IUnityClient client)
        {
            if (client == null || _client != null)
                throw new InvalidOperationException("RoomDialogUI: Bind and Unbind must alternate.");

            _client = client;
            // 断开时顺手收窗：房间操作只对当前连接有意义，连接没了窗口就该没了。
            _client.Disconnected += OnDisconnected;
            _client.CurrentRoomChanged += OnCurrentRoomChanged;
            _client.RoomOperationCompleted += OnCompleted;
        }

        public void Unbind()
        {
            if (_client == null)
                return;

            _client.Disconnected -= OnDisconnected;
            _client.CurrentRoomChanged -= OnCurrentRoomChanged;
            _client.RoomOperationCompleted -= OnCompleted;
            _client = null;
            SetActive(false);
        }

        private void OnDisconnected(string reason)
        {
            SetActive(false);
        }
        protected override void ConstructPanelContent() { }
        protected override PanelDragger CreatePanelDragger() { return new ResponsivePanelDragger(this); }

        private void CreateContent()
        {
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(ContentRoot, false, false, true, true, 0);

            GameObject body = UIFactory.CreateVerticalGroup(
                ContentRoot, "DialogBody", false, false, true, true,
                Layout.SpaceSm,
                // UIFactory 按 (top, bottom, left, right) 取这个 Vector4，不是直觉的
                // (left, top, right, bottom)。写成后者上下会互换，正文整体偏下。
                new Vector4(Layout.SpaceMd, Layout.SpaceLg, Layout.SpaceLg, Layout.SpaceLg),
                Plugin.Theme.SurfaceRaised);
            UIFactory.SetLayoutElement(body, flexibleHeight: 9999, flexibleWidth: 9999);

            _heading = UIFactory.CreateLabel(body, "Heading", string.Empty, TextAnchor.MiddleLeft,
                Plugin.Theme.TextPrimary, false, Layout.FontSection);
            _heading.fontStyle = FontStyle.Bold;
            UIFactory.SetLayoutElement(_heading.gameObject, minHeight: 24, preferredHeight: 24,
                flexibleHeight: 0, flexibleWidth: 9999);

            // 只有"加入房间"需要副标题：那时表单里没有房名，得靠它说明加的是哪一间。
            // 其它两个形态它是空的，整行隐藏、不占位置。
            _subtitle = UIFactory.CreateLabel(body, "Subtitle", string.Empty, TextAnchor.UpperLeft,
                Plugin.Theme.TextSecondary, false, Layout.FontDetail);
            _subtitle.horizontalOverflow = HorizontalWrapMode.Wrap;
            UIFactory.SetLayoutElement(_subtitle.gameObject, minHeight: 0, flexibleHeight: 0, flexibleWidth: 9999);

            _name = CreateField(body, "Name", "房间名", "最多 " + RoomConstants.MaxNameLength + " 个字符",
                RoomConstants.MaxNameLength, out _nameRow);
            _capacity = CreateField(body, "Capacity", "人数上限", "0 表示不限", 0, out _capacityRow);
            _capacity.Component.contentType = InputField.ContentType.IntegerNumber;
            _password = CreateField(body, "Password", "密码", "输入房间密码",
                RoomConstants.MaxPasswordLength, out _);
            _password.Component.contentType = InputField.ContentType.Password;

            // 弹性空白：三个形态的行数不一样，窗口高度按最高的那个定，
            // 矮的形态把多出来的高度放在表单和按钮之间，按钮才不会浮在半空。
            GameObject spacer = UIFactory.CreateUIObject("Spacer", body);
            UIFactory.SetLayoutElement(spacer, minHeight: 0, flexibleHeight: 9999, flexibleWidth: 9999);

            CreateErrorRow(body);
            CreateButtonRow(body);

            SetActive(false);
        }

        /// <summary>
        /// 错误/进度行。固定行高，没有内容时整行隐藏。
        ///
        /// 以前它是 minHeight 52 且 flexibleHeight 9999 的弹性区，于是：
        /// 打开弹窗时它就白占一块高度，提交时那行"正在等待服务器确认…"还会把按钮往下推。
        /// </summary>
        private void CreateErrorRow(GameObject body)
        {
            _error = UIFactory.CreateLabel(body, "Status", string.Empty, TextAnchor.UpperLeft,
                Plugin.Theme.TextDanger, false, Layout.FontDetail);
            _error.horizontalOverflow = HorizontalWrapMode.Wrap;
            UIFactory.SetLayoutElement(_error.gameObject, minHeight: 20, preferredHeight: 20,
                flexibleHeight: 0, flexibleWidth: 9999);
            _error.gameObject.SetActive(false);
        }

        private void CreateButtonRow(GameObject body)
        {
            GameObject buttons = UIFactory.CreateHorizontalGroup(
                body, "Buttons", false, false, true, true, Layout.SpaceMd,
                // 上下都不留：这一行上面已经有那段弹性空白了，再给内边距等于白给。
                new Vector4(0, 0, 0, 0), Plugin.Theme.SurfaceRaised);
            UIFactory.SetLayoutElement(buttons, minHeight: Layout.PrimaryButtonHeight,
                preferredHeight: Layout.PrimaryButtonHeight, flexibleHeight: 0, flexibleWidth: 9999);

            _submit = UiKit.CreateConfirmButton(buttons, "Submit", "确认");
            _submit.OnClick += Submit;
            _cancel = UiKit.CreateButton(buttons, "Cancel", "取消");
            _cancel.OnClick += () => SetActive(false);
        }

        /// <summary>
        /// 表单一行：左边字段名，右边输入框。
        /// 字段名固定宽度，所以三行的输入框左边缘天然对齐；placeholder 只补充格式。
        /// </summary>
        private static InputFieldRef CreateField(GameObject parent, string id, string label, string placeholder,
            int characterLimit, out GameObject row)
        {
            row = UiKit.CreateFieldRow(parent, id + "Row");
            UiKit.CreateFieldLabel(row, id + "Label", label);

            InputFieldRef input = UiKit.CreateInputField(row, id + "Input", placeholder);
            if (characterLimit > 0)
                input.Component.characterLimit = characterLimit;

            return input;
        }

        public void ShowCreate()
        {
            if (!CanOpen) return;
            _operation = RoomOperation.Create; _roomId = 0;
            _heading.text = "创建房间";
            SetSubtitle(null);
            _name.Text = ""; _capacity.Text = "0";
            Open();
        }

        public void ShowEdit()
        {
            if (!CanOpen) return;
            var current = _client.CurrentRoom;
            if (current == null || current.IsLobby || current.OwnerPlayerId != _client.LocalPlayer.Id) return;
            _operation = RoomOperation.Update; _roomId = current.Id;
            _heading.text = "编辑房间";
            SetSubtitle(null);
            _name.Text = current.Name; _capacity.Text = current.MaxPlayers.ToString();
            Open();
        }

        public void ShowJoin(RoomInfo room)
        {
            if (!CanOpen || room == null) return;
            _operation = RoomOperation.Join; _roomId = room.Id;
            _heading.text = "加入房间";
            SetSubtitle(room.Name + " · " + room.PlayerCount + " / "
                + (room.MaxPlayers == 0 ? "∞" : room.MaxPlayers.ToString()) + " 人");
            Open();
        }

        private bool CanOpen => _client != null && _client.IsConnected && _client.CurrentRoom != null && !_client.IsRoomOperationPending;

        private void Open()
        {
            _submitted = false; _password.Text = ""; SetError(null);
            bool join = _operation == RoomOperation.Join;

            _nameRow.SetActive(!join); _capacityRow.SetActive(!join);
            SetPasswordPlaceholder(join
                ? "输入房间密码"
                : _operation == RoomOperation.Update ? "留空表示不修改" : "留空表示不设密码");

            _submit.ButtonText.text = join ? "加入" : _operation == RoomOperation.Create ? "创建并加入" : "保存";
            SetActive(true);
            RefreshControls();
            var input = join ? _password : _name;
            input.Component.Select(); input.Component.ActivateInputField();
        }

        /// <summary>
        /// 同一个输入框在三种形态下含义不同，靠 placeholder 说清楚"留空算什么"。
        /// 这比再来一组分段按钮便宜，也不会让"保持"和"移除"看起来像两个对等的选项。
        /// </summary>
        private void SetPasswordPlaceholder(string text)
        {
            if (_password.Component.placeholder is Text placeholder)
                placeholder.text = text;
        }

        private void SetSubtitle(string text)
        {
            _subtitle.text = text;
            _subtitle.gameObject.SetActive(!string.IsNullOrEmpty(text));
        }

        /// <summary><paramref name="message"/> 为 null 表示没有消息，整行隐藏而不是留一块空白。</summary>
        private void SetError(string message)
        {
            _error.text = message ?? string.Empty;
            _error.gameObject.SetActive(!string.IsNullOrEmpty(message));
        }

        public override void SetActive(bool active)
        {
            bool changed = Enabled != active;
            if (changed) _keyboardGuardFrame = Time.frameCount;
            if (!active)
            {
                _submitted = false;
                if (_password != null) { _password.Component.DeactivateInputField(); _password.Text = ""; }
                if (_name != null) _name.Component.DeactivateInputField();
                if (_capacity != null) _capacity.Component.DeactivateInputField();
            }
            base.SetActive(active);
            if (changed) ActiveChanged?.Invoke(active);
        }

        public override void Update()
        {
            if (!Enabled) return;
            if (InputManager.GetKeyDown(KeyCode.Escape)) { SetActive(false); return; }
            if (InputManager.GetKeyDown(KeyCode.Tab) && !_submitted)
            {
                var inputs = new List<InputFieldRef>();
                foreach (var input in new[] { _name, _capacity, _password })
                    if (input.GameObject.activeInHierarchy) inputs.Add(input);
                int index = inputs.FindIndex(input => input.Component.isFocused);
                int direction = InputManager.GetKey(KeyCode.LeftShift) || InputManager.GetKey(KeyCode.RightShift) ? -1 : 1;
                var next = inputs[(index + direction + inputs.Count) % inputs.Count];
                next.Component.Select(); next.Component.ActivateInputField();
            }
            if (InputManager.GetKeyDown(KeyCode.Return) || InputManager.GetKeyDown(KeyCode.KeypadEnter)) Submit();
        }

        private void Submit()
        {
            if (_submitted || _client == null || !_client.IsConnected || _client.IsRoomOperationPending) return;
            int maxPlayers = 0;
            if (_operation != RoomOperation.Join)
            {
                if (string.IsNullOrWhiteSpace(_name.Text)) { SetError("房间名不能为空。"); return; }
                if (!int.TryParse(_capacity.Text, out maxPlayers) || maxPlayers < 0)
                { SetError("人数上限必须是非负整数，0 表示不限。"); return; }
            }
            _submitted = true; SetError("正在等待服务器确认…");
            RefreshControls();
            if (_operation == RoomOperation.Create) _client.CreateRoom(_name.Text, _password.Text, maxPlayers);
            else if (_operation == RoomOperation.Join) _client.JoinRoom(_roomId, _password.Text);
            else
            {
                // 编辑时密码只有两种：留空 = 不动，填了 = 新密码。
                // 门面仍然支持 RoomPasswordChange.Remove，只是界面上没有这条路。
                RoomPasswordChange change = string.IsNullOrEmpty(_password.Text)
                    ? RoomPasswordChange.Keep
                    : RoomPasswordChange.Set;
                _client.UpdateRoom(_roomId, _name.Text, maxPlayers, change, _password.Text);
            }
        }

        private void RefreshControls()
        {
            bool editable = !_submitted;
            _submit.Component.interactable = editable;
            _cancel.ButtonText.text = editable ? "取消" : "关闭";
            _name.Component.interactable = editable;
            _capacity.Component.interactable = editable;
            _password.Component.interactable = editable;
        }

        private void OnCompleted(RoomOperationResult result)
        {
            if (!Enabled || !_submitted || result.Operation != _operation) return;
            if (result.Error == RoomError.Busy && _client.IsRoomOperationPending) return;
            _submitted = false;
            if (result.IsSuccess) SetActive(false);
            else { SetError(RoomUiText.Error(result.Error)); RefreshControls(); }
        }

        private void OnCurrentRoomChanged()
        {
            if (!Enabled) return;
            var room = _client.CurrentRoom;
            if (room == null || (_operation == RoomOperation.Update && (room.Id != _roomId || room.OwnerPlayerId != _client.LocalPlayer.Id)))
                SetActive(false);
        }
    }

    internal static class RoomUiText
    {
        public static string Error(RoomError error)
        {
            switch (error)
            {
                case RoomError.NotReady: return "尚未进入服务器房间，请等待连接完成。";
                case RoomError.Busy: return "另一个房间操作尚未完成。";
                case RoomError.RoomNotFound: return "房间已不存在，请刷新列表。";
                case RoomError.IncorrectPassword: return "房间密码不正确，请重新输入。";
                case RoomError.RoomFull: return "房间人数已满。";
                case RoomError.NotOwner: return "只有当前房主可以修改房间。";
                case RoomError.InvalidName: return "房间名不能为空，且不能超过 " + RoomConstants.MaxNameLength + " 个字符。";
                case RoomError.InvalidCapacity: return "人数上限不能为负数或低于现有人数；0 表示不限。";
                case RoomError.InvalidPassword: return "密码无效，请检查密码设置。";
                case RoomError.StaleMembership: return "所在房间已变化，请重试。";
                default: return "不允许进行此房间操作。";
            }
        }
    }
}
