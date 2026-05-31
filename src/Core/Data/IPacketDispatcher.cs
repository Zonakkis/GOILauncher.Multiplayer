using System;
using LiteNetLib;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data
{
    public interface IPacketDispatcher
    {
        void RegisterStruct<TPacket>(Action<TPacket, NetPeer> onReceive)
             where TPacket : struct, INetSerializable;
        void RegisterClass<TPacket>(Action<TPacket, NetPeer> onReceive)
             where TPacket : class, INetSerializable, new();
        void Dispatch(NetPeer peer, NetDataReader reader);
    }
}