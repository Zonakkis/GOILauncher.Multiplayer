using System.Collections.Generic;
using GOILauncher.Multiplayer.Core.Data.Models;
using LiteNetLib;

namespace GOILauncher.Multiplayer.Server.Services
{
    public interface IPlayerService
    {
        Dictionary<int, ServerPlayer> Players { get; }
        ServerPlayer AddOrUpdate(ServerPlayer player);
        bool TryGet(int playerId, out ServerPlayer player);
        bool TryRemove(int playerId, out ServerPlayer player);
        void Clear();
    }
}
