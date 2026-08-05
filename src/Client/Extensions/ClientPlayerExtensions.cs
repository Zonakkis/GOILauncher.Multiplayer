using GOILauncher.Multiplayer.Core.Data.Models;

namespace GOILauncher.Multiplayer.Client.Extensions
{
    public static class ClientPlayerExtensions
    {
        public static string Format(this PlayerInfo player)
        {
            return $"[{player.Id}][{player.Platform}]{player.Name}";
        }
    }
}
