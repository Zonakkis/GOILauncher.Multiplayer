using System;

namespace GOILauncher.Multiplayer.Client.Events
{
    public class PlayerConnectedEventArgs : EventArgs
    {
        public ClientPlayer Player { get; set; }
    }
}
