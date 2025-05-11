using GOILauncher.Multiplayer.Shared.Game;

namespace GOILauncher.Multiplayer.Shared.Packets
{
    public class PlayerMovePacket : IPacket
    {
        public PacketType PacketType => PacketType.PlayerMove;
        public int PlayerId { get; set; }
        public Move Move { get; set; }
        public void Serialize(LiteNetLib.Utils.NetDataWriter writer)
        {
            writer.Put(PlayerId);
            writer.Put(Move);
        }
        public void Deserialize(LiteNetLib.Utils.NetDataReader reader)
        {
            PlayerId = reader.GetInt();
            Move = reader.Get<Move>();
        }
    }
}
