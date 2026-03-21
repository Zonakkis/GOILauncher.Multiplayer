namespace GOILauncher.Multiplayer.Core.Data.Packets.S2C
{
    public struct ServerHandShakePacket : IPacket
    {
        public PacketType Type => PacketType.ServerHandShake;
        public int PlayerId { get; set; }
    }
}