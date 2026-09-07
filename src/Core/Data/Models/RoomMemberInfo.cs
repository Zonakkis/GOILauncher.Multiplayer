using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Models
{
    /// <summary>A membership incarnation, not just a reusable network peer ID.</summary>
    public struct RoomMemberInfo : INetSerializable
    {
        public PlayerInfo Player { get; private set; }
        public ulong MembershipId { get; private set; }
        public RoomMemberInfo(PlayerInfo player, ulong membershipId)
        { Player = player; MembershipId = membershipId; }
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Player.Id); writer.Put(Player.Name); writer.Put((byte)Player.Platform);
            writer.Put(Player.IsInGame); writer.Put(MembershipId);
        }
        public void Deserialize(NetDataReader reader)
        {
            Player = new PlayerInfo(reader.GetInt(), reader.GetString(), (Platform)reader.GetByte(), reader.GetBool());
            MembershipId = reader.GetULong();
        }
    }

    /// <summary>Guards both ends against packets queued before either player's last room change.</summary>
    public struct RoomPacketScope : INetSerializable
    {
        public ulong RecipientMembershipId { get; set; }
        public ulong PlayerMembershipId { get; set; }
        public RoomPacketScope(ulong recipientMembershipId, ulong playerMembershipId)
        { RecipientMembershipId = recipientMembershipId; PlayerMembershipId = playerMembershipId; }
        public void Serialize(NetDataWriter writer)
        { writer.Put(RecipientMembershipId); writer.Put(PlayerMembershipId); }
        public void Deserialize(NetDataReader reader)
        { RecipientMembershipId = reader.GetULong(); PlayerMembershipId = reader.GetULong(); }
    }
}
