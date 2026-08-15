using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Client.Synchronization;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Unity.Models;
using UnityEngine;

namespace GOILauncher.Multiplayer.Unity
{
    /// <summary>
    /// Unity adapter for the player-state sync Module. It samples the local Player in
    /// LateUpdate and applies accepted remote states after the PlayerManager has created
    /// their pooled instances.
    /// </summary>
    public class PlayerStateSynchronizer : MonoBehaviour
    {
        private const float StateInterval = 1f / 60f;

        public ClientPlayerStateSync StateSync { get; set; }
        public IEventBus EventBus { get; set; }
        public IGameManager GameManager { get; set; }
        public IPlayerManager PlayerManager { get; set; }

        private float _elapsed;

        public void Init()
        {
            EventBus.Subscribe<PlayerStateReceivedEvent>(OnPlayerStateReceived);
        }

        public void LateUpdate()
        {
            if (!GameManager.IsInGame || !StateSync.IsConnected)
            {
                _elapsed = 0f;
                return;
            }

            var localPlayer = PlayerManager.LocalPlayer;
            if (localPlayer == null)
            {
                _elapsed = 0f;
                return;
            }

            _elapsed += Time.deltaTime;
            if (_elapsed < StateInterval)
            {
                return;
            }

            _elapsed -= StateInterval;
            PlayerState state;
            if (localPlayer.TryCaptureState(out state))
            {
                StateSync.Send(state);
            }
        }

        private void OnPlayerStateReceived(PlayerStateReceivedEvent e)
        {
            if (!GameManager.IsInGame)
            {
                return;
            }

            var remotePlayer = PlayerManager.GetPlayer(e.PlayerId) as RemotePlayer;
            if (remotePlayer != null)
            {
                remotePlayer.ApplyState(e.State);
            }
        }
    }
}
