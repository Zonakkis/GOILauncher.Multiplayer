using System;
using System.Collections.Generic;
using FluentAssertions;
using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Client.Synchronization;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Network;
using LiteNetLib;
using LiteNetLib.Utils;
using Moq;
using NUnit.Framework;

namespace GOILauncher.Multiplayer.Core.Test.Client
{
    [TestFixture]
    public class ClientPlayerStateSyncTests
    {
        private CapturingDispatcher _dispatcher;
        private FakeNetworkClient _networkClient;
        private EventBus _eventBus;
        private ClientPlayerStateSync _sync;

        [SetUp]
        public void Setup()
        {
            _dispatcher = new CapturingDispatcher();
            _networkClient = new FakeNetworkClient();
            _eventBus = new EventBus(new Mock<ILogger<EventBus>>().Object);
            _sync = new ClientPlayerStateSync(_networkClient, _dispatcher, _eventBus);
        }

        [Test]
        public void Send_UsesUnreliableDeliveryAndPersistentSequence()
        {
            _sync.Send(default(PlayerState));
            _sync.Send(default(PlayerState));

            _networkClient.Sent.Count.Should().Be(2);
            _networkClient.Sent[0].Method.Should().Be(DeliveryMethod.Unreliable);
            ((C2SPlayerStatePacket)_networkClient.Sent[0].Packet).Sequence.Should().Be(0u);
            ((C2SPlayerStatePacket)_networkClient.Sent[1].Packet).Sequence.Should().Be(1u);
        }

        [Test]
        public void Receive_DropsStaleStatePerPlayer()
        {
            var received = 0;
            _eventBus.Subscribe<PlayerStateReceivedEvent>(e => received++);

            _dispatcher.Receive(new S2CPlayerStatePacket { PlayerId = 4, Sequence = 10 });
            _dispatcher.Receive(new S2CPlayerStatePacket { PlayerId = 4, Sequence = 9 });
            _dispatcher.Receive(new S2CPlayerStatePacket { PlayerId = 4, Sequence = 11 });

            received.Should().Be(2);
        }

        [Test]
        public void PlayerLeft_ClearsSequenceSoReentryCanStartAtAnySequence()
        {
            var received = 0;
            _eventBus.Subscribe<PlayerStateReceivedEvent>(e => received++);

            _dispatcher.Receive(new S2CPlayerStatePacket { PlayerId = 4, Sequence = 10 });
            _eventBus.Publish(new PlayerLeftEvent(4, "remote", Platform.PC));
            _dispatcher.Receive(new S2CPlayerStatePacket { PlayerId = 4, Sequence = 0 });

            received.Should().Be(2);
        }

        private sealed class CapturingDispatcher : IPacketDispatcher
        {
            private Action<S2CPlayerStatePacket, NetPeer> _handler;

            public void RegisterStruct<TPacket>(Action<TPacket, NetPeer> onReceive)
                where TPacket : struct, INetSerializable
            {
                if (typeof(TPacket) == typeof(S2CPlayerStatePacket))
                {
                    _handler = (Action<S2CPlayerStatePacket, NetPeer>)(object)onReceive;
                }
            }

            public void RegisterClass<TPacket>(Action<TPacket, NetPeer> onReceive)
                where TPacket : class, INetSerializable, new()
            {
            }

            public void Dispatch(NetPeer peer, NetDataReader reader)
            {
            }

            public void Receive(S2CPlayerStatePacket packet)
            {
                _handler(packet, null);
            }
        }

        private sealed class FakeNetworkClient : INetworkClient
        {
            public bool IsConnected { get; set; } = true;
            public List<SentPacket> Sent { get; } = new List<SentPacket>();

            public void Connect(string host, int port) { }
            public void Disconnect() { }
            public void Poll() { }
            public void Dispose() { }

            public void Send(INetSerializable packet, DeliveryMethod method)
            {
                Sent.Add(new SentPacket(packet, method));
            }
        }

        private sealed class SentPacket
        {
            public INetSerializable Packet { get; private set; }
            public DeliveryMethod Method { get; private set; }

            public SentPacket(INetSerializable packet, DeliveryMethod method)
            {
                Packet = packet;
                Method = method;
            }
        }
    }
}
