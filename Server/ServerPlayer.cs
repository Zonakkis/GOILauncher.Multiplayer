using LiteNetLib;
using System;


namespace GOILauncher.Multiplayer.Server
{
    public class ServerPlayer
    {
        public NetPeer Peer { get; set; }
        public int Id => Peer.Id;
        public bool IsInGame { get; set; }
    }
}
