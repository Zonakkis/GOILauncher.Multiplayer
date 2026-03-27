
using GOILauncher.Multiplayer.Core.Data.Models;

namespace GOILauncher.Multiplayer.Server.Events
{
    public class ClientHandshakeEvent
    {
        public string PlayerName { get; set; }
        public Platform Platform { get; set; }
        public ClientHandshakeEvent(string playerName, Platform platform)
        {
            PlayerName = playerName;
            Platform = platform;
        }
    }
}
