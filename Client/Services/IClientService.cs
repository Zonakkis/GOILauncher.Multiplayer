namespace GOILauncher.Multiplayer.Client.Services
{
    public interface IClientService
    {
        int LocalPlayerId { get; }
        void Connect(string host, int port);
    }
}