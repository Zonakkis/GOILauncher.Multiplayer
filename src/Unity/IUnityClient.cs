using System;
using System.Collections.Generic;
using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Client.Models;

namespace GOILauncher.Multiplayer.Unity
{
    public interface IUnityClient
    {
        bool IsConnected { get; }
        IList<IClientPlayer> Players { get; }
        List<Message> ChatMessages { get; }

        event EventHandler<PlayerListUpdatedEventArgs> PlayerListUpdated;
        void Connect(string host, int port, string playerName);
        void Disconnect();
        void SendMessage(MessageType type, string message);
    }
}
