using System;
using GOILauncher.Multiplayer.Client.Events;

namespace GOILauncher.Multiplayer.Client.Interfaces
{
    public interface IClientEventListener
    {
        event EventHandler<ServerConnectedEventArgs> ServerConnected;
        event EventHandler<ServerDisconnectedEventArgs> ServerDisconnected;
        event EventHandler<PlayerConnectedEventArgs> PlayerConnected;
        event EventHandler<PlayerDisconnectedEventArgs> PlayerDisconnected;
        event EventHandler<PlayerEnteredEventArgs> PlayerEntered;
        event EventHandler<PlayerLeftEventArgs> PlayerLeft;
        event EventHandler<PlayerMovedEventArgs> PlayerMoved;
        event EventHandler<ChatMessageReceivedEventArgs> ChatMessageReceived;
    }
}