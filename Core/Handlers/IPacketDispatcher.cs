using System;

namespace GOILauncher.Multiplayer.Core.Handlers
{
    public interface IPacketDispatcher
    {
        void Dispatch(ArraySegment<byte> data);
    }
}