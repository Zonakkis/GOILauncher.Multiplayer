using System.Collections.Generic;

namespace GOILauncher.Multiplayer.Server.Services
{
    public interface IPlayerService
    {
        Dictionary<int, ServerPlayer> Players { get; }
    }
}
