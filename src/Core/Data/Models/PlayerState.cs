using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Models
{
    /// <summary>
    /// 其中 PlayerRotation 和 SliderRotation 只绕 Z 轴旋转。
    /// </summary>
    public struct PlayerState : INetSerializable
    {
        public UnityVector3 PlayerPosition { get; set; }
        public float PlayerRotation { get; set; }
        public UnityVector3 SliderPosition { get; set; }
        public float SliderRotation { get; set; }
        public UnityVector3 HandlePosition { get; set; }
        public UnityQuaternion HandleRotation { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerPosition);
            writer.Put(PlayerRotation);
            writer.Put(SliderPosition);
            writer.Put(SliderRotation);
            writer.Put(HandlePosition);
            writer.Put(HandleRotation);
        }

        public void Deserialize(NetDataReader reader)
        {
            PlayerPosition = reader.Get<UnityVector3>();
            PlayerRotation = reader.GetFloat();
            SliderPosition = reader.Get<UnityVector3>();
            SliderRotation = reader.GetFloat();
            HandlePosition = reader.Get<UnityVector3>();
            HandleRotation = reader.Get<UnityQuaternion>();
        }
    }
}
