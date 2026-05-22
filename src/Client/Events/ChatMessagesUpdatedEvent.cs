using System.Collections.Generic;
using GOILauncher.Multiplayer.Client.Models;

namespace GOILauncher.Multiplayer.Client.Events
{
    public class ChatMessagesUpdatedEvent
    {
        public List<Message> Messages { get; }
        public Message Message { get; }

        public ChatMessagesUpdatedEvent(List<Message> messages, Message message)
        {
            Messages = messages;
            Message = message;
        }
    }
}
