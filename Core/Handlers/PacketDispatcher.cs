using Autofac;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Packets;
using System;
using System.Collections.Generic;

namespace GOILauncher.Multiplayer.Core.Handlers
{
    public class PacketDispatcher : IPacketDispatcher
    {
        private readonly Dictionary<PacketType, IPacketHandler> _handlers;
        private readonly ArraySegmentReader _reader;

        public PacketDispatcher(IEnumerable<IPacketHandler> handlers, ArraySegmentReader reader)
        {
            _handlers = new Dictionary<PacketType, IPacketHandler>();
            foreach (var handler in handlers)
            {
                _handlers[handler.Type] = handler;
            }

            _reader = reader;
        }

        public void Dispatch(ArraySegment<byte> data)
        {
            _reader.Begin(data);
            PacketType type = (PacketType)_reader.ReadByte();
            _reader.End();
            if (_handlers.TryGetValue(type, out IPacketHandler handler))
            {
                handler.Handle(new ArraySegment<byte>(data.Array, data.Offset + 1, data.Count - 1));
            }
            else
            {
                throw new InvalidOperationException($"No handler found for packet type {type}");
            }
        }
    }
}
