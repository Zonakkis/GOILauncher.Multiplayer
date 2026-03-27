namespace GOILauncher.Multiplayer.Core.Event
{
    public class ClientDisconnectedEvent
    {
        public int ClientId { get; }
        public string Reason { get; }

        public ClientDisconnectedEvent(int clientId, string reason)
        {
            ClientId = clientId;
            Reason = reason;
        }
    }
}
