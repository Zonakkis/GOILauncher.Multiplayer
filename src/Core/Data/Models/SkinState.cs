using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Models
{
    /// <summary>
    /// 一名玩家某个部件当前的皮肤：换的是哪个槽位、用哪张贴图、金度多少。
    /// 不含贴图字节——字节按 <see cref="Hash"/> 单独索取，收端手上有同哈希的就不用再传。
    /// </summary>
    public struct SkinState : INetSerializable
    {
        public byte Slot { get; set; }

        /// <summary>贴图的内容哈希；空哈希表示这名玩家用原版贴图。</summary>
        public SkinHash Hash { get; set; }

        /// <summary>
        /// 材质的 <c>_Goldness</c>：黑罐 0，金罐 1。它和贴图是同一个材质上的两件事，
        /// 所以原版贴图（空哈希）也要带上它——金罐玩家没装皮肤时靠的就是这个值。
        /// </summary>
        public float Goldness { get; set; }

        public bool HasTexture
        {
            get { return !Hash.IsEmpty; }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Slot);
            writer.Put(Hash);
            writer.Put(Goldness);
        }

        public void Deserialize(NetDataReader reader)
        {
            Slot = reader.GetByte();
            Hash = reader.Get<SkinHash>();
            Goldness = reader.GetFloat();
        }
    }
}
