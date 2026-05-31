using System.Collections.Generic;
using GOILauncher.Multiplayer.Client.Models;

namespace GOILauncher.Multiplayer.Client.Events
{
    public class PlayerListUpdatedEvent
    {   
        public IList<ClientPlayer> Players { get; }

        public PlayerListUpdatedEvent(IList<ClientPlayer> players)
        {
            Players = players;
        }
    }
}
