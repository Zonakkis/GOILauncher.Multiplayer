using GOILauncher.Multiplayer.Core.Data.Models;

namespace GOILauncher.Multiplayer.Unity.Models
{
    public class RemotePlayer : PlayerBase
    {
        /// <summary>
        /// 应用远端玩家的状态（玩家/锤子/滑杆的位置与旋转），由状态同步阶段调用。
        /// </summary>
        public void ApplyState(PlayerState state)
        {
            // TODO: 状态同步阶段实现
        }
    }
}
