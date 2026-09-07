using UnityEngine;

namespace GOILauncher.Multiplayer.Unity
{
    public interface IGameManager
    {
        bool IsInGame { get; }
        GameObject Player { get; }
        GameObject PlayerPrefab { get; }
        GameObject Cursor { get; }
    }
}
