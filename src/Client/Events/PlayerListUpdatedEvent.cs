using System.Collections.Generic;
using GOILauncher.Multiplayer.Core.Data.Models;

namespace GOILauncher.Multiplayer.Client.Events
{
    public class PlayerListUpdatedEvent
    {   
        public IList<PlayerInfo> Players { get; }

        public PlayerListUpdatedEvent(IList<PlayerInfo> players)
        {
            Players = players;
        }
    }
}
