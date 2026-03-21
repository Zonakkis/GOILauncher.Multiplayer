namespace GOILauncher.Multiplayer.Client.Events
{
    public class ChatMessageEvent
    {
        public int PlayerId { get; }
        public string Message { get; }
        public ChatMessageEvent(int playerId, string message)
        {
            PlayerId = playerId;
            Message = message;
        }
    }
}
