using System.Collections.Generic;
using GOILauncher.Multiplayer.Core.Data.Models;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Packets
{
    public class S2CPlayerListPacket : INetSerializable
    {
        public List<IPlayerInfo> Players { get; set; } = new List<IPlayerInfo>();

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Players.Count);
            foreach (var player in Players)
            {
                writer.Put(player.Id);
                writer.Put(player.Name);
                writer.Put((byte)player.Platform);
                writer.Put(player.IsInGame);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            Players.Clear();
            int playerCount = reader.GetInt();
            for (int i = 0; i < playerCount; i++)
            {
                Players.Add(new PlayerInfo
                {
                    Id = reader.GetInt(),
                    Name = reader.GetString(),
                    Platform = (Platform)reader.GetByte(),
                    IsInGame = reader.GetBool()
                });
            }
        }
    }
}
