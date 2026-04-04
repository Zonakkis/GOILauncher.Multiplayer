namespace GOILauncher.Multiplayer.Unity
{
    public interface IUnityServer
    {
        bool IsRunning { get; }
        void Start(int port);
        void Stop();
    }
}