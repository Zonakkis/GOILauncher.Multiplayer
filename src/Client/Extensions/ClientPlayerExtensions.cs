using GOILauncher.Multiplayer.Client.Models;

namespace GOILauncher.Multiplayer.Client.Extensions
{
    public static class ClientPlayerExtensions
    {
        public static string Format(this IClientPlayer player)
        {
            return $"[{player.Info.Id}][{player.Info.Platform}]{player.Info.Name}";
        }
    }
}
