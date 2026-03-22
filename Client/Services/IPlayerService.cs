using GOILauncher.Multiplayer.Client.Models;
using System.Collections.Generic;

namespace GOILauncher.Multiplayer.Client.Services
{
    public interface IPlayerService
    {
        Dictionary<int, ClientPlayer> Players { get; }
    }
}