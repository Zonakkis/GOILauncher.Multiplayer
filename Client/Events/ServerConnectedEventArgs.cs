using System;

namespace GOILauncher.Multiplayer.Client.Events
{
    public class ServerConnectedEventArgs : EventArgs
    {
        public int PlayerId { get; set; }
    }
}
