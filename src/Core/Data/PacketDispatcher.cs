using System;
using LiteNetLib;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data
{
    public class PacketDispatcher : IPacketDispatcher
    {
        private readonly NetPacketProcessor _processor;

        public PacketDispatcher(NetPacketProcessor processor)
        {
            _processor = processor;
        }

        public void RegisterStruct<TPacket>(Action<TPacket, NetPeer> onReceive)
             where TPacket : struct, INetSerializable
        {
            _processor.SubscribeNetSerializable(onReceive);
        }

        public void RegisterClass<TPacket>(Action<TPacket, NetPeer> onReceive)
             where TPacket : class, INetSerializable, new()
        {
            _processor.SubscribeNetSerializable(onReceive);
        }

        public void Dispatch(NetPeer peer, NetDataReader reader)
        {
            _processor.ReadAllPackets(reader, peer);
        }
    }
}
