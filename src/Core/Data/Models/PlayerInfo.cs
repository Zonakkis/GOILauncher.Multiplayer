namespace GOILauncher.Multiplayer.Core.Data.Models
{
    public class PlayerInfo : IPlayerInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Platform Platform { get; set; }
        public bool IsInGame { get; set; }
    }
}