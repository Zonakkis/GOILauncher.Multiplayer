namespace GOILauncher.Multiplayer.Core.Data.Constants
{
    /// <summary>
    /// LiteNetLib 的通道号在整个协议里是全局分配的，所以集中编号在这里：两个功能各自
    /// 挑号会撞车，而撞车的表现不是报错——同一条可靠通道上两种包会互相排在对方后面。
    /// </summary>
    public static class NetworkChannels
    {
        /// <summary>状态同步和其余所有包走的通道。</summary>
        public const byte Default = 0;

        /// <summary>
        /// 皮肤清单和贴图字节走的通道。一张贴图有几百 KB，要分片可靠重传，
        /// 和 60 Hz 的状态同步挤同一条可靠通道会互相拖慢。
        /// </summary>
        public const byte Skin = 1;

        /// <summary>
        /// <c>NetManager.ChannelsCount</c> 要设成这个值。它决定 <c>NetPeer._channels</c>
        /// 的长度（<c>ChannelsCount * 4</c>），小了的话往高号通道发包会直接下标越界。
        /// 收发两端都要设，否则收端映射不出通道。
        /// </summary>
        public const byte Count = 2;
    }
}
