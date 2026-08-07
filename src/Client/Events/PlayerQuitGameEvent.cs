using GOILauncher.Multiplayer.Core.Data.Models;

namespace GOILauncher.Multiplayer.Client.Events
{
    /// <summary>
    /// 某个远端玩家退出游戏（IsInGame true → false）时发布。
    /// </summary>
    public class PlayerQuitGameEvent
    {
        public PlayerInfo Player { get; }

        public PlayerQuitGameEvent(PlayerInfo player)
        {
            Player = player;
        }
    }
}
