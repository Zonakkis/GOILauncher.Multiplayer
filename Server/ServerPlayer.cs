using GOILauncher.Multiplayer.Core.Data.Models;
using LiteNetLib;


namespace GOILauncher.Multiplayer.Server
{
    public class ServerPlayer
    {
        public NetPeer Peer { get; set; }
        public int Id => Peer.Id;
        public string Name { get; set; }
        public Platform Platform { get; set; }
        public bool IsInGame { get; set; }
    }
}
