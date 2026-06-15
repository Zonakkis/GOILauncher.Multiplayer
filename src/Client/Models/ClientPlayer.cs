using GOILauncher.Multiplayer.Core.Data.Models;

namespace GOILauncher.Multiplayer.Client.Models
{
    public class ClientPlayer : IClientPlayer
    {
        public PlayerInfo Info { get; set; }
        IPlayerInfo IClientPlayer.Info => Info;
    }
}
