using System;

namespace GOILauncher.Multiplayer.Core.Data.Handlers
{
    public interface IPacketDispatcher
    {
        void Dispatch(ArraySegment<byte> data);
    }
}