using GOILauncher.Multiplayer.Client.Interfaces;
using GOILauncher.Multiplayer.Shared.Game;
using UnityEngine;

namespace GOILauncher.Multiplayer.Client.Game
{
    public abstract class PlayerBase : MonoBehaviour, IGamePlayer
    {
        public Transform Player { get; private set; }
        public Transform Handle { get; private set; }
        public Transform Slider { get; private set; }
        public Move Move { get; protected set; }
        public Move NextMove { get; protected set; }
        public bool IsRenderersEnabled { get; private set; } = true;

        public void Awake()
        {
            Player = transform;
            Slider = transform.Find("Hub").transform.Find("Slider");
            Handle = Slider.transform.Find("Handle");
        }

        public Move GetMove()
        {
            return new Move
            {
                PlayerPosition = Player.position,
                PlayerRotation = Player.rotation,
                HandlePosition = Handle.position,
                HandleRotation = Handle.rotation,
                SliderPosition = Slider.position,
                SliderRotation = Slider.rotation
            };
        }

        public void ApplyMove(Move move)
        {
            Move = move;
            Player.position = move.PlayerPosition;
            Player.rotation = move.PlayerRotation;
            Handle.position = move.HandlePosition;
            Handle.rotation = move.HandleRotation;
            Slider.position = move.SliderPosition;
            Slider.rotation = move.SliderRotation;
        }

        public virtual void SetNextMove(Move move)
        {
            NextMove = move;
        }

        public void SetRenderersEnabled(bool enabled)
        {
            if(IsRenderersEnabled == enabled)
                return;
            foreach (var renderer in Player.GetComponentsInChildren<Renderer>())
                renderer.enabled = enabled;
            IsRenderersEnabled = enabled;
        }
    }
}