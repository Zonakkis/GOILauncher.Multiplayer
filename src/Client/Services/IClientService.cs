using System;

namespace GOILauncher.Multiplayer.Client.Services
{
    public interface IClientService : IDisposable
    {
        bool IsConnected { get; }
        void Connect(string host, int port);
        void Disconnect();
        void Poll();
    }
}