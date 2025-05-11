using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Shared.Packets
{
    public interface IPacket : INetSerializable
    {
        PacketType PacketType { get; }
    }
}
