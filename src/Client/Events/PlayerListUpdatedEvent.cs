using System.Collections.Generic;
using GOILauncher.Multiplayer.Client.Models;

namespace GOILauncher.Multiplayer.Client.Events
{
    public class PlayerListUpdatedEvent
    {   
        public IList<IClientPlayer> Players { get; }

        public PlayerListUpdatedEvent(IList<IClientPlayer> players)
        {
            Players = players;
        }
    }
}
