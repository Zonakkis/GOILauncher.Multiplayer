

using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Models
{
    public struct UnityQuaternion : INetSerializable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }
        
        public void Serialize(NetDataWriter writer)
        {
            // 同步包中的Quaternion的X、Y都为0，不传输
            // writer.Put(X);
            // writer.Put(Y);
            writer.Put(Z);
            writer.Put(W);
        }

        public void Deserialize(NetDataReader reader)
        {
            // 同步包中的Quaternion的X、Y都为0，不传输
            // X = reader.GetFloat();
            // Y = reader.GetFloat();
            X = 0;
            Y = 0;
            Z = reader.GetFloat();
            W = reader.GetFloat();
        }
    }
}
