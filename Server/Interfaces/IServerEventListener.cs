using System;
using GOILauncher.Multiplayer.Shared.Events;
using ChatMessageReceivedEventArgs = GOILauncher.Multiplayer.Server.Events.ChatMessageReceivedEventArgs;
using PlayerConnectedEventArgs = GOILauncher.Multiplayer.Server.Events.PlayerConnectedEventArgs;

namespace GOILauncher.Multiplayer.Server.Interfaces
{
    public interface IServerEventListener
    {
        event EventHandler<PlayerConnectedEventArgs> PlayerConnected;
        event EventHandler<ChatMessageReceivedEventArgs> ChatMessageReceived;
    }
}