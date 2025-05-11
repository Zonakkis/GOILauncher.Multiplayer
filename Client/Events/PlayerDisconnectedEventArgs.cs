using System;

namespace GOILauncher.Multiplayer.Client.Events
{
    public class PlayerDisconnectedEventArgs : EventArgs
    {
        public ClientPlayer Player { get; set; }
    }
}
