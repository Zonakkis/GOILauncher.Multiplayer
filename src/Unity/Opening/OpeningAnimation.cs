using System.Reflection;
using UnityEngine;

namespace GOILauncher.Multiplayer.Unity.Opening
{
    /// <summary>
    /// Visual-only adapter for a remote PoseControl. Never calls PlayOpeningAnimation:
    /// that method pauses PlayerControl input and, in Modpack, reads the observer's settings.
    /// </summary>
    internal sealed class OpeningAnimation : IOpeningAnimation
    {
        internal const string LayerName = "Animation";
        private const string WakeUpName = "WakeUp";
        private const string RattleName = "Rattle";

        // The same private flag exists in all three shipped PoseControl versions.
        private static readonly FieldInfo AnimatingField = typeof(PoseControl)
            .GetField("animating", BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly PoseControl _pose;

        internal OpeningAnimation(PoseControl pose)
        {
            _pose = pose;
        }

        internal static bool TryGetWakeUpState(Animator animator, int layer, out OpeningAnimationSample state)
        {
            // A trigger is not a state name. Identify the motion, then use the actual state's
            // fullPathHash for seeking. Inspect the destination first during a transition.
            var inTransition = animator.IsInTransition(layer);
            var current = Sample(animator.GetCurrentAnimatorStateInfo(layer),
                animator.GetCurrentAnimatorClipInfo(layer));
            var next = inTransition ? Sample(animator.GetNextAnimatorStateInfo(layer),
                animator.GetNextAnimatorClipInfo(layer)) : default(OpeningAnimationSample);
            return OpeningAnimationSample.TrySelect(inTransition, current, next, out state);
        }

        private static OpeningAnimationSample Sample(AnimatorStateInfo state, AnimatorClipInfo[] clips)
        {
            return new OpeningAnimationSample
            {
                IsWakeUp = HasWakeUpClip(clips), StateHash = state.fullPathHash,
                Length = state.length, NormalizedTime = state.normalizedTime
            };
        }

        private static bool HasWakeUpClip(AnimatorClipInfo[] clips)
        {
            foreach (var info in clips)
                if (info.clip != null && info.clip.name == WakeUpName)
                    return true;
            return false;
        }

        public void Prepare()
        {
            if (_pose == null) return;
            PrepareAnimator(_pose.anim);
            PrepareAnimator(_pose.potAnim);
        }

        private static void PrepareAnimator(Animator animator)
        {
            if (animator == null) return;
            // Run BEFORE the pooled object is activated: Android's PoseControl.Start advances
            // the Animator too, before the deferred seek has a chance to suppress events.
            animator.fireEvents = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        }

        public bool Apply(float remainingSeconds)
        {
            if (_pose == null)
                return false;

            Prepare();
            if (remainingSeconds <= 0f || float.IsNaN(remainingSeconds) || float.IsInfinity(remainingSeconds))
                return Reset();

            var animator = _pose.anim;
            if (animator == null || AnimatingField == null)
            {
                Reset();
                return false;
            }
            var layer = animator.GetLayerIndex(LayerName);
            if (layer < 0)
            {
                Reset();
                return false;
            }

            OpeningAnimationSample state;
            if (!TryGetWakeUpState(animator, layer, out state))
            {
                Begin(layer);
                animator.ResetTrigger(WakeUpName);
                animator.SetTrigger(WakeUpName);
                animator.Update(0f);
                if (!TryGetWakeUpState(animator, layer, out state))
                {
                    Reset();
                    return false;
                }
            }

            var progress = OpeningAnimationTiming.GetNormalizedTime(state.Length, remainingSeconds);
            Begin(layer);
            animator.Play(state.StateHash, layer, progress);
            animator.ResetTrigger(WakeUpName);
            animator.Update(0f);

            // Evaluation above samples the clip's own blendAmt/handBlend curves. Do not replace
            // those curves with linear weights derived from remaining seconds.
            if (progress >= 1f || _pose.blendAmt <= 0f)
                Complete(layer);
            else
                SetLayerWeights(layer, _pose.blendAmt);

            return ApplyPot(state.Length, remainingSeconds, progress >= 1f);
        }

        private bool Reset()
        {
            // Completion never depends on triggering/discovering WakeUp successfully. The pot
            // must be reset even when the body Animator/layer/state is unavailable.
            var animator = _pose.anim;
            var bodyReset = false;
            if (animator != null)
            {
                var layer = animator.GetLayerIndex(LayerName);
                animator.ResetTrigger(WakeUpName);
                if (layer >= 0)
                {
                    OpeningAnimationSample state;
                    if (TryGetWakeUpState(animator, layer, out state))
                        animator.Play(state.StateHash, layer, 1f);
                    Complete(layer);
                    animator.Update(0f);
                    Complete(layer);
                    bodyReset = true;
                }
            }
            _pose.blendAmt = 0f;
            _pose.handBlend = 1f;
            if (AnimatingField != null) AnimatingField.SetValue(_pose, false);
            var potReset = ApplyPot(0f, 0f, true);
            return bodyReset && potReset;
        }

        private void Begin(int layer)
        {
            AnimatingField.SetValue(_pose, true);
            _pose.blendAmt = 1f;
            _pose.handBlend = 0f;
            SetLayerWeights(layer, 1f);
        }

        private void Complete(int layer)
        {
            if (AnimatingField != null) AnimatingField.SetValue(_pose, false);
            _pose.blendAmt = 0f;
            _pose.handBlend = 1f;
            SetLayerWeights(layer, 0f);
        }

        private void SetLayerWeights(int openingLayer, float weight)
        {
            for (var i = 0; i < _pose.anim.layerCount; i++)
                _pose.anim.SetLayerWeight(i, i == openingLayer ? weight : 1f - weight);
        }

        private bool ApplyPot(float bodyLength, float remainingSeconds, bool completed)
        {
            var animator = _pose.potAnim;
            if (animator == null)
                return false;

            animator.fireEvents = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.Play(RattleName, -1, 0f);
            animator.Update(0f);
            for (var layer = 0; layer < animator.layerCount; layer++)
            {
                var state = animator.GetCurrentAnimatorStateInfo(layer);
                if (!state.IsName(RattleName))
                    continue;

                // potAnim has no extra per-frame PoseControl.Update call. Map the common
                // elapsed game time through its own duration/rate, not the body's 2x rate.
                var elapsed = OpeningAnimationTiming.GetElapsedSeconds(bodyLength, remainingSeconds);
                var progress = completed ? 1f : OpeningAnimationTiming.GetSecondaryNormalizedTime(
                    state.length, animator.speed * state.speed * state.speedMultiplier, elapsed);
                animator.Play(state.fullPathHash, layer, progress);
                animator.Update(0f);
                return true;
            }

            return false;
        }
    }
}
