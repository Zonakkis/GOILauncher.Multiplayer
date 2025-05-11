using System;

namespace GOILauncher.Multiplayer.Client.Events
{
    public class ServerDisconnectedEventArgs : EventArgs
    {
        public string Reason { get; set; }
    }
}
