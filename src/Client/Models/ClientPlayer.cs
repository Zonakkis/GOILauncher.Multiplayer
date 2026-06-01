using GOILauncher.Multiplayer.Core.Data.Models;

namespace GOILauncher.Multiplayer.Client.Models
{
    public class ClientPlayer : IClientPlayer
    {
        public IPlayerInfo Info { get; set; }
    }
}
