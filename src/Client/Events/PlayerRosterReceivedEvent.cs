namespace GOILauncher.Multiplayer.Client.Events
{
    /// <summary>
    /// 收到服务端的完整名单快照（S2CPlayerListPacket）并写入 IPlayerService 后发布。
    ///
    /// 服务端只在握手时发送一次该包，所以这是"加入一个已经有人的服务器"时唯一能得知
    /// 这些玩家存在的时机——他们不会再产生 PlayerJoinedEvent。
    ///
    /// 不携带名单：订阅者从 IPlayerService 读取，避免又多出一份快照副本。
    /// </summary>
    public class PlayerRosterReceivedEvent
    {
    }
}
