using System;

namespace GOILauncher.Multiplayer.Shared.Events
{
    public class ServerDisconnectedEventArgs : EventArgs
    {
        public string Reason { get; set; }
    }
}