using System;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data
{
    public interface IPacketDispatcher
    {
        void RegisterStruct<TPacket>(Action<TPacket, PacketSender> onReceive)
             where TPacket : struct, INetSerializable;
        void RegisterClass<TPacket>(Action<TPacket, PacketSender> onReceive)
             where TPacket : class, INetSerializable, new();
        void Dispatch(PacketSender sender, NetDataReader reader);
    }

    /// <summary>
    /// Packet pipeline for the client role. Registrations here are only ever driven by
    /// bytes arriving on the client socket, so a server-bound packet can never reach a
    /// client handler (and vice versa) even when both roles run in one process.
    /// </summary>
    public interface IClientPacketDispatcher : IPacketDispatcher
    {
    }

    /// <summary>
    /// Packet pipeline for the server role. See <see cref="IClientPacketDispatcher"/>.
    /// </summary>
    public interface IServerPacketDispatcher : IPacketDispatcher
    {
    }
}
