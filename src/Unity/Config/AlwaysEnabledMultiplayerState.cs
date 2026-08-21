namespace GOILauncher.Multiplayer.Unity.Config
{
    internal sealed class AlwaysEnabledMultiplayerState : IMultiplayerState
    {
        public bool Enabled
        {
            get { return true; }
        }
    }
}
