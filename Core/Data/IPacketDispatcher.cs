using System;
using GOILauncher.Multiplayer.Core.Data.Packets;
using LiteNetLib;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data
{
    public interface IPacketDispatcher
    {
        void RegisterStruct<TPacket>(Action<TPacket, NetPeer> onReceive)
             where TPacket : struct, IPacket, INetSerializable;
        void RegisterClass<TPacket>(Action<TPacket, NetPeer> onReceive)
             where TPacket : class, IPacket, new();
        void Dispatch(NetPeer peer, NetDataReader reader);
    }
}