using LiteNetLib;

namespace GOILauncher.Multiplayer.Network
{
    public interface INetworkServer
    {
        void Start(int port);
        void Stop();
        void Poll();
        void Send(int clientId, byte[] data, DeliveryMethod method);
    }
}
