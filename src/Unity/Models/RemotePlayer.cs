using GOILauncher.Multiplayer.Core.Data.Constants;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Shared.Extensions;
using GOILauncher.Multiplayer.Unity.Extensions;
using UnityEngine;

namespace GOILauncher.Multiplayer.Unity.Models
{
    public class RemotePlayer : PlayerBase
    {
        private const float StateInterval = ModConstants.SyncFrameTime;

        public GameObject LocalPlayer { get; set; }
        private Renderer[] _renderers;
        private Material _potMaterial;

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
        private bool _isRenderersEnabled;
        private GUIStyle _labelStyle;

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
            if (!_hasState || _interpolationElapsed >= StateInterval)
            {
                return;
            }

            _interpolationElapsed = Mathf.Min(
                StateInterval,
                _interpolationElapsed + Time.deltaTime);
            ApplyInterpolated(_fromState, _targetState,
                _interpolationElapsed / StateInterval);

            var cameraPosition = _camera.transform.position;
            var halfHeight = _camera.orthographicSize;
            var halfWidth = _camera.aspect * halfHeight;
            var _topLeft = new Vector2(cameraPosition.x - halfWidth, cameraPosition.y + halfHeight);
            var _bottomRight = new Vector2(cameraPosition.x + halfWidth, cameraPosition.y - halfHeight);
            _isInCamera = transform.position.IsInRectangle(
                _topLeft.x - GameConstants.PlayerWidth, _topLeft.y + GameConstants.PlayerHeight,
                _bottomRight.x + GameConstants.PlayerWidth, _bottomRight.y - GameConstants.PlayerWidth);
            SetRenderersEnabled(_isInCamera);
        }

        public override void Reset()
        {
            base.Reset();
            _fromState = default(PlayerState);
            _targetState = default(PlayerState);
            _interpolationElapsed = 0f;
            _hasState = false;
        }

         /// <summary>
        /// 换掉罐子的贴图和金度。<paramref name="texture"/> 为 null 时只改金度。
        /// 返回 false 表示这个实例上找不到罐子的 MeshRenderer，外观没动过。
        /// </summary>
        /// <remarks>
        /// 只写 <c>material</c>（每个 Renderer 自己那份），绝不写 <c>sharedMaterial</c>：
        /// 远端实例是从本地 Player 克隆来的，两边的 Renderer 指着同一个材质，写共享那份
        /// 等于把本地玩家也换掉。读 <c>material</c> 这一下就已经让 Unity 拷出独立副本了，
        /// 副本归本实例所有，销毁时要自己收（见 <see cref="OnDestroy"/>）。
        /// </remarks>
        public bool ApplySkin(Texture2D texture, float goldness)
        {
            var material = GetPotMaterial();
            if (material == null)
            {
                return false;
            }

            if (texture != null)
            {
                material.mainTexture = texture;
            }
            if (material.HasProperty(GameConstants.GoldnessProperty))
            {
                material.SetFloat(GameConstants.GoldnessProperty, goldness);
            }
            return true;
        }

        public void OnDestroy()
        {
            // 这份材质是读 material 时 Unity 为本实例拷出来的，没人替我们回收。
            if (_potMaterial != null)
            {
                Destroy(_potMaterial);
                _potMaterial = null;
            }
        }

        private Material GetPotMaterial()
        {
            // 实例本身在池里反复借还，材质副本跟着实例活着，所以只拷一次。
            if (_potMaterial != null)
            {
                return _potMaterial;
            }

            var mesh = transform.Find(GameConstants.PotMeshPath);
            if (mesh == null)
            {
                return null;
            }

            var renderer = mesh.GetComponent<Renderer>();
            if (renderer == null)
            {
                return null;
            }

            _potMaterial = renderer.material;
            return _potMaterial;
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
