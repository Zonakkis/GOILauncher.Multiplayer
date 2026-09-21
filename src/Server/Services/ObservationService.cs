using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autofac;
using GOILauncher.Multiplayer.Core.Data.Constants;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Core.Utils;
using GOILauncher.Multiplayer.Network;
using GOILauncher.Multiplayer.Server.Events;

namespace GOILauncher.Multiplayer.Server.Services
{
    /// <summary>
    /// Read-only window onto the running server for a console/dashboard host. It keeps its own
    /// display copies so a UI thread never enumerates PlayerService/RoomService (Poll-thread state).
    ///
    /// Threading contract: every mutation (event handlers + <see cref="MarkPoll"/>) runs on the
    /// server's Poll thread — LiteNetLib dispatches callbacks inside PollEvents, and the host calls
    /// MarkPoll from the same loop. <see cref="Snapshot"/> is the only cross-thread entry point, so
    /// it does no work on the live services; it just freezes the caches under <c>_gate</c>.
    /// Nothing here is authoritative — the server still never intervenes in player operations.
    /// </summary>
    public sealed class ObservationService : IObservationService, IStartable
    {
        private sealed class ConnState
        {
            public string EndPoint = string.Empty;
            public int? LatencyMilliseconds;
            public int NetworkErrorCount;
        }

        // Display budget. Tuned for a mod server with a handful of players, not scale.
        private const int RefreshIntervalMs = 150;
        private const int MaxChatMessagesPerRoom = 200;
        private static readonly TimeSpan ChatRetention = TimeSpan.FromMinutes(30);

        private readonly IPlayerService _players;
        private readonly IRoomService _rooms;
        private readonly INetworkServer _network;
        private readonly IServerEventBus _events;
        private readonly ILogger<ObservationService> _logger;
        private readonly ObservationClock _clock;

        private readonly object _gate = new object();
        private readonly Dictionary<int, ConnState> _connections = new Dictionary<int, ConnState>();
        private readonly Dictionary<int, List<ChatMessageObservation>> _chat = new Dictionary<int, List<ChatMessageObservation>>();

        private List<ConnectionObservation> _cachedConnections = new List<ConnectionObservation>();
        private List<RoomObservation> _cachedRooms = new List<RoomObservation>();
        private ServerTrafficObservation _cachedTraffic = ServerTrafficObservation.Empty;

        private long _pollCount;
        private int _unattributedErrors;
        private DateTime _lastPoll;
        private bool _hasLastPoll;
        private TimeSpan _maxPollGap;
        private DateTime _nextRefresh;
        private DateTime? _startedAt;

        public ObservationService(IPlayerService players, IRoomService rooms, INetworkServer network,
            IServerEventBus events, ILogger<ObservationService> logger)
            : this(players, rooms, network, events, logger, () => DateTime.Now)
        {
        }

        // Test seam: drive TTL/retention without sleeping. Autofac picks the 5-arg ctor since it
        // cannot resolve ObservationClock, so production always uses the wall clock.
        public ObservationService(IPlayerService players, IRoomService rooms, INetworkServer network,
            IServerEventBus events, ILogger<ObservationService> logger, ObservationClock clock)
        {
            _players = players; _rooms = rooms; _network = network; _events = events;
            _logger = logger; _clock = clock;
        }

        void IStartable.Start()
        {
            _events.Subscribe<ClientConnectedEvent>(OnConnected);
            _events.Subscribe<ClientDisconnectedEvent>(OnDisconnected);
            _events.Subscribe<ClientHandshakeEvent>(OnHandshake);
            _events.Subscribe<NetworkLatencyUpdatedEvent>(OnLatency);
            _events.Subscribe<NetworkErrorEvent>(OnNetworkError);
            _events.Subscribe<ChatRelayedEvent>(OnChat);
            _events.Subscribe<ServerStoppedEvent>(e => Reset());
        }

        public void MarkPoll()
        {
            var now = _clock();
            lock (_gate)
            {
                _pollCount++;
                if (_hasLastPoll)
                {
                    var gap = now - _lastPoll;
                    if (gap > _maxPollGap) _maxPollGap = gap;
                }
                _lastPoll = now; _hasLastPoll = true;
                if (_network.IsRunning && !_startedAt.HasValue) _startedAt = now;
                if (now >= _nextRefresh)
                {
                    RefreshCaches();
                    _nextRefresh = now.AddMilliseconds(RefreshIntervalMs);
                }
            }
        }

        public ServerObservationSnapshot Snapshot
        {
            get
            {
                var now = _clock();
                lock (_gate)
                {
                    PruneChat(now);
                    return new ServerObservationSnapshot(
                        _network.IsRunning,
                        _startedAt,
                        _startedAt.HasValue ? now - _startedAt.Value : (TimeSpan?)null,
                        _pollCount,
                        _maxPollGap,
                        _unattributedErrors,
                        _cachedTraffic,
                        new ReadOnlyCollection<ConnectionObservation>(_cachedConnections.ToList()),
                        new ReadOnlyCollection<RoomObservation>(_cachedRooms.ToList()),
                        new ReadOnlyCollection<ChatMessageObservation>(ChatFor(RoomConstants.LobbyId, now).ToList()),
                        BuildRoomChat(now));
                }
            }
        }

        private void RefreshCaches()
        {
            // Poll-thread only: enumerates live services and samples LiteNetLib counters.
            var traffic = _network.SampleServerTraffic();
            _cachedTraffic = new ServerTrafficObservation(
                traffic.PacketsSent, traffic.PacketsReceived, traffic.BytesSent, traffic.BytesReceived,
                traffic.PacketLoss, traffic.PacketLossPercent);

            var trafficById = new Dictionary<int, ConnectionStatsObservation>();
            foreach (var t in _network.SamplePeerTraffic())
                trafficById[t.ClientId] = new ConnectionStatsObservation(
                    t.PacketsSent, t.PacketsReceived, t.BytesSent, t.BytesReceived, t.PacketLoss, t.PacketLossPercent);

            // A connection can exist before its handshake (PlayerService entry), and vice versa for
            // a beat; union both so a half-open peer is still visible with whatever we know.
            var ids = new HashSet<int>(_connections.Keys);
            foreach (var id in _players.Players.Keys) ids.Add(id);

            var rows = new List<ConnectionObservation>(ids.Count);
            foreach (var id in ids.OrderBy(x => x))
            {
                ConnState state;
                _connections.TryGetValue(id, out state);
                PlayerInfo player;
                _players.Players.TryGetValue(id, out player);

                ConnectionStatsObservation stats;
                trafficById.TryGetValue(id, out stats);

                // 房间归属现读于 RoomService（本方法只在 Poll 线程跑），-1 = 尚未入房。
                RoomMembership membership;
                var roomId = _rooms.TryGetMembership(id, out membership) ? membership.RoomId : -1;

                rows.Add(new ConnectionObservation(
                    id,
                    player != null ? player.Name : null,
                    player != null ? player.Platform : default(Platform),
                    player != null && player.IsInGame,
                    roomId,
                    state != null ? state.EndPoint : string.Empty,
                    state != null ? state.LatencyMilliseconds : (int?)null,
                    stats,
                    state != null ? state.NetworkErrorCount : 0,
                    player != null));
            }
            _cachedConnections = rows;

            // RoomInfo carries OwnerPlayerId only; join the display name here for the console.
            var rooms = new List<RoomObservation>();
            foreach (var room in _rooms.Rooms)
            {
                string ownerName = null;
                if (room.OwnerPlayerId.HasValue)
                {
                    PlayerInfo owner;
                    if (_players.Players.TryGetValue(room.OwnerPlayerId.Value, out owner)) ownerName = owner.Name;
                }
                rooms.Add(new RoomObservation(room, ownerName));
            }
            _cachedRooms = rooms;
        }

        private void OnConnected(ClientConnectedEvent e)
        {
            lock (_gate)
            {
                _connections[e.ClientId] = new ConnState
                {
                    EndPoint = e.RemoteEndPoint != null ? e.RemoteEndPoint.ToString() : string.Empty
                };
            }
        }

