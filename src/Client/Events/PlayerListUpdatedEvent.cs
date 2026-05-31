using System.Collections.Generic;
using GOILauncher.Multiplayer.Client.Models;

namespace GOILauncher.Multiplayer.Client.Events
{
    public class PlayerListUpdatedEvent
    {
        public Dictionary<int, ClientPlayer> Players { get; }

        public PlayerListUpdatedEvent(Dictionary<int, ClientPlayer> players)
        {
            Players = players;
        }
    }
}
