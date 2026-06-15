using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Packets
{
    public struct C2SIsInGameUpdatePacket : INetSerializable
    {
        public bool IsInGame { get; set; }
        public C2SIsInGameUpdatePacket(bool isInGame)
        {
            IsInGame = isInGame;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(IsInGame);
        }
        
        public void Deserialize(NetDataReader reader)
        {
            IsInGame = reader.GetBool();
        }
    }
}
