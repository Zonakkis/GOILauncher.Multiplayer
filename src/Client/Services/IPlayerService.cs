using GOILauncher.Multiplayer.Client.Models;
using GOILauncher.Multiplayer.Core.Data.Models;
using System.Collections.Generic;

namespace GOILauncher.Multiplayer.Client.Services
{
    public interface IPlayerService
    {
        ClientPlayer LocalPlayer { get; }
        Dictionary<int, ClientPlayer> Players { get; }
        void SetLocalPlayerInfo(IPlayerInfo info);
    }
}