using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GOILauncher.Multiplayer.Core.Utils;

namespace GOILauncher.Multiplayer.Server.Services
{
    /// <summary>
    /// Per-room chat history for the observation facade: append messages, drop those past the
    /// retention window, and drop a room's history once that room is gone.
    ///
    /// Not thread-safe by design — <see cref="ObservationService"/> holds the only instance and calls
    /// every member under its own <c>_gate</c>, so the Poll-thread contract stays in one place instead
    /// of being duplicated here.
    /// </summary>
    internal sealed class ObservationChatLog
    {
        // Age window the dashboard shows; history is bounded by time only, no per-room count cap.
        // A mod server with a handful of players stays small over 24h.
        private static readonly TimeSpan Retention = TimeSpan.FromHours(24);

        private readonly Dictionary<int, List<ChatMessageObservation>> _byRoom
            = new Dictionary<int, List<ChatMessageObservation>>();

        public void Record(int roomId, ChatMessageObservation message)
        {
            List<ChatMessageObservation> history;
            if (!_byRoom.TryGetValue(roomId, out history))
                _byRoom[roomId] = history = new List<ChatMessageObservation>();
            history.Add(message);
        }

        /// <summary>Drops expired messages and rooms that no longer exist — chat dies with its room.</summary>
        public void Prune(DateTime now, ICollection<int> aliveRoomIds)
        {
            var cutoff = now.ToUnixTimeSeconds() - (long)Retention.TotalSeconds;
            var deadRooms = new List<int>();
            foreach (var kv in _byRoom)
            {
                kv.Value.RemoveAll(m => m.Timestamp < cutoff);
                if (!aliveRoomIds.Contains(kv.Key) || kv.Value.Count == 0) deadRooms.Add(kv.Key);
            }
            foreach (var id in deadRooms) _byRoom.Remove(id);
        }

        public IEnumerable<ChatMessageObservation> For(int roomId)
        {
            List<ChatMessageObservation> history;
            return _byRoom.TryGetValue(roomId, out history) ? history : Enumerable.Empty<ChatMessageObservation>();
        }

        /// <summary>Snapshot of every room's history except <paramref name="excludedRoomId"/>, which is surfaced separately.</summary>
        public Dictionary<int, ReadOnlyCollection<ChatMessageObservation>> ByRoomExcept(int excludedRoomId)
        {
            var result = new Dictionary<int, ReadOnlyCollection<ChatMessageObservation>>();
            foreach (var kv in _byRoom)
            {
                if (kv.Key == excludedRoomId) continue;
                result[kv.Key] = new ReadOnlyCollection<ChatMessageObservation>(kv.Value.ToList());
            }
            return result;
        }

        public void Clear() { _byRoom.Clear(); }
    }
}
