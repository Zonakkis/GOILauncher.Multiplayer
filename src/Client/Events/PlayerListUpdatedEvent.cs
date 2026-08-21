namespace GOILauncher.Multiplayer.Client.Events
{
    /// <summary>
    /// 名单快照发生变化时发布，供 UI 刷新。
    ///
    /// 不携带名单：IPlayerService 是唯一权威来源，订阅者刷新时从那里读，
    /// 否则每个订阅者手里都会留下一份随时可能过期的副本。
    ///
    /// 这只是"有变化"的信号，不是实例生命周期的驱动源——需要按玩家增删远端实例的
    /// 模块订阅 PlayerJoined / PlayerLeft / PlayerEnteredGame / PlayerQuitGame /
    /// PlayerRosterReceived 这些描述具体变化的事件。
    /// </summary>
    public class PlayerListUpdatedEvent
    {
    }
}
