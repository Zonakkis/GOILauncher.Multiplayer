namespace GOILauncher.Multiplayer.Unity.Opening
{
    /// <summary>Local Animator metadata, never part of the network protocol.</summary>
    internal struct OpeningAnimationSample
    {
        public bool IsWakeUp;
        public int StateHash;
        public float Length;
        public float NormalizedTime;

        internal static bool TrySelect(bool inTransition, OpeningAnimationSample current,
            OpeningAnimationSample next, out OpeningAnimationSample result)
        {
            if (inTransition && next.IsWakeUp)
            {
                result = next;
                return true;
            }
            if (current.IsWakeUp)
            {
                result = current;
                return true;
            }
            result = default(OpeningAnimationSample);
            return false;
        }
    }
}
