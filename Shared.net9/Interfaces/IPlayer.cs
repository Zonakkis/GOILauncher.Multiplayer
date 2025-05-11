using GOILauncher.Multiplayer.Shared.Game;
using UnityEngine;

namespace GOILauncher.Multiplayer.Shared.Interfaces
{
    public interface IPlayer
    {
        Transform Player { get; }

        Move Move { get; }

        Move NextMove { get; }

        Move GetMove();

        void ApplyMove(Move move);

        void SetNextMove(Move move);
    }
}