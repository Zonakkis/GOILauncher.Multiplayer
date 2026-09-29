using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autofac;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Constants;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Core.Utils;
using GOILauncher.Multiplayer.Network;
using GOILauncher.Multiplayer.Server.Events;
using GOILauncher.Multiplayer.Server.Services;
using LiteNetLib;

namespace GOILauncher.Multiplayer.Server.Synchronization
{
    /// <summary>
    /// Keeps connected players' Opening countdowns, routing snapshots only through current room memberships.
    /// All cache access runs on the server's Poll thread; no clock value crosses the network.
    /// </summary>
    public sealed class OpeningRelay : IStartable, IDisposable
    {
        private readonly INetworkServer _network;
        private readonly IServerPacketDispatcher _dispatcher;
        private readonly IPlayerService _players;
        private readonly IRoomService _rooms;
        private readonly IServerEventBus _events;
        private readonly ILogger<OpeningRelay> _logger;
        private readonly OpeningClock _clock;
        private readonly Dictionary<int, OpeningCountdown> _openings = new Dictionary<int, OpeningCountdown>();
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private bool _disposed;

        public OpeningRelay(INetworkServer network, IServerPacketDispatcher dispatcher,
            IPlayerService players, IRoomService rooms, IServerEventBus events, ILogger<OpeningRelay> logger)
            : this(network, dispatcher, players, rooms, events, logger,
                () => (double)Stopwatch.GetTimestamp() / Stopwatch.Frequency)
        {
        }

        // OpeningClock is not registered with Autofac; production uses the constructor above.
        public OpeningRelay(INetworkServer network, IServerPacketDispatcher dispatcher,
            IPlayerService players, IRoomService rooms, IServerEventBus events,
            ILogger<OpeningRelay> logger, OpeningClock clock)
        {
            _network = network;
            _dispatcher = dispatcher;
            _players = players;
            _rooms = rooms;
            _events = events;
            _logger = logger;
            _clock = clock;
        }

        void IStartable.Start()
        {
            _dispatcher.RegisterStruct<C2SOpeningStatePacket>(OnOpening);
            _subscriptions.Add(_events.Subscribe<PlayerRoomEnteredEvent>(OnRoomEntered));
            _subscriptions.Add(_events.Subscribe<PlayerStatusChangedEvent>(OnStatusChanged));
            _subscriptions.Add(_events.Subscribe<ClientDisconnectedEvent>(e => _openings.Remove(e.ClientId)));
            _subscriptions.Add(_events.Subscribe<ServerStoppedEvent>(e => _openings.Clear()));
        }

        private void OnOpening(C2SOpeningStatePacket packet, PacketSender sender)
        {
            PlayerInfo player;
            if (_disposed || !_players.TryGetPlayer(sender.Id, out player)
                || player == null || !player.IsInGame
                || !_rooms.IsCurrentMembership(sender.Id, packet.MembershipId)) return;
            if (!packet.State.IsValid)
            {
                _logger.Warn("Player {PlayerId} announced an invalid Opening state, ignored.", sender.Id);
                return;
            }

            // Equal durations are new announcements too: restarting a scene restarts the countdown.
            _openings[sender.Id] = new OpeningCountdown(packet.State, _clock());
            foreach (var member in _rooms.GetMembers(sender.Id))
                if (member.PlayerId != sender.Id) SendOpening(member.PlayerId, sender.Id);
        }

        private void OnRoomEntered(PlayerRoomEnteredEvent e)
        {
            if (_disposed) return;
            // RoomService has already queued the ordered roster notifications for both directions.
            foreach (var member in _rooms.GetMembers(e.PlayerId))
            {
                if (member.PlayerId == e.PlayerId) continue;
                SendOpening(e.PlayerId, member.PlayerId);
                SendOpening(member.PlayerId, e.PlayerId);
            }
        }

        private void SendOpening(int recipientId, int playerId)
        {
            OpeningCountdown countdown;
            RoomPacketScope scope;
            if (!_openings.TryGetValue(playerId, out countdown)
                || !_rooms.TryGetScope(recipientId, playerId, out scope)) return;

            // Expiry remains a known zero. Receivers outside the gameplay scene also cache it.
            _network.Send(recipientId, new S2COpeningStatePacket
            {
                Scope = scope,
                PlayerId = playerId,
                State = countdown.GetState(_clock())
            }, NetworkChannels.Default, DeliveryMethod.ReliableOrdered);
        }

        private void OnStatusChanged(PlayerStatusChangedEvent e)
        {
            if (!e.Player.IsInGame) _openings.Remove(e.Player.Id);
        }

        public void Dispose()
        {
            if (_disposed) return;
            // The dispatcher has no unregister API; its retained callback becomes inert.
            _disposed = true;
            foreach (var subscription in _subscriptions) subscription.Dispose();
            _subscriptions.Clear();
            _openings.Clear();
        }
    }
}
