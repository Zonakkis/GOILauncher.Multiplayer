namespace GOILauncher.Multiplayer.Unity
{
    /// <summary>
    /// Provides the current availability of the multiplayer module to Unity runtime services.
    /// </summary>
    public interface IMultiplayerState
    {
        bool Enabled { get; }
    }
}
