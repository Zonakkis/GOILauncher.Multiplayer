namespace GOILauncher.Multiplayer.Core.Event
{
    public class ClientConnectedEvent
    {
        public int ClientId { get; }

        public ClientConnectedEvent(int clientId)
        {
            ClientId = clientId;
        }
    }
}
