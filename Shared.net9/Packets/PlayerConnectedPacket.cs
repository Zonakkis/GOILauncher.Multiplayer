using GOILauncher.Multiplayer.Shared.Game;
using LiteNetLib.Utils;
using UnityEngine;

namespace GOILauncher.Multiplayer.Shared.Packets
{
    public class PlayerConnectedPacket : IPacket
    {
        public PacketType PacketType => PacketType.PlayerConnected;
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public RuntimePlatform Platform { get; set; }
        public bool IsInGame { get; set; }
        public Move InitMove { get; set; }
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerId);
            writer.Put(PlayerName);
            writer.Put((byte)Platform);
            writer.Put(IsInGame);
            writer.Put(InitMove);
        }

        public void Deserialize(NetDataReader reader)
        {
            PlayerId = reader.GetInt();
            PlayerName = reader.GetString();
            Platform = (RuntimePlatform)reader.GetByte();
            IsInGame = reader.GetBool();
            InitMove = reader.Get<Move>();
        }
    }
}
