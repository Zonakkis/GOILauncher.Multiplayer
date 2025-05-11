using GOILauncher.Multiplayer.Shared.Game;
using LiteNetLib;
using UnityEngine;

namespace GOILauncher.Multiplayer.Server
{
    public class ServerPlayer
    {
        public NetPeer Peer { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public RuntimePlatform Platform { get; set; }
        public bool IsInGame { get; set; }
        public Move Move { get; set; }
    }
}
