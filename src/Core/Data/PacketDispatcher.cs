using System;
using GOILauncher.Multiplayer.Core.Log;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data
{
    public abstract class PacketDispatcher : IPacketDispatcher
    {
        private readonly NetPacketProcessor _processor;
        private readonly ILogger<PacketDispatcher> _logger;

        protected PacketDispatcher(NetPacketProcessor processor, ILogger<PacketDispatcher> logger)
        {
            _processor = processor;
            _logger = logger;
        }

        public void RegisterStruct<TPacket>(Action<TPacket, PacketSender> onReceive)
             where TPacket : struct, INetSerializable
        {
            _processor.SubscribeNetSerializable(onReceive);
        }

        public void RegisterClass<TPacket>(Action<TPacket, PacketSender> onReceive)
             where TPacket : class, INetSerializable, new()
        {
            _processor.SubscribeNetSerializable(onReceive);
        }

        public void Dispatch(PacketSender sender, NetDataReader reader)
        {
            // Boxed once per datagram rather than once per packet: NetPacketProcessor's
            // user-data channel is typed as object.
            object boxedSender = sender;
            try
            {
                _processor.ReadAllPackets(reader, boxedSender);
            }
            catch (ParseException ex)
            {
                // A packet belonging to the other role, or a malformed one, must not
                // abort the whole PollEvents pass.
                if (_logger != null)
                    _logger.Warn("Discarded unhandled packet from peer {PeerId}: {Reason}", sender.Id, ex.Message);
            }
        }
    }

    public sealed class ClientPacketDispatcher : PacketDispatcher, IClientPacketDispatcher
    {
        public ClientPacketDispatcher(NetPacketProcessor processor, ILogger<PacketDispatcher> logger)
            : base(processor, logger)
        {
        }
    }

    public sealed class ServerPacketDispatcher : PacketDispatcher, IServerPacketDispatcher
    {
        public ServerPacketDispatcher(NetPacketProcessor processor, ILogger<PacketDispatcher> logger)
            : base(processor, logger)
        {
        }
    }
}
