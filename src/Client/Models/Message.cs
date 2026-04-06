using System;

namespace GOILauncher.Multiplayer.Client.Models
{
    public class Message
    {
        public MessageType Type { get; }
        public string Sender { get; }
        public string Content { get; }
        public DateTime DateTime { get; }
        public Message(MessageType type, string sender, string content) : this(type, sender, content, DateTime.Now)
        {
            
        }

        public Message(MessageType type, string sender, string content, DateTime timestamp)
        {
            Type = type;
            Sender = sender;
            Content = content;
            DateTime = timestamp;
        }

    }
}
