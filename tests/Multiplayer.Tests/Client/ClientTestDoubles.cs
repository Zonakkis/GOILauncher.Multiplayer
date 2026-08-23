using System;
using System.Collections.Generic;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Network;
using LiteNetLib;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Tests.Client
{
    /// <summary>
    /// Records what the client tried to put on the wire so packet handlers can be
    /// asserted on without a live socket.
    /// </summary>
    internal sealed class FakeNetworkClient : INetworkClient
    {
        public bool IsConnected { get; set; }
        public List<SentPacket> Sent { get; private set; }

        public FakeNetworkClient()
        {
            IsConnected = true;
            Sent = new List<SentPacket>();
        }

        public void Connect(string host, int port) { }
        public void Disconnect() { }
        public void Poll() { }
        public void Dispose() { }

        public void Send(INetSerializable packet, DeliveryMethod method)
        {
            Sent.Add(new SentPacket(packet, method));
        }
    }

    internal sealed class SentPacket
    {
        public INetSerializable Packet { get; private set; }
        public DeliveryMethod Method { get; private set; }

        public SentPacket(INetSerializable packet, DeliveryMethod method)
        {
            Packet = packet;
            Method = method;
        }
    }

    /// <summary>
    /// Captures the handlers a client module registers, so tests can feed S2C packets
    /// in without a live socket.
    /// </summary>
    internal sealed class RecordingClientDispatcher : IClientPacketDispatcher
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

        public void Receive<TPacket>(TPacket packet)
            where TPacket : INetSerializable
        {
            Delegate handler;
            if (!_handlers.TryGetValue(typeof(TPacket), out handler))
            {
                throw new InvalidOperationException(
                    "No handler registered for " + typeof(TPacket).Name);
            }

            // S2C packets carry no meaningful sender: the client only ever talks to one server.
            ((Action<TPacket, PacketSender>)handler)(packet, default(PacketSender));
        }
    }
}
