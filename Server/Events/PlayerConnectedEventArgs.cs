using System;

namespace GOILauncher.Multiplayer.Server.Events
{
    public class PlayerConnectedEventArgs : EventArgs
    {
        public ServerPlayer Player { get; set; }
    }
}
