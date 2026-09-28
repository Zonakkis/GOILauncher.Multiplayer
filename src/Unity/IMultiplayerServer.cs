namespace GOILauncher.Multiplayer.Unity
{
    public interface IMultiplayerServer
    {
        bool IsRunning { get; }
        void Start(int port);
        void Stop();
    }
}