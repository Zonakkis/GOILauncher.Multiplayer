using System;

namespace GOILauncher.Multiplayer.Client.Events
{
    public class PlayerEnteredEventArgs : EventArgs
    {
        public ClientPlayer Player { get; set; }
    }
}
