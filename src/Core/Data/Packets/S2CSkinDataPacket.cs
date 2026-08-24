using GOILauncher.Multiplayer.Core.Data.Models;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Packets
{
    /// <summary>某名玩家皮肤的 PNG 字节。</summary>
    public struct S2CSkinDataPacket : INetSerializable
    {
        public int PlayerId { get; set; }
        public SkinBlob Blob { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerId);
            writer.Put(Blob);
        }

        public void Deserialize(NetDataReader reader)
        {
            PlayerId = reader.GetInt();
            Blob = reader.Get<SkinBlob>();
        }
    }
}
