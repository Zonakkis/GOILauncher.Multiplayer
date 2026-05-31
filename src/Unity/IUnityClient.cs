using System;
using System.Collections.Generic;
using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Client.Models;

namespace GOILauncher.Multiplayer.Client
{
    public interface IUnityClient
    {
        bool IsConnected { get; }
        IList<ClientPlayer> Players { get; }
        List<Message> ChatMessages { get; }

        event EventHandler<PlayerListUpdatedEventArgs> PlayerListUpdated;
        void Connect(string host, int port, string playerName);
        void Disconnect();
        void SendMessage(MessageType type, string message);
    }
}
