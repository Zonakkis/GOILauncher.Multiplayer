using System.Collections.Generic;

namespace GOILauncher.Multiplayer.Server.Services
{
    public interface IPlayerService
    {
        Dictionary<int, ServerPlayer> Players { get; }
        bool TryGet(int playerId, out ServerPlayer player);
        bool TryRemove(int playerId, out ServerPlayer player);
    }
}
