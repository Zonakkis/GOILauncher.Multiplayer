using System;

namespace GOILauncher.Multiplayer.Shared.Events
{
    public class ClientDisconnectedEventArgs : EventArgs
    {
        public int ClientId { get; set; }
        public string Reason { get; set; }
    }
}