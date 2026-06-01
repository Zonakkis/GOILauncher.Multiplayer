using GOILauncher.Multiplayer.Core.Data.Models;
using LiteNetLib;


namespace GOILauncher.Multiplayer.Server
{
    public class ServerPlayer : IServerPlayer
    {
        public NetPeer Peer { get; set; }
        public PlayerInfo Info { get; set; }
    }
}
