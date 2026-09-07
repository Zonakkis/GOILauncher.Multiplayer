using System;
using System.Collections.Generic;
using GOILauncher.Multiplayer.Core.Data.Constants;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Extensions;
using GOILauncher.Multiplayer.Unity;
using GOILauncher.Multiplayer.Unity.Config;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Models;
using UniverseLib.UI.Panels;

namespace GOILauncher.Multiplayer.UI
{
    /// <summary>One reusable editor/password window. Only the client facade performs commands.</summary>
    public sealed class RoomDialogUI : PanelBase
    {
        private readonly IUnityClient _client;
        private readonly MultiplayerSettings _settings;
        private RoomOperation _operation;
        private int _roomId;
        private RoomPasswordChange _passwordChange;
        private bool _submitted;
        private int _keyboardGuardFrame = -1;
        public bool BlocksGameplayShortcuts => Enabled || _keyboardGuardFrame == Time.frameCount;
        private Text _heading;
        private Text _error;
        private GameObject _nameRow, _capacityRow, _passwordRow, _passwordActions;
        private InputFieldRef _name, _capacity, _password;
        private ButtonRef _submit, _cancel, _keepPassword, _setPassword, _removePassword;
        public event Action<bool> ActiveChanged;
        public override string Name => "房间操作";
        public override int MinWidth => 460;
        public override int MinHeight => 360;
        public override Vector2 DefaultAnchorMin => new Vector2(0.5f, 0.5f);
        public override Vector2 DefaultAnchorMax => new Vector2(0.5f, 0.5f);
        public override Vector2 DefaultPosition => new Vector2(-230, 180);
        public override bool CanDragAndResize => true;

