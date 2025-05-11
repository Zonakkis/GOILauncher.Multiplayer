using GOILauncher.Multiplayer.Shared.Extensions;
using LiteNetLib.Utils;
using UnityEngine;

namespace GOILauncher.Multiplayer.Shared.Game
{
    public class Move : INetSerializable
    {
        public Vector3 PlayerPosition { get; set; }
        public Quaternion PlayerRotation { get; set; }
        public Vector3 HandlePosition { get; set; }
        public Quaternion HandleRotation { get; set; }
        public Vector3 SliderPosition { get; set; }
        public Quaternion SliderRotation { get; set; }
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
