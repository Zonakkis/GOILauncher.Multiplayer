using GOILauncher.Multiplayer.Core.Data.Models;
using System.Collections.Generic;

namespace GOILauncher.Multiplayer.Client.Services
{
    public interface IPlayerService
    {
        PlayerInfo LocalPlayer { get; }
        Dictionary<int, PlayerInfo> Players { get; }
        void SetLocalPlayerInfo(PlayerInfo info);
        void SetIsInGame(bool isInGame);
    }
}
