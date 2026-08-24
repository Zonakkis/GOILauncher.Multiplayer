namespace GOILauncher.Multiplayer.Unity.Events
{
    /// <summary>
    /// 一名远端玩家的场景实例刚刚创建（或从实例池借出）。
    /// </summary>
    /// <remarks>
    /// 借出来的实例带着上一个使用者的外观，模板本身带的又是本地玩家的外观，所以任何"按玩家
    /// 改外观"的模块都必须在这一刻补一次，不能只等网络事件——那名玩家的清单可能早就到了。
    /// </remarks>
    public class RemotePlayerInstanceCreatedEvent
    {
        public int PlayerId { get; private set; }

        public RemotePlayerInstanceCreatedEvent(int playerId)
        {
            PlayerId = playerId;
        }
    }
}
