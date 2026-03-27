using GOILauncher.Multiplayer.Core.Data.Models;

namespace GOILauncher.Multiplayer.Client.Events
{
    public class PlayerLeftEvent
    {
        public int PlayerId { get; }
        public string PlayerName { get; }
        public Platform Platform { get; }
        public PlayerLeftEvent(int playerId, string playerName, Platform platform)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            Platform = platform;
        }
    }
}
