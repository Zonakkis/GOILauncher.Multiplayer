using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Models
{
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
            writer.Put(PlayerRotation);
            writer.Put(HandlePosition);
            writer.Put(HandleRotation);
            writer.Put(SliderPosition);
            writer.Put(SliderRotation);
        }

        public void Deserialize(NetDataReader reader)
        {
            // 和 Serialize 的 Put 对称：读写都走模型自己的 INetSerializable，
            // 所以格式改了不用来这里跟着改（UnityQuaternion 就只传 Z/W）。
            PlayerPosition = reader.Get<UnityVector3>();
            PlayerRotation = reader.Get<UnityQuaternion>();
            HandlePosition = reader.Get<UnityVector3>();
            HandleRotation = reader.Get<UnityQuaternion>();
            SliderPosition = reader.Get<UnityVector3>();
            SliderRotation = reader.Get<UnityQuaternion>();
        }
    }
}
