using System;

namespace GOILauncher.Multiplayer.Client.Services
{
    public interface IClientService : IDisposable
    {
        int LocalPlayerId { get; }
        void Connect(string host, int port);
        void Disconnect();
        void Poll();
        void SendChatMessage(string message);
    }
}