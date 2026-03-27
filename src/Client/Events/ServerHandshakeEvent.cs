
namespace GOILauncher.Multiplayer.Client.Events
{
    public class ServerHandshakeEvent
    {
        public int PlayerId { get; set; }
        public ServerHandshakeEvent(int playerId)
        {
            PlayerId = playerId;
        }
    }
}
