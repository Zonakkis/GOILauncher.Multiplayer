using GOILauncher.Multiplayer.Core.Data.Models;

namespace GOILauncher.Multiplayer.Client.Events
{
    /// <summary>
    /// 一名远端玩家的皮肤已经可以渲染了：清单到了，并且（如果有自定义贴图）字节也到了。
    /// </summary>
    /// <remarks>
    /// <see cref="Payload"/> 为 null 就是"用内置的原版贴图"，不管 <see cref="State"/> 里的哈希
    /// 是什么——服务端说那张贴图拿不到时也走这条路。<see cref="State"/> 始终是金度的唯一来源，
    /// 原版贴图也要照它设 <c>_Goldness</c>，否则金罐玩家会变成黑罐。
    /// </remarks>
    public class PlayerSkinReceivedEvent
    {
        public int PlayerId { get; private set; }
        public SkinState State { get; private set; }

        /// <summary>PNG 字节；null 表示按原版贴图渲染。收到方不得改写数组内容。</summary>
        public byte[] Payload { get; private set; }

        public PlayerSkinReceivedEvent(int playerId, SkinState state, byte[] payload)
        {
            PlayerId = playerId;
            State = state;
            Payload = payload;
        }
    }
}
