using System;
using System.Collections.Generic;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Data.Serialization;
using GOILauncher.Multiplayer.Core.Log;

namespace GOILauncher.Multiplayer.Core.Data.Handlers
{
    public class PacketDispatcher : IPacketDispatcher
    {
        private readonly ArraySegmentReader _reader;
        private readonly ILogger<PacketDispatcher> _logger;
        private readonly Dictionary<PacketType, Action<ArraySegment<byte>>> _routes
            = new Dictionary<PacketType, Action<ArraySegment<byte>>>();

        public PacketDispatcher(ArraySegmentReader reader, ILogger<PacketDispatcher> logger)
        {
            _reader = reader;
            _logger = logger;
        }

        public void RegisterSerializer<TPacket>(
            PacketType type, IPacketSerializer<TPacket> serializer, IPacketHandler<TPacket> handler)
            where TPacket : IPacket
        {
            _routes[type] = (data) =>
            {
                var packet = serializer.Deserialize(data);
                handler.Handle(packet);
            };
        }

        public void Dispatch(ArraySegment<byte> data)
        {
            _reader.Begin(data);
            PacketType type = (PacketType)_reader.ReadByte();
            _reader.End();
            if (_routes.TryGetValue(type, out Action<ArraySegment<byte>> route))
            {
                route(data);
                return;
            }
            _logger.Warn($"No handler found for packet type {type}");
        }
    }
}
