using LiteNetLib;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Network
{
    public interface INetworkServer
    {
        void Start(int port);
        void Stop();
        void Poll();
        void Send<T>(int clientId, T packet, DeliveryMethod method) where T : class, new();
        void Send(int clientId, INetSerializable packet, DeliveryMethod method);
    }
}
