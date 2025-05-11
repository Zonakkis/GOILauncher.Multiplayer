using GOILauncher.Multiplayer.Shared.Game;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Shared.Packets
{
    public class PlayerEnteredPacket : IPacket
    {
        public PacketType PacketType => PacketType.PlayerEntered;
        public int PlayerId { get; set; }
        public Move InitMove { get; set; } = new Move();

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerId);
            writer.Put(InitMove);
        }

        public void Deserialize(NetDataReader reader)
        {
            PlayerId = reader.GetInt();
            InitMove = reader.Get<Move>();
        }
    }
}
