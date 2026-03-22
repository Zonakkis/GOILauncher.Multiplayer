using GOILauncher.Multiplayer.Core.Extensions;
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
            PlayerPosition = reader.GetVector3();
            PlayerRotation = reader.GetQuaternion();
            HandlePosition = reader.GetVector3();
            HandleRotation = reader.GetQuaternion();
            SliderPosition = reader.GetVector3();
            SliderRotation = reader.GetQuaternion();
        }
    }
}
