using System.Collections.Generic;
using GOILauncher.Multiplayer.Core.Data.Models;

namespace GOILauncher.Multiplayer.Server.Services
{
    public interface IPlayerService
    {
        Dictionary<int, PlayerInfo> Players { get; }
    }
}
