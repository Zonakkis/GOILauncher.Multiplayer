using GOILauncher.Multiplayer.Core.Data.Packets;
using System;

namespace GOILauncher.Multiplayer.Core.Data.Handlers
{
    public interface IPacketHandler<TPacket> where TPacket : IPacket
    {
        void Handle(TPacket packet);
    }
}
