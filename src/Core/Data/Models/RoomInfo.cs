using GOILauncher.Multiplayer.Core.Data.Constants;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Models
{
    /// <summary>Public room metadata. Password verification material never belongs here.</summary>
    public sealed class RoomInfo : INetSerializable
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public bool HasPassword { get; private set; }
        public int MaxPlayers { get; private set; }
        public int PlayerCount { get; private set; }
        public int? OwnerPlayerId { get; private set; }
        public bool IsLobby => Id == RoomConstants.LobbyId;
        public bool IsFull => MaxPlayers > 0 && PlayerCount >= MaxPlayers;

        public RoomInfo() { }
        public RoomInfo(int id, string name, bool hasPassword, int maxPlayers, int playerCount, int? ownerPlayerId)
        {
            Id = id; Name = name; HasPassword = hasPassword; MaxPlayers = maxPlayers;
            PlayerCount = playerCount; OwnerPlayerId = ownerPlayerId;
        }
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id); writer.Put(Name); writer.Put(HasPassword); writer.Put(MaxPlayers);
            writer.Put(PlayerCount); writer.Put(OwnerPlayerId.HasValue);
            if (OwnerPlayerId.HasValue) writer.Put(OwnerPlayerId.Value);
        }
        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetInt(); Name = reader.GetString(); HasPassword = reader.GetBool();
            MaxPlayers = reader.GetInt(); PlayerCount = reader.GetInt();
            OwnerPlayerId = reader.GetBool() ? (int?)reader.GetInt() : null;
        }
    }
}
