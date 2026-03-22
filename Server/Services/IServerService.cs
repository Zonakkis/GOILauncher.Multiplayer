using LiteNetLib.Utils;
using System;

namespace GOILauncher.Multiplayer.Server.Services
{
    public interface IServerService
    {
        void Start(int port);
        void Stop();
        void Poll();
        void Broadcast(
            INetSerializable packet, Func<ServerPlayer, bool> predicate = null);
    }
}