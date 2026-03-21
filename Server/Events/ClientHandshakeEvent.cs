
namespace GOILauncher.Multiplayer.Server.Events
{
    public class ClientHandshakeEvent
    {
        public string PlayerName { get; set; }
        public ClientHandshakeEvent(string playerName)
        {
            PlayerName = playerName;
        }
    }
}
