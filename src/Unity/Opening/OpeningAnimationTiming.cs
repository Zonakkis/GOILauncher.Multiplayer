using System;

namespace GOILauncher.Multiplayer.Unity.Opening
{
    /// <summary>
    /// Converts body clip time to game seconds. PoseControl manually updates its enabled
    /// Normal-mode body Animator in LateUpdate in addition to Unity's automatic update.
    /// The resulting 2x rate was confirmed in game; this is NOT Animator.speed.
    /// </summary>
    public static class OpeningAnimationTiming
    {
        public const float BodyPlaybackRate = 2f;

        public static float GetRemainingSeconds(float length, float normalizedTime)
        {
            if (!IsPositiveFinite(length) || !IsFinite(normalizedTime))
                return 0f;

            return length * (1f - Clamp01(normalizedTime)) / BodyPlaybackRate;
        }

        public static float GetNormalizedTime(float length, float remainingSeconds)
        {
            if (!IsPositiveFinite(length) || !IsFinite(remainingSeconds) || remainingSeconds < 0f)
                return 1f;

            return Clamp01(1f - remainingSeconds / length * BodyPlaybackRate);
        }

        public static float GetElapsedSeconds(float bodyLength, float remainingSeconds)
        {
            if (!IsPositiveFinite(bodyLength) || !IsFinite(remainingSeconds))
                return 0f;

            return Math.Max(0f, bodyLength / BodyPlaybackRate - Math.Max(0f, remainingSeconds));
        }

        public static float GetSecondaryNormalizedTime(float length, float playbackRate, float elapsedSeconds)
        {
            if (!IsPositiveFinite(length) || !IsFinite(playbackRate) || !IsFinite(elapsedSeconds))
                return 1f;

            return Clamp01(Math.Max(0f, elapsedSeconds) / length * Math.Max(0f, playbackRate));
        }

        private static bool IsPositiveFinite(float value) => value > 0f && IsFinite(value);
        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private static float Clamp01(float value) => Math.Max(0f, Math.Min(1f, value));
    }
}
