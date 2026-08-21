using System.Collections.ObjectModel;
using GOILauncher.Multiplayer.Client.Models;

namespace GOILauncher.Multiplayer.Unity
{
    public interface IUnityClient
    {
        bool IsConnected { get; }
        ReadOnlyCollection<Message> ChatMessages { get; }

        void Connect(string host, int port, string playerName);
        void Disconnect();
        void SendMessage(MessageType type, string message);
    }
}
