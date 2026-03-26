using GOILauncher.Multiplayer.Client.Extensions;
using GOILauncher.Multiplayer.Client.Models;
using System.Collections.Generic;

namespace GOILauncher.Multiplayer.Client.Services
{
    public interface IPlayerService
    {
        ClientPlayer LocalPlayer { get; }
        Dictionary<int, ClientPlayer> Players { get; }
        void UpdateLocalPlayerMetadata(PlayerMetadata metadata);
    }
}