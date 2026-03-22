using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Extensions;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Packets
{
    public struct S2CPlayerJoinedPacket : INetSerializable
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public Platform Platform { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerId);
            writer.Put(PlayerName);
            writer.Put(Platform);
        }

        public void Deserialize(NetDataReader reader)
        {
            PlayerId = reader.GetInt();
            PlayerName = reader.GetString();
            Platform = reader.GetPlatform();
        }
    }
}
