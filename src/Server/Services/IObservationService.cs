namespace GOILauncher.Multiplayer.Server.Services
{
    /// <summary>
    /// Read-only diagnostics view for a server host console. Purely observational: it exposes
    /// what is happening and whether the loop is healthy, and offers no way to act on players.
    /// </summary>
    public interface IObservationService
    {
        /// <summary>
        /// Called once per iteration of the host's Poll loop. Advances liveness counters and,
        /// on a throttle, refreshes the display caches. Must run on the same thread as Poll.
        /// </summary>
        void MarkPoll();

        /// <summary>Immutable view safe to read from any thread.</summary>
        ServerObservationSnapshot Snapshot { get; }
    }
}
