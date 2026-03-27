using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Extensions;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Packets
{
    public struct C2SClientHandShakePacket : INetSerializable
    {
        public string PlayerName { get; set; }
        public Platform Platform { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerName);
            writer.Put(Platform);
        }

        public void Deserialize(NetDataReader reader)
        {
            PlayerName = reader.GetString();
            Platform = reader.GetPlatform();
        }
    }
}