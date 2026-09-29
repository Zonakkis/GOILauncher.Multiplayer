using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autofac;
using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Client.Services;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Constants;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Core.Utils;
using GOILauncher.Multiplayer.Network;
using LiteNetLib;

namespace GOILauncher.Multiplayer.Client.Synchronization
{
    /// <summary>
    /// Keeps one scene's local Opening snapshot and the current room's remote countdowns.
    /// Only time spent in this process's cache is subtracted; no clock crosses the network.
    /// Unity owns sampling and calls ClearLocal when the scene ends or restarts.
    /// </summary>
    public sealed class ClientOpeningSync : IStartable, IDisposable
    {
        private readonly INetworkClient _networkClient;
        private readonly IClientPacketDispatcher _dispatcher;
        private readonly IClientEventBus _eventBus;
        private readonly ILogger<ClientOpeningSync> _logger;
        private readonly IPlayerService _players;
        private readonly OpeningClock _clock;
        private readonly Dictionary<int, OpeningCountdown> _remoteOpenings =
            new Dictionary<int, OpeningCountdown>();
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private OpeningCountdown? _localOpening;
        private bool _disposed;

        public ClientOpeningSync(INetworkClient networkClient,
            IClientPacketDispatcher dispatcher, IClientEventBus eventBus,
            ILogger<ClientOpeningSync> logger, IPlayerService players)
            : this(networkClient, dispatcher, eventBus, logger, players,
                () => (double)Stopwatch.GetTimestamp() / Stopwatch.Frequency)
        {
        }

        // Test seam: no sleeps or wall clock. OpeningClock is not registered with Autofac,
        // so production resolves the five-argument constructor above.
        public ClientOpeningSync(INetworkClient networkClient,
            IClientPacketDispatcher dispatcher, IClientEventBus eventBus,
            ILogger<ClientOpeningSync> logger, IPlayerService players, OpeningClock clock)
        {
            _networkClient = networkClient;
            _dispatcher = dispatcher;
            _eventBus = eventBus;
            _logger = logger;
            _players = players;
            _clock = clock;
        }

        void IStartable.Start()
        {
            _dispatcher.RegisterStruct<S2COpeningStatePacket>(OnOpeningReceived);
            _subscriptions.Add(_eventBus.Subscribe<RoomMembershipChangedEvent>(OnRoomChanged));
            _subscriptions.Add(_eventBus.Subscribe<ServerDisconnectedEvent>(OnDisconnected));
            _subscriptions.Add(_eventBus.Subscribe<PlayerLeftEvent>(OnPlayerLeft));
            _subscriptions.Add(_eventBus.Subscribe<PlayerQuitGameEvent>(OnPlayerQuitGame));
        }

        /// <summary>
        /// Replaces the local sample, even if its duration is unchanged (a scene restart).
        /// Offline samples keep aging until membership is ready; zero is a real announcement.
        /// </summary>
        public void Announce(OpeningState state)
        {
            if (_disposed) return;
            if (!state.IsValid)
            {
                _logger.Warn("Refusing to announce an invalid Opening state.");
                return;
            }

            _localOpening = new OpeningCountdown(state, _clock());
            SendLocalOpening();
        }

        /// <summary>False means no remote snapshot; an expired snapshot remains known zero.</summary>
        public bool TryGetOpening(int playerId, out OpeningState state)
        {
            OpeningCountdown countdown;
            if (!_remoteOpenings.TryGetValue(playerId, out countdown))
            {
                state = default(OpeningState);
                return false;
            }

            state = countdown.GetState(_clock());
            return true;
        }

        /// <summary>Forgets the previous scene without announcing completion or touching remotes.</summary>
        public void ClearLocal()
        {
            _localOpening = null;
        }

        private void SendLocalOpening()
        {
            if (!_networkClient.IsConnected || _players.LocalMembershipId == 0 ||
                !_localOpening.HasValue)
            {
                return;
            }

            // LocalPlayer.IsInGame is reset on disconnect, independently of the Unity scene.
            // ClearLocal, not that roster flag, owns whether this scene's sample still exists.
            _networkClient.Send(new C2SOpeningStatePacket
            {
                MembershipId = _players.LocalMembershipId,
                State = _localOpening.Value.GetState(_clock())
            }, NetworkChannels.Default, DeliveryMethod.ReliableOrdered);
        }

        private void OnOpeningReceived(S2COpeningStatePacket packet, PacketSender _)
        {
            if (_disposed || !_players.AcceptsRemote(packet.PlayerId, packet.Scope)) return;
            if (!packet.State.IsValid)
            {
                _logger.Warn("Received an invalid Opening state for player {PlayerId}.", packet.PlayerId);
                return;
            }

            PlayerInfo player;
            if (!_players.TryGetPlayer(packet.PlayerId, out player) || !player.IsInGame) return;

            _remoteOpenings[packet.PlayerId] = new OpeningCountdown(packet.State, _clock());
            _eventBus.Publish(new PlayerOpeningReceivedEvent(packet.PlayerId, packet.State));
        }

        private void OnRoomChanged(RoomMembershipChangedEvent e)
        {
            _remoteOpenings.Clear();
            SendLocalOpening();
        }

        private void OnDisconnected(ServerDisconnectedEvent e)
        {
            // Same-scene reconnect must not restart or discard the locally sampled countdown.
            _remoteOpenings.Clear();
        }

        private void OnPlayerLeft(PlayerLeftEvent e)
        {
            _remoteOpenings.Remove(e.PlayerId);
        }

        private void OnPlayerQuitGame(PlayerQuitGameEvent e)
        {
            _remoteOpenings.Remove(e.Player.Id);
        }

        public void Dispose()
        {
            // The dispatcher has no unregister API; its callback ignores packets after disposal.
            _disposed = true;
            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }
            _subscriptions.Clear();
            _remoteOpenings.Clear();
            ClearLocal();
        }
    }
}
