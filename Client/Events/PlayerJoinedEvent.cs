using GOILauncher.Multiplayer.Core.Data.Models;

namespace GOILauncher.Multiplayer.Client.Events
{
    public class PlayerJoinedEvent
    {
        public int PlayerId { get; }
        public string PlayerName { get; }
        public Platform Platform { get; }
        public PlayerJoinedEvent(int playerId, string playerName, Platform platform)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            Platform = platform;
        }
    }
}
