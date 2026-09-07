using GOILauncher.Multiplayer.Core.Data.Models;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Packets
{
    /// <summary>
    /// "这名玩家现在用的是这张皮肤"。<see cref="PlayerId"/> 由服务端按发送方填，
    /// 不取客户端包体里的值。
    /// </summary>
    public struct S2CSkinManifestPacket : INetSerializable
    {
        public RoomPacketScope Scope { get; set; }
        public int PlayerId { get; set; }
        public SkinState State { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Scope);
            writer.Put(PlayerId);
            writer.Put(State);
        }

        public void Deserialize(NetDataReader reader)
        {
            Scope = reader.Get<RoomPacketScope>();
            PlayerId = reader.GetInt();
            State = reader.Get<SkinState>();
        }
    }
}
