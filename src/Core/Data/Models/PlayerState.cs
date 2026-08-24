using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Models
{
    /// <summary>
    /// 其中 PlayerRotation 和 HandleRotation 只传 Z/W，X/Y 恒为0。
    /// </summary>
    public struct PlayerState : INetSerializable
    {
        public UnityVector3 PlayerPosition { get; set; }
        public UnityQuaternion PlayerRotation { get; set; }
        public UnityVector3 HandlePosition { get; set; }
        public UnityQuaternion HandleRotation { get; set; }
        public UnityVector3 SliderPosition { get; set; }
        public UnityQuaternion SliderRotation { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerPosition);
            writer.Put(PlayerRotation.Z);
            writer.Put(PlayerRotation.W);
            writer.Put(HandlePosition);
            writer.Put(HandleRotation.Z);
            writer.Put(HandleRotation.W);
            writer.Put(SliderPosition);
            writer.Put(SliderRotation);
        }

        public void Deserialize(NetDataReader reader)
        {
            // 和 Serialize 的 Put 对称：读写都走模型自己的 INetSerializable，
            // 所以格式改了不用来这里跟着改（UnityQuaternion 就只传 Z/W）。
            PlayerPosition = reader.Get<UnityVector3>();
            PlayerRotation = new UnityQuaternion { Z = reader.GetFloat(), W = reader.GetFloat() };
            HandlePosition = reader.Get<UnityVector3>();
            HandleRotation = new UnityQuaternion { Z = reader.GetFloat(), W = reader.GetFloat() };
            SliderPosition = reader.Get<UnityVector3>();
            SliderRotation = reader.Get<UnityQuaternion>();
        }
    }
}
