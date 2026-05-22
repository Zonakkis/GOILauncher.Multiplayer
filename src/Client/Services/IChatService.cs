using System.Collections.Generic;
using GOILauncher.Multiplayer.Client.Models;

namespace GOILauncher.Multiplayer.Client.Services
{
    public interface IChatService
    {
        List<Message> Messages { get; }
        void SendMessage(MessageType type, string message);
    }
}
