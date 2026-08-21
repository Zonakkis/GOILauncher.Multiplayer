using GOILauncher.Multiplayer.Core.Data.Models;

namespace GOILauncher.Multiplayer.Client.Events
{
    /// <summary>
    /// 本地玩家身份确立后发布：此时 IPlayerService.LocalPlayer 已写入服务端分配的 Id，
    /// 名单里也已经有本地玩家。
    ///
    /// 与 ServerHandshakeEvent 的区别是发布时机而非内容：ServerHandshakeEvent 由
    /// ClientService 在收到握手包时立刻发布，那时 PlayerService 还没更新自己的状态。
    /// 需要读取 IPlayerService 的模块订阅本事件，不要靠 EventBus 的订阅顺序去抢在
    /// PlayerService 后面执行。
    /// </summary>
    public class LocalPlayerReadyEvent
    {
        public PlayerInfo LocalPlayer { get; private set; }

        public LocalPlayerReadyEvent(PlayerInfo localPlayer)
        {
            LocalPlayer = localPlayer;
        }
    }
}
