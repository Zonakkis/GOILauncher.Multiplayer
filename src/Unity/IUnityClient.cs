using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Client.Models;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Unity.Events;

namespace GOILauncher.Multiplayer.Unity
{
    public interface IUnityClient
    {
        bool IsConnected { get; }
        IList<PlayerInfo> Players { get; }
        ReadOnlyCollection<Message> ChatMessages { get; }

        event EventHandler<PlayerListUpdatedEventArgs> PlayerListUpdated;
        void Connect(string host, int port, string playerName);
        void Disconnect();
        void SendMessage(MessageType type, string message);
    }
}
