using GOILauncher.Multiplayer.Core.Data.Models;

namespace GOILauncher.Multiplayer.Client.Events
{
    /// <summary>
    /// 某个远端玩家进入游戏（IsInGame false → true）时发布。
    /// </summary>
    public class PlayerEnteredGameEvent
    {
        public PlayerInfo Player { get; }

        public PlayerEnteredGameEvent(PlayerInfo player)
        {
            Player = player;
        }
    }
}
