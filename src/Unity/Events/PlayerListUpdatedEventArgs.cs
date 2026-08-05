using System;
using System.Collections.Generic;
using GOILauncher.Multiplayer.Core.Data.Models;

namespace GOILauncher.Multiplayer.Unity.Events
{
    public class PlayerListUpdatedEventArgs : EventArgs
    {
        public IList<PlayerInfo> Players { get; }

        public PlayerListUpdatedEventArgs(IList<PlayerInfo> players)
        {
            Players = players;
        }
    }
}
