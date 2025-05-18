using GOILauncher.Multiplayer.Client.Interfaces;
using GOILauncher.Multiplayer.Shared.Game;
using UnityEngine;

namespace GOILauncher.Multiplayer.Client
{
    public class ClientPlayer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public RuntimePlatform Platform { get; set; }
        public bool IsInGame { get; set; }
        public IGamePlayer GamePlayer { get; set; }
        public Move Move { get; set; }
    }
}
