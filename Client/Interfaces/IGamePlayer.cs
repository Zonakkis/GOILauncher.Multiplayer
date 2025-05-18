using GOILauncher.Multiplayer.Shared.Game;
using UnityEngine;

namespace GOILauncher.Multiplayer.Client.Interfaces
{
    public interface IGamePlayer
    {
        Transform Player { get; }

        Transform Handle { get; }

        Move Move { get; }

        Move NextMove { get; }

        bool IsRenderersEnabled { get; }

        Move GetMove();

        void ApplyMove(Move move);

        void SetNextMove(Move move);

        void SetRenderersEnabled(bool enabled);
    }
}