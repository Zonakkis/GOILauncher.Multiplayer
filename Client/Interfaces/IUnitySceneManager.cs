using System;

namespace GOILauncher.Multiplayer.Shared.Interfaces
{
    public interface IUnitySceneManager
    {
        bool IsInGame { get; }

        event EventHandler GameSceneEntered;
        event EventHandler GameSceneLeft;
        event EventHandler GameRestarted;
    }
}