namespace GOILauncher.Multiplayer.Unity
{
    internal sealed class AlwaysEnabledMultiplayerState : IMultiplayerState
    {
        public bool Enabled
        {
            get { return true; }
        }
    }
}
