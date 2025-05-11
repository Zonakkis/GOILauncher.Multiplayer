using System;

namespace GOILauncher.Multiplayer.Server.Events
{
    public class PlayerDisconnectedEventArgs : EventArgs
    {
        public ServerPlayer Player { get; set; }

        public string Reason { get; set; }
    }
}
