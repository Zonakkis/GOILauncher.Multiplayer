namespace GOILauncher.Multiplayer.Core.Event
{
    public class ServerDisconnectedEvent
    {
        public string Reason { get; }

        public ServerDisconnectedEvent(string reason)
        {
            Reason = reason;
        }
    }
}
