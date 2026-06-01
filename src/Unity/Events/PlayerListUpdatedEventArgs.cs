using System;
using System.Collections.Generic;
using GOILauncher.Multiplayer.Client.Models;

namespace GOILauncher.Multiplayer.Client.Events
{
    public class PlayerListUpdatedEventArgs : EventArgs
    {
        public IList<IClientPlayer> Players { get; }

        public PlayerListUpdatedEventArgs(IList<IClientPlayer> players)
        {
            Players = players;
        }
    }
}
