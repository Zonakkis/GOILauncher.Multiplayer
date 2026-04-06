using System.Collections.Generic;
using GOILauncher.Multiplayer.Client.Models;

namespace GOILauncher.Multiplayer.Client.Services
{
    public interface IChatService
    {
        List<Message> ChatMessages { get; }
        void AddMessage(Message message);
        void SendSystemMessage(string content);
        void SendChatMessage(string content);
    }
}