using System.Collections.ObjectModel;
using GOILauncher.Multiplayer.Client.Models;

namespace GOILauncher.Multiplayer.Client.Services
{
    public interface IChatService
    {
        ReadOnlyCollection<Message> Messages { get; }
        void SendMessage(MessageType type, string message);
    }
}
