using System;
using GOILauncher.Multiplayer.Client.Events;

namespace GOILauncher.Multiplayer.Client.Interfaces
{
    public interface IGamePlayerManager
    {
        event EventHandler<PlayerMovedEventArgs> PlayerMoved;
        void ResetGame();
    }
}