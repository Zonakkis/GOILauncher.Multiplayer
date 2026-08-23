namespace GOILauncher.Multiplayer.Unity.Config
{
    /// <summary>
    /// Platform port for persisted settings. Values are passed as strings so that each platform
    /// implementation stays trivial; naming, typing, defaults and change notification belong to
    /// <see cref="MultiplayerSettings"/>.
    /// </summary>
    public interface ISettingsStore
    {
        bool TryRead(string key, out string raw);

        /// <summary>
        /// Persists a single value. Implementations write through, so a crash cannot lose a setting
        /// the player already changed.
        /// </summary>
        void Write(string key, string raw);

        /// <summary>
        /// Flushes buffered writes. A no-op for write-through stores, but required by stores that
        /// batch (a <c>PlayerPrefs</c>-backed one would need its <c>Save</c> here).
        /// </summary>
        void Flush();
    }
}
