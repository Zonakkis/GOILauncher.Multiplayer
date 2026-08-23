using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Models
{
    /// <summary>
    /// 线上的四元数只有 Z 和 W 两个 float，X/Y 按 0 还原。
    /// </summary>
    /// <remarks>
    /// 这不是压缩，是领域事实：需要同步的三个 Transform（<c>Player</c>、<c>Hub/Slider</c>、
    /// <c>Hub/Slider/Handle</c>）都只绕 Z 轴转，世界旋转的 X/Y 恒为 0（已实机确认，见
    /// <c>docs/game-runtime.md</c>）。所以这个类型对状态同步是有意有损的：X/Y 写进来会被丢掉。
    ///
    /// 想改成传全四个分量的话，改 <see cref="Serialize"/> 和 <see cref="Deserialize"/> 就够了——
    /// 格式只存在于这一处，没有第二份实现要跟着改。
    /// </remarks>
    public struct UnityQuaternion : INetSerializable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Z);
            writer.Put(W);
        }

        public void Deserialize(NetDataReader reader)
        {
            X = 0;
            Y = 0;
            Z = reader.GetFloat();
            W = reader.GetFloat();
        }
    }
}
