using System;
using GOILauncher.Multiplayer.Client.Models;

namespace GOILauncher.Multiplayer.Client.Events
{
    public class ChatMessageEvent
    {
        public Message Message { get; }
        public ChatMessageEvent(MessageType type, string sender, string content, DateTime dateTime)
        {
            Message = new Message(type, sender, content, dateTime);
        }
        public ChatMessageEvent(Message message)
        {
            Message = message;
        }
    }
}
