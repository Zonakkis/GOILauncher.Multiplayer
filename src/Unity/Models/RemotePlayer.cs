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
                PlayerRotation = FromQuaternion(transform.rotation),
                SliderPosition = FromVector3(slider.position),
                SliderRotation = FromQuaternion(slider.rotation),
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
                $"（{Vector3.Distance(transform.position, LocalPlayer.transform.position):0.0}m）");
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
