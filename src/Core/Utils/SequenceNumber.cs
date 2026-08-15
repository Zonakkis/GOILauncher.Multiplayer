namespace GOILauncher.Multiplayer.Core.Utils
{
    public static class SequenceNumber
    {
        private const uint HalfRange = 0x80000000u;

        /// <summary>
        /// Compares uint sequence numbers while allowing the counter to wrap around.
        /// The exact half-range distance is treated as ambiguous and rejected.
        /// </summary>
        public static bool IsNewer(uint candidate, uint current)
        {
            var distance = unchecked(candidate - current);
            return distance != 0 && distance < HalfRange;
        }
    }
}
