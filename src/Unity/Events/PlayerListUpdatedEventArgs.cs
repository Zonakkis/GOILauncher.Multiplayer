using System;
using System.Collections.Generic;
using GOILauncher.Multiplayer.Client.Models;

namespace GOILauncher.Multiplayer.Client.Events
{
    public class PlayerListUpdatedEventArgs : EventArgs
    {
        public IList<ClientPlayer> Players { get; }

        public PlayerListUpdatedEventArgs(IList<ClientPlayer> players)
        {
            Players = players;
        }
    }
}
