using System;
using GOILauncher.Multiplayer.Shared.Game;

namespace GOILauncher.Multiplayer.Client.Events
{
    public class PlayerLeftEventArgs : EventArgs
    {
        public ClientPlayer Player { get; set; }
    }
}
