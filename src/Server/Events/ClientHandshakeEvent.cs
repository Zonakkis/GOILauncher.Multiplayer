
using GOILauncher.Multiplayer.Core.Data.Models;

namespace GOILauncher.Multiplayer.Server.Events
{
    public class ClientHandshakeEvent
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public Platform Platform { get; set; }
        public ClientHandshakeEvent(int playerId, string playerName, Platform platform)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            Platform = platform;
        }
    }
}