        private void OnDisconnected(ClientDisconnectedEvent e)
        {
            lock (_gate) _connections.Remove(e.ClientId);
        }

        private void OnHandshake(ClientHandshakeEvent e)
        {
            // Ensure a row exists even if we missed the transport connect (ordering safety net).
            lock (_gate)
            {
                ConnState state;
                if (!_connections.TryGetValue(e.PlayerId, out state))
                    _connections[e.PlayerId] = new ConnState();
            }
        }

        private void OnLatency(NetworkLatencyUpdatedEvent e)
        {
            lock (_gate)
            {
                ConnState state;
                if (_connections.TryGetValue(e.ClientId, out state))
                    state.LatencyMilliseconds = e.LatencyMilliseconds;
            }
        }

        private void OnNetworkError(NetworkErrorEvent e)
        {
            lock (_gate)
            {
                // LiteNetLib reports socket errors by endpoint, not peer id; attribute where the
                // endpoint matches a live connection, otherwise it counts as an unattributed error.
                bool matched = false;
                foreach (var kv in _connections)
                {
                    if (kv.Value.EndPoint == e.EndPoint) { kv.Value.NetworkErrorCount++; matched = true; }
                }
                if (!matched)
                {
                    _unattributedErrors++;
                    _logger.Warn("Unattributed network error from {EndPoint}: {SocketError}.", e.EndPoint, e.SocketError);
                }
            }
        }

        private void OnChat(ChatRelayedEvent e)
        {
            lock (_gate)
            {
                List<ChatMessageObservation> history;
                if (!_chat.TryGetValue(e.RoomId, out history))
                    _chat[e.RoomId] = history = new List<ChatMessageObservation>();
                history.Add(new ChatMessageObservation(e.Timestamp, e.PlayerId, e.PlayerName, e.Content));
                if (history.Count > MaxChatMessagesPerRoom)
                    history.RemoveRange(0, history.Count - MaxChatMessagesPerRoom);
            }
        }

        private void Reset()
        {
            lock (_gate)
            {
                _connections.Clear(); _chat.Clear();
                _cachedConnections = new List<ConnectionObservation>();
                _cachedRooms = new List<RoomObservation>();
                _cachedTraffic = ServerTrafficObservation.Empty;
                _pollCount = 0; _unattributedErrors = 0;
                _hasLastPoll = false; _lastPoll = default(DateTime); _maxPollGap = TimeSpan.Zero;
                _nextRefresh = default(DateTime); _startedAt = null;
            }
        }

        // Drops expired messages and rooms that no longer exist — chat dies with its room.
        private void PruneChat(DateTime now)
        {
            var cutoff = now.ToUnixTimeSeconds() - (long)ChatRetention.TotalSeconds;
            var aliveRoomIds = new HashSet<int>(_cachedRooms.Select(r => r.Info.Id));
            var deadRooms = new List<int>();
            foreach (var kv in _chat)
            {
                kv.Value.RemoveAll(m => m.Timestamp < cutoff);
                if (!aliveRoomIds.Contains(kv.Key) || kv.Value.Count == 0) deadRooms.Add(kv.Key);
            }
            foreach (var id in deadRooms) _chat.Remove(id);
        }

        private IEnumerable<ChatMessageObservation> ChatFor(int roomId, DateTime now)
        {
            List<ChatMessageObservation> history;
            return _chat.TryGetValue(roomId, out history) ? history : Enumerable.Empty<ChatMessageObservation>();
        }

        private Dictionary<int, ReadOnlyCollection<ChatMessageObservation>> BuildRoomChat(DateTime now)
        {
            var result = new Dictionary<int, ReadOnlyCollection<ChatMessageObservation>>();
            foreach (var kv in _chat)
            {
                if (kv.Key == RoomConstants.LobbyId) continue; // surfaced as LobbyChat instead
                result[kv.Key] = new ReadOnlyCollection<ChatMessageObservation>(kv.Value.ToList());
            }
            return result;
        }
    }
}
