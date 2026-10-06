using System.Collections.Generic;
using GOILauncher.Multiplayer.Core.Data.Constants;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Unity.Extensions;
using GOILauncher.Multiplayer.Unity.Opening;
using GOILauncher.Multiplayer.Unity.Skin;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace GOILauncher.Multiplayer.Unity.Models
{
    public class RemotePlayer : PlayerBase
    {
        private const float StateInterval = ModConstants.SyncFrameTime;

        public GameObject LocalPlayer { get; set; }
        private Renderer[] _renderers;

        // 按槽位缓存 material 副本：罐子和身体是两个 Renderer，各自那份独立材质跟着本实例活着。
        private readonly Dictionary<byte, Material> _materials = new Dictionary<byte, Material>();

        /// <summary>
        /// 到本地玩家的直线距离（米）。头顶标签和玩家列表共用这一处定义，
        /// 换度量方式（比如改成高度差）时只改这里。
        /// </summary>
        public float DistanceToLocalPlayer
        {
            get
            {
                return LocalPlayer == null
                    ? 0f
                    : Vector3.Distance(transform.position, LocalPlayer.transform.position);
            }
        }

        private PlayerState _fromState;
        private PlayerState _targetState;
        private float _interpolationElapsed;
        private bool _hasState;

        private bool _isInCamera;
        private readonly Camera _camera = Camera.main;
        /// <summary>
        /// 刚 Instantiate 出来的实例，renderer 是跟着本地 Player 克隆来的，实际是启用状态。
        /// 所以初值不能是 false——那等于撒谎，第一次 SetRenderersEnabled(false) 会被当成
        /// "已经是这个状态" 而跳过，renderer 其实一直开着。用 null 表示 "还没应用过"，
        /// 保证第一次调用必定真正写一遍。
        /// </summary>
        private bool? _isRenderersEnabled;
        private GUIStyle _labelStyle;
        private OpeningPlayback _openingPlayback;
        private Coroutine _openingRoutine;

        internal void PrepareOpening()
        {
            if (_openingPlayback == null)
                _openingPlayback = new OpeningPlayback(
                    new OpeningAnimation(GetComponentInChildren<PoseControl>(true)),
                    () => (double)Stopwatch.GetTimestamp() / Stopwatch.Frequency);
            _openingPlayback.Prepare();
        }

        /// <summary>Zero explicitly restores the completed pose; positive values resume its tail.</summary>
        public void ApplyOpening(float remainingSeconds)
        {
            StopOpeningRoutine();
            PrepareOpening();
            _openingRoutine = StartCoroutine(
                _openingPlayback.ApplyAfterOneFrame(remainingSeconds, OnOpeningApplied));
        }

        private void OnOpeningApplied(bool succeeded)
        {
            _openingRoutine = null;
            if (!succeeded)
                Debug.LogWarning("[Multiplayer] Could not apply opening animation for player " + Id + ".");
        }

        private void StopOpeningRoutine()
        {
            if (_openingPlayback != null)
                _openingPlayback.Cancel();
            if (_openingRoutine == null)
                return;
            StopCoroutine(_openingRoutine);
            _openingRoutine = null;
        }

        private void OnDisable()
        {
            StopOpeningRoutine();
        }

        /// <summary>
        /// The first state snaps the pooled instance into place. Later states are
        /// interpolated over one 60 Hz interval so packet loss does not freeze it.
        /// </summary>
        public void ApplyState(PlayerState state)
        {
            if (!_hasState)
            {
                ApplyDirect(state);
                _fromState = state;
                _targetState = state;
                _interpolationElapsed = StateInterval;
                _hasState = true;
                return;
            }

            PlayerState currentState;
            _fromState = TryCaptureState(out currentState) ? currentState : _targetState;
            _targetState = state;
            _interpolationElapsed = 0f;
        }

        public void LateUpdate()
        {
            UpdateInterpolation();
            UpdateVisibility();
        }

        /// <summary>
        /// 由对方的发包节奏驱动：没有状态、或这一段已经插完，就没有东西要推进。
        /// </summary>
        private void UpdateInterpolation()
        {
            if (!_hasState || _interpolationElapsed >= StateInterval)
            {
                return;
            }

            _interpolationElapsed = Mathf.Min(
                StateInterval,
                _interpolationElapsed + Time.deltaTime);
            ApplyInterpolated(_fromState, _targetState,
                _interpolationElapsed / StateInterval);
        }

        /// <summary>
        /// 必须每帧都跑，不能并进 <see cref="UpdateInterpolation"/> 的条件里：可见性由
        /// 本地相机的位置决定，而相机会在对方一个包都不发的时候继续移动。对方打开暂停菜单时
        /// 原版会把 <c>Time.timeScale</c> 设为 0，发送端的节流器用的是缩放时间，于是上行彻底停住
        /// （见 docs/agent/state-synchronization.md）。裁剪一旦跟着发包节奏走，暂停的玩家就会
        /// 永远停在最后一次的显隐状态上——在屏幕外暂停的人，走到他面前也不会重新出现。
        /// </summary>
        private void UpdateVisibility()
        {
            _isInCamera = CameraVisibility.IsWithinView(
                _camera.transform.position,
                _camera.orthographicSize,
                _camera.aspect,
                transform.position);
            SetRenderersEnabled(_isInCamera);
        }

        public override void Reset()
        {
            StopOpeningRoutine();
            PrepareOpening();
            _openingPlayback.Reset();
            base.Reset();
            _fromState = default(PlayerState);
            _targetState = default(PlayerState);
            _interpolationElapsed = 0f;
            _hasState = false;
        }

         /// <summary>
        /// 换掉某个槽位（罐子 / 身体）的贴图和金度。<paramref name="texture"/> 为 null 时只改金度。
        /// <paramref name="hasCustomSkin"/> 表示这名玩家这个槽位有没有自定义皮肤：没有的话
        /// <paramref name="texture"/> 只是原版基线，安卓上会改由 substance 按金度现生成（见
        /// <see cref="SkinMaterial.ApplyTo"/>）。
        /// 贴图与金度怎么落到材质上（PC 的 shader 属性还是 Android 的 procedural input、二者能否叠加）
        /// 全部收在 <see cref="SkinMaterial.ApplyTo"/>，这里不认平台；身体没有金度那一步天然是空操作。
        /// 返回 false 表示这个实例上找不到该槽位的 Renderer，外观没动过。
        /// </summary>
        /// <remarks>
        /// 只写 <c>material</c>（每个 Renderer 自己那份），绝不写 <c>sharedMaterial</c>：
        /// 远端实例是从本地 Player 克隆来的，两边的 Renderer 指着同一个材质，写共享那份
        /// 等于把本地玩家也换掉。读 <c>material</c> 这一下就已经让 Unity 拷出独立副本了，
        /// 副本归本实例所有，销毁时要自己收（见 <see cref="OnDestroy"/>）。
        /// </remarks>
        public bool ApplySkin(byte slot, Texture2D texture, float goldness, bool hasCustomSkin)
        {
            var material = GetMaterial(slot);
            if (material == null)
            {
                return false;
            }

            SkinMaterial.ApplyTo(material, texture, goldness, hasCustomSkin);
            return true;
        }

        public void OnDestroy()
        {
            StopOpeningRoutine();
            // 这些材质是读 material 时 Unity 为本实例拷出来的，没人替我们回收。
            foreach (var material in _materials.Values)
            {
                if (material != null)
                {
                    Destroy(material);
                }
            }
            _materials.Clear();
        }

        private Material GetMaterial(byte slot)
        {
            // 实例本身在池里反复借还，材质副本跟着实例活着，所以每槽位只拷一次。
            Material material;
            if (_materials.TryGetValue(slot, out material) && material != null)
            {
                return material;
            }

            var path = GameConstants.SkinMeshPath(slot);
            if (path == null)
            {
                return null;
            }

            var mesh = transform.Find(path);
            if (mesh == null)
            {
                return null;
            }

            var renderer = mesh.GetComponent<Renderer>();
            if (renderer == null)
            {
                return null;
            }

            material = renderer.material;
            _materials[slot] = material;
            return material;
        }

        private bool TryCaptureState(out PlayerState state)
        {
            Transform slider;
            Transform nestedHandle;
            if (!TryGetStateTransforms(out slider, out nestedHandle))
            {
                state = default(PlayerState);
                return false;
            }

            state = new PlayerState
            {
                PlayerPosition = FromVector3(transform.position),
                PlayerRotation = transform.rotation.eulerAngles.z,
                SliderPosition = FromVector3(slider.position),
                SliderRotation = slider.rotation.eulerAngles.z,
                HandlePosition = FromVector3(nestedHandle.position),
                HandleRotation = FromQuaternion(nestedHandle.rotation)
            };
            return true;
        }

        private void ApplyDirect(PlayerState state)
        {
            Transform slider;
            Transform nestedHandle;
            if (!TryGetStateTransforms(out slider, out nestedHandle))
            {
                return;
            }

            transform.position = ToVector3(state.PlayerPosition);
            transform.rotation = ToQuaternion(state.PlayerRotation);
            slider.position = ToVector3(state.SliderPosition);
            slider.rotation = ToQuaternion(state.SliderRotation);
            nestedHandle.position = ToVector3(state.HandlePosition);
            nestedHandle.rotation = ToQuaternion(state.HandleRotation);
        }

        private void ApplyInterpolated(PlayerState from, PlayerState to, float t)
        {
            Transform slider;
            Transform nestedHandle;
            if (!TryGetStateTransforms(out slider, out nestedHandle))
            {
                return;
            }

            transform.position = Vector3.Lerp(
                ToVector3(from.PlayerPosition), ToVector3(to.PlayerPosition), t);
            transform.rotation = Quaternion.Slerp(
                ToQuaternion(from.PlayerRotation), ToQuaternion(to.PlayerRotation), t);
            slider.position = Vector3.Lerp(
                ToVector3(from.SliderPosition), ToVector3(to.SliderPosition), t);
            slider.rotation = Quaternion.Slerp(
                ToQuaternion(from.SliderRotation), ToQuaternion(to.SliderRotation), t);
            nestedHandle.position = Vector3.Lerp(
                ToVector3(from.HandlePosition), ToVector3(to.HandlePosition), t);
            nestedHandle.rotation = Quaternion.Slerp(
                ToQuaternion(from.HandleRotation), ToQuaternion(to.HandleRotation), t);
        }

        private static Vector3 ToVector3(UnityVector3 value)
        {
            return new Vector3(value.X, value.Y, value.Z);
        }

        private static Quaternion ToQuaternion(float value)
        {
            return Quaternion.Euler(0, 0 , value);
        }

        private static Quaternion ToQuaternion(UnityQuaternion value)
        {
            return new Quaternion(value.X, value.Y, value.Z, value.W);
        }

        public void OnGUI()
        {
            if (_labelStyle == null)
                _labelStyle = new GUIStyle(GUI.skin.label);
            _labelStyle.fontSize = (int)Mathf.Clamp(GameConstants.DefaultFontSize
                * (Screen.width / GameConstants.DefaultScreenWidth)
                * (Screen.dpi / GameConstants.DefaultScreenDpi)
                * (GameConstants.DefaultCameraOrthographicSize / _camera.orthographicSize), 10f, 25f);
            var label = new GUIContent(
                _isInCamera ? $"[{Id}][{Platform.ToReadableString()}]{Name}" :
                $"[{Id}][{Platform.ToReadableString()}]{Name}" +
                $"（{DistanceToLocalPlayer:0.0}m）");
            var labelSize = _labelStyle.CalcSize(label);
            Vector2 labelScreenPosition = _camera.WorldToScreenPoint(transform.position + transform.up * 1.5f);
            labelScreenPosition.y = Screen.height - labelScreenPosition.y;
            var halfWidth = labelSize.x / 2f;
            var halfHeight = labelSize.y / 2f;
            labelScreenPosition.x = Mathf.Clamp(
                labelScreenPosition.x, halfWidth, Screen.width - halfWidth);
            labelScreenPosition.y = Mathf.Clamp(
                labelScreenPosition.y, halfHeight, Screen.height - halfHeight);
            var labelRect = new Rect(
                labelScreenPosition.x - halfWidth, labelScreenPosition.y - halfHeight,
                labelSize.x, labelSize.y);
            var shadowRect = labelRect;
            shadowRect.position += Vector2.one;
            _labelStyle.normal.textColor = Color.black;
            GUI.Label(shadowRect, label, _labelStyle);
            _labelStyle.normal.textColor = Color.white;
            GUI.Label(labelRect, label, _labelStyle);
        }

        public void SetRenderersEnabled(bool enabled)
        {
            if (_isRenderersEnabled == enabled)
                return;
            if (_renderers == null)
                _renderers = transform.GetComponentsInChildren<Renderer>();
            foreach (var renderer in _renderers)
                renderer.enabled = enabled;
            _isRenderersEnabled = enabled;
        }
    }
}
