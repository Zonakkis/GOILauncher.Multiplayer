using System;
using GOILauncher.Multiplayer.Shared.Game;

namespace GOILauncher.Multiplayer.Client.Events
{
    public class PlayerMovedEventArgs : EventArgs
    {
        public ClientPlayer Player { get; set; }
        public Move Move { get; set; }
    }
}
