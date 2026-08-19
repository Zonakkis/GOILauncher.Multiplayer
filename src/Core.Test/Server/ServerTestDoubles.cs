using System;
using System.Collections.Generic;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Network;
using LiteNetLib;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Test.Server
{
    /// <summary>
    /// Records what the server tried to put on the wire so packet handlers can be
    /// asserted on without a live socket.
    /// </summary>
    internal sealed class FakeNetworkServer : INetworkServer
    {
        public List<SentPacket> Sent { get; private set; }

        public FakeNetworkServer()
        {
            Sent = new List<SentPacket>();
        }

        public bool IsRunning { get; set; }

        public void Start(int port) { IsRunning = true; }
        public void Stop() { IsRunning = false; }
        public void Poll() { }
        public void Dispose() { }

        public void Send(int clientId, INetSerializable packet, DeliveryMethod method)
        {
            Sent.Add(new SentPacket(clientId, packet, method));
        }

        public void Multicast(IEnumerable<int> clientIds, INetSerializable packet, DeliveryMethod method)
        {
            foreach (var clientId in clientIds)
            {
                Send(clientId, packet, method);
            }
        }

        public void Broadcast(INetSerializable packet, DeliveryMethod method)
        {
            throw new NotSupportedException("Broadcast is not exercised by these tests.");
        }
    }

    internal sealed class SentPacket
    {
        public int ClientId { get; private set; }
        public INetSerializable Packet { get; private set; }
        public DeliveryMethod Method { get; private set; }

        public SentPacket(int clientId, INetSerializable packet, DeliveryMethod method)
        {
            ClientId = clientId;
            Packet = packet;
            Method = method;
        }
    }

    /// <summary>
    /// Captures the handlers a server module registers, so tests can feed packets in
    /// with an arbitrary <see cref="PacketSender"/>.
    /// </summary>
    internal sealed class RecordingServerDispatcher : IServerPacketDispatcher
    {
        private readonly Dictionary<Type, Delegate> _handlers = new Dictionary<Type, Delegate>();

        public void RegisterStruct<TPacket>(Action<TPacket, PacketSender> onReceive)
            where TPacket : struct, INetSerializable
        {
            _handlers[typeof(TPacket)] = onReceive;
        }

        public void RegisterClass<TPacket>(Action<TPacket, PacketSender> onReceive)
            where TPacket : class, INetSerializable, new()
        {
            _handlers[typeof(TPacket)] = onReceive;
        }

        public void Dispatch(PacketSender sender, NetDataReader reader)
        {
        }

        public void Receive<TPacket>(TPacket packet, int senderId)
            where TPacket : INetSerializable
        {
            Delegate handler;
            if (!_handlers.TryGetValue(typeof(TPacket), out handler))
            {
                throw new InvalidOperationException(
                    "No handler registered for " + typeof(TPacket).Name);
            }

            ((Action<TPacket, PacketSender>)handler)(packet, new PacketSender(senderId));
        }

        public bool HasHandlerFor<TPacket>()
        {
            return _handlers.ContainsKey(typeof(TPacket));
        }
    }
}
