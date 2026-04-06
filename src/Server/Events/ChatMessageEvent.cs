using System;
using GOILauncher.Multiplayer.Core.Utils;

namespace GOILauncher.Multiplayer.Server.Events
{
    public class ChatMessageEvent
    {
        public int PlayerId { get; }
        public string Message { get; }
        public DateTime Timestamp { get; }
        public ChatMessageEvent(int playerId, string message, long timestamp)
        {
             Timestamp = DateTimeUtils.FromUnixTimeSeconds(timestamp);
             PlayerId = playerId;
             Message = message;
        }
    }
}
