using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Shared.Packets
{
    public class ChatMessagePacket : IPacket
    {
        public PacketType PacketType => PacketType.ChatMessage;
        public int PlayerId { get; set; }
        public string Message { get; set; }
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerId);
            writer.Put(Message);
        }

        public void Deserialize(NetDataReader reader)
        {
            PlayerId = reader.GetInt();
            Message = reader.GetString();
        }
    }
}
