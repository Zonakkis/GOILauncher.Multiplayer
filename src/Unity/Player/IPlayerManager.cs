using System.Collections.Generic;
using GOILauncher.Multiplayer.Unity.Models;

namespace GOILauncher.Multiplayer.Unity.Player
{
    public interface IPlayerManager
    {
        LocalPlayer LocalPlayer { get; }
        PlayerBase GetPlayer(int playerId);
        IEnumerable<PlayerBase> Players { get; }
    }
}
