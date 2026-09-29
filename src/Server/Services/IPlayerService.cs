using System.Collections.Generic;
using GOILauncher.Multiplayer.Core.Data.Models;

namespace GOILauncher.Multiplayer.Server.Services
{
    public interface IPlayerService
    {
        IEnumerable<PlayerInfo> Players { get; }

        bool TryGetPlayer(int playerId, out PlayerInfo player);
    }
}
