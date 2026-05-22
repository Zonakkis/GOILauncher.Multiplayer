using System.Collections.Generic;
using GOILauncher.Multiplayer.Client.Models;

namespace GOILauncher.Multiplayer.Client
{
    public interface IUnityClient
    {
        bool IsConnected { get; }
        List<Message> ChatMessages { get; }
        void Connect(string host, int port, string playerName);
        void Disconnect();
        void SendMessage(MessageType type, string message);
    }
}
