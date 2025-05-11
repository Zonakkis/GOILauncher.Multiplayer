using GOILauncher.Multiplayer.Shared.Game;
using GOILauncher.Multiplayer.Shared.Interfaces;
using UnityEngine;

namespace GOILauncher.Multiplayer.Client
{
    public class ClientPlayer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public RuntimePlatform Platform { get; set; }
        public bool IsInGame { get; set; }
        public IPlayer Player { get; set; }
        public Move InitMove { get; set; }
    }
}