        public RoomDialogUI(UIBase owner, IUnityClient client, MultiplayerSettings settings) : base(owner)
        {
            _client = client; _settings = settings;
            CreateContent();
            _client.Disconnected += reason => SetActive(false);
            _client.CurrentRoomChanged += OnCurrentRoomChanged;
            _client.RoomOperationCompleted += OnCompleted;
            _settings.EnabledChanged += enabled => { if (!enabled) SetActive(false); };
            SetActive(false);
        }
        protected override void ConstructPanelContent() { }
        protected override PanelDragger CreatePanelDragger() { return new ResponsivePanelDragger(this); }
        private void CreateContent()
        {
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(ContentRoot, false, false, true, true, 8, 12, 12, 10, 10);
            _heading = UIFactory.CreateLabel(ContentRoot, "Heading", "", TextAnchor.MiddleLeft);
            _heading.supportRichText = false;
            UIFactory.SetLayoutElement(_heading.gameObject, minHeight: 38, flexibleHeight: 0);
            _name = CreateInput("Name", "房间名", "输入房间名称", out _nameRow);
            _name.Component.characterLimit = RoomConstants.MaxNameLength;
            _capacity = CreateInput("Capacity", "人数上限", "0 表示不限", out _capacityRow);
            _capacity.Component.contentType = InputField.ContentType.IntegerNumber;
            _passwordActions = UIFactory.CreateHorizontalGroup(ContentRoot, "PasswordActions", false, false, true, true, 6);
            _keepPassword = CreatePasswordAction("Keep", "保持密码", RoomPasswordChange.Keep);
            _setPassword = CreatePasswordAction("Set", "设置新密码", RoomPasswordChange.Set);
            _removePassword = CreatePasswordAction("Remove", "移除密码", RoomPasswordChange.Remove);
            _password = CreateInput("Password", "密码", "创建时留空则不设密码", out _passwordRow);
            _password.Component.contentType = InputField.ContentType.Password;
            _password.Component.characterLimit = RoomConstants.MaxPasswordLength;
            _error = UIFactory.CreateLabel(ContentRoot, "Status", "", TextAnchor.UpperLeft);
            _error.color = new Color(1f, 0.65f, 0.5f);
            _error.supportRichText = false;
            UIFactory.SetLayoutElement(_error.gameObject, minHeight: 52, flexibleHeight: 9999);
            var buttons = UIFactory.CreateHorizontalGroup(ContentRoot, "Buttons", false, false, true, true, 8);
            _submit = UIFactory.CreateButton(buttons, "Submit", "确认");
            _submit.SetConfirm();
            UIFactory.SetLayoutElement(_submit.Component.gameObject, minHeight: 32, flexibleWidth: 9999);
            _submit.OnClick += Submit;
            _cancel = UIFactory.CreateButton(buttons, "Cancel", "取消");
            _cancel.SetCancel();
            UIFactory.SetLayoutElement(_cancel.Component.gameObject, minHeight: 32, flexibleWidth: 9999);
            _cancel.OnClick += () => SetActive(false);
        }
        private InputFieldRef CreateInput(string id, string title, string placeholder, out GameObject row)
        {
            row = UIFactory.CreateHorizontalGroup(ContentRoot, id + "Row", false, false, true, true, 8);
            UIFactory.SetLayoutElement(row, minHeight: 34, flexibleHeight: 0);
            var label = UIFactory.CreateLabel(row, id + "Label", title, TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(label.gameObject, minWidth: 80, flexibleWidth: 0);
            var input = UIFactory.CreateInputField(row, id + "Input", placeholder);
            UIFactory.SetLayoutElement(input.GameObject, minHeight: 28, flexibleWidth: 9999);
            return input;
        }
        private ButtonRef CreatePasswordAction(string id, string title, RoomPasswordChange change)
        {
            var button = UIFactory.CreateButton(_passwordActions, id, title);
            UIFactory.SetLayoutElement(button.Component.gameObject, minHeight: 28, flexibleWidth: 9999);
            button.OnClick += () => SetPasswordChange(change);
            return button;
        }
        public void ShowCreate()
        {
            if (!CanOpen) return;
            _operation = RoomOperation.Create; _roomId = 0;
            _heading.text = "创建房间 · 创建成功后自动进入";
            _name.Text = ""; _capacity.Text = "0";
            Open();
        }
        public void ShowEdit()
        {
            var room = _client.CurrentRoom;
            if (!CanOpen || room == null || room.IsLobby || room.OwnerPlayerId != _client.LocalPlayer.Id) return;
            _operation = RoomOperation.Update; _roomId = room.Id;
            _heading.text = "编辑房间 · " + room.Name + " · 当前 " + room.PlayerCount + " 人";
            _name.Text = room.Name; _capacity.Text = room.MaxPlayers.ToString();
            Open();
        }
        public void ShowJoin(RoomInfo room)
        {
            if (!CanOpen || room == null) return;
            _operation = RoomOperation.Join; _roomId = room.Id;
            _heading.text = "加入房间 · " + room.Name;
            Open();
        }
        private bool CanOpen => _settings.Enabled && _client.IsConnected && _client.CurrentRoom != null && !_client.IsRoomOperationPending;
        private void Open()
        {
            _submitted = false; _password.Text = ""; _error.text = "";
            bool join = _operation == RoomOperation.Join;
            _nameRow.SetActive(!join); _capacityRow.SetActive(!join);
            _passwordActions.SetActive(_operation == RoomOperation.Update);
            SetPasswordChange(RoomPasswordChange.Keep);
            _submit.ButtonText.text = join ? "加入" : _operation == RoomOperation.Create ? "创建并加入" : "保存";
            SetActive(true);
            RefreshControls();
            var input = join ? _password : _name;
            input.Component.Select(); input.Component.ActivateInputField();
        }
        private void SetPasswordChange(RoomPasswordChange change)
        {
            _passwordChange = change;
            _password.Text = "";
            _keepPassword.SetTabActive(change == RoomPasswordChange.Keep);
            _setPassword.SetTabActive(change == RoomPasswordChange.Set);
            _removePassword.SetTabActive(change == RoomPasswordChange.Remove);
            _passwordRow.SetActive(_operation != RoomOperation.Update || change == RoomPasswordChange.Set);
            var placeholder = _password.Component.placeholder as Text;
            if (placeholder != null) placeholder.text = _operation == RoomOperation.Create ? "留空则不设密码"
                : _operation == RoomOperation.Join ? "输入房间密码" : "输入新密码";
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
            if (Input.GetKeyDown(KeyCode.Escape)) { SetActive(false); return; }
            if (Input.GetKeyDown(KeyCode.Tab) && !_submitted)
            {
                var inputs = new List<InputFieldRef>();
                foreach (var input in new[] { _name, _capacity, _password })
                    if (input.GameObject.activeInHierarchy) inputs.Add(input);
                int index = inputs.FindIndex(input => input.Component.isFocused);
                int direction = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? -1 : 1;
                var next = inputs[(index + direction + inputs.Count) % inputs.Count];
                next.Component.Select(); next.Component.ActivateInputField();
            }
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) Submit();
        }
        private void Submit()
        {
            if (_submitted || !_settings.Enabled || !_client.IsConnected || _client.IsRoomOperationPending) return;
            int maxPlayers = 0;
            if (_operation != RoomOperation.Join)
            {
                if (string.IsNullOrWhiteSpace(_name.Text)) { _error.text = "房间名不能为空。"; return; }
                if (!int.TryParse(_capacity.Text, out maxPlayers) || maxPlayers < 0)
                { _error.text = "人数上限必须是非负整数，0 表示不限。"; return; }
            }
            if (_operation == RoomOperation.Update && _passwordChange == RoomPasswordChange.Set && string.IsNullOrEmpty(_password.Text))
            { _error.text = "请输入新密码，或选择移除密码。"; return; }
            _submitted = true; _error.text = "正在等待服务器确认…";
            RefreshControls();
            if (_operation == RoomOperation.Create) _client.CreateRoom(_name.Text, _password.Text, maxPlayers);
            else if (_operation == RoomOperation.Join) _client.JoinRoom(_roomId, _password.Text);
            else _client.UpdateRoom(_roomId, _name.Text, maxPlayers, _passwordChange, _password.Text);
        }
        private void RefreshControls()
        {
            bool editable = !_submitted;
            _submit.Component.interactable = editable;
            _cancel.ButtonText.text = editable ? "取消" : "关闭";
            _name.Component.interactable = editable; _capacity.Component.interactable = editable; _password.Component.interactable = editable;
            _keepPassword.Component.interactable = editable; _setPassword.Component.interactable = editable; _removePassword.Component.interactable = editable;
        }
        private void OnCompleted(RoomOperationResult result)
        {
            if (!Enabled || !_submitted || result.Operation != _operation) return;
            if (result.Error == RoomError.Busy && _client.IsRoomOperationPending) return;
            _submitted = false;
            if (result.IsSuccess) SetActive(false);
            else { _error.text = RoomUiText.Error(result.Error); RefreshControls(); }
        }
        private void OnCurrentRoomChanged()
        {
            if (!Enabled) return;
            var room = _client.CurrentRoom;
            if (room == null || (_operation == RoomOperation.Update && (room.Id != _roomId || room.OwnerPlayerId != _client.LocalPlayer.Id)))
                SetActive(false);
            else if (_operation == RoomOperation.Update)
                _heading.text = "编辑房间 · " + room.Name + " · 当前 " + room.PlayerCount + " 人";
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
