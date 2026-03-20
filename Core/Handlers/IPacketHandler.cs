using GOILauncher.Multiplayer.Core.Packets;
using System;

namespace GOILauncher.Multiplayer.Core.Handlers
{
    public interface IPacketHandler
    {
        PacketType Type { get; }
        void Handle(ArraySegment<byte> data);
    }
}
