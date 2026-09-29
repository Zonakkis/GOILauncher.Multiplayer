using System;
using System.Collections;
using System.Collections.Generic;
using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Client.Synchronization;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Unity.Events;
using GOILauncher.Multiplayer.Unity.Models;
using GOILauncher.Multiplayer.Unity.Player;
using UnityEngine;

namespace GOILauncher.Multiplayer.Unity.Opening
{
    public class OpeningSynchronizer : MonoBehaviour
    {
        public ClientOpeningSync OpeningSync { get; set; }
        public IClientEventBus EventBus { get; set; }
        public IGameManager GameManager { get; set; }
        public IPlayerManager PlayerManager { get; set; }
        public LocalOpeningReader OpeningReader { get; set; }

        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private Coroutine _readRoutine;

        public void Init()
        {
            _subscriptions.Add(EventBus.Subscribe<GameStartedEvent>(e => BeginRead()));
            _subscriptions.Add(EventBus.Subscribe<GameRestartedEvent>(e => BeginRead()));
            _subscriptions.Add(EventBus.Subscribe<GameQuitEvent>(e => ClearLocal()));
            _subscriptions.Add(EventBus.Subscribe<RemotePlayerInstanceCreatedEvent>(e => Apply(e.PlayerId)));
            _subscriptions.Add(EventBus.Subscribe<PlayerOpeningReceivedEvent>(e => Apply(e.PlayerId)));
        }

        private void BeginRead()
        {
            ClearLocal();
            _readRoutine = StartCoroutine(ReadLocal());
        }

        private IEnumerator ReadLocal()
        {
            var player = GameManager.Player;
            yield return null;
            _readRoutine = null;
            if (!GameManager.IsInGame || player == null || player != GameManager.Player)
                yield break;

            OpeningState state;
            // A failed read must not leave the preceding scene's animation playing remotely.
            if (!OpeningReader.TryRead(out state))
                state = default(OpeningState);
            OpeningSync.Announce(state);
        }

        private void Apply(int playerId)
        {
            var remote = PlayerManager.GetPlayer(playerId) as RemotePlayer;
            if (remote == null)
                return;

            OpeningState state;
            // Unknown instances must not expose an opening cloned from the local Player or
            // retained from the pool's previous occupant. A later announcement will replace it.
            if (!OpeningSync.TryGetOpening(playerId, out state))
                state = default(OpeningState);
            remote.ApplyOpening(state.RemainingSeconds);
        }

        private void ClearLocal()
        {
            StopRead();
            OpeningSync.ClearLocal();
        }

        private void StopRead()
        {
            if (_readRoutine != null)
            {
                StopCoroutine(_readRoutine);
                _readRoutine = null;
            }
        }

        private void OnDisable()
        {
            StopRead();
        }

        private void OnDestroy()
        {
            StopRead();
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
            _subscriptions.Clear();
        }
    }
}
