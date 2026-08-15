using GOILauncher.Multiplayer.Core.Data.Models;
using UnityEngine;

namespace GOILauncher.Multiplayer.Unity.Models
{
    public class RemotePlayer : PlayerBase
    {
        private const float StateInterval = 1f / 60f;

        private PlayerState _fromState;
        private PlayerState _targetState;
        private float _interpolationElapsed;
        private bool _hasState;

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
    }
}
