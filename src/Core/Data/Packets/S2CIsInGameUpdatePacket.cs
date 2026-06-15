using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Packets
{
    public struct S2CIsInGameUpdatePacket : INetSerializable
    {
        public int PlayerId { get; set; }
        public bool IsInGame { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerId);
            writer.Put(IsInGame);
        }

        public void Deserialize(NetDataReader reader)
        {
            PlayerId = reader.GetInt();
            IsInGame = reader.GetBool();
        }
    }
}