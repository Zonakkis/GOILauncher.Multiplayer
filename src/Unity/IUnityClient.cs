namespace GOILauncher.Multiplayer.Client
{
    public interface IUnityClient
    {
        bool IsConnected { get; }
        void Connect(string host, int port, string playerName);
        void Disconnect();
    }
}
