using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Extensions;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Packets
{
    public struct S2CPlayerJoinedPacket : INetSerializable
    {
        public RoomPacketScope Scope { get; set; }
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public Platform Platform { get; set; }
        public bool IsInGame { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Scope);
            writer.Put(PlayerId);
            writer.Put(PlayerName);
            writer.Put(Platform);   
            writer.Put(IsInGame);
        }

        public void Deserialize(NetDataReader reader)
        {
            Scope = reader.Get<RoomPacketScope>();
            PlayerId = reader.GetInt();
            PlayerName = reader.GetString();
            Platform = reader.GetPlatform();
            IsInGame = reader.GetBool();
        }
    }
}
