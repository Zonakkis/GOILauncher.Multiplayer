namespace GOILauncher.Multiplayer.Core.Data.Models
{
    public interface IPlayerInfo
    {
        int Id { get; }
        string Name { get; }
        Platform Platform { get; }
        bool IsInGame { get; }
    }
}