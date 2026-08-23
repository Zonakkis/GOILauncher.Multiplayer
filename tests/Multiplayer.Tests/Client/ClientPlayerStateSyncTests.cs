using Autofac;
using FluentAssertions;
using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Client.Synchronization;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using LiteNetLib;
using Moq;
using NUnit.Framework;

namespace GOILauncher.Multiplayer.Tests.Client
{
    [TestFixture]
    public class ClientPlayerStateSyncTests
    {
        private RecordingClientDispatcher _dispatcher;
        private FakeNetworkClient _networkClient;
        private EventBus _eventBus;
        private ClientPlayerStateSync _sync;

        [SetUp]
        public void Setup()
        {
            _dispatcher = new RecordingClientDispatcher();
            _networkClient = new FakeNetworkClient();
            _eventBus = new EventBus(new Mock<ILogger<EventBus>>().Object);
            _sync = new ClientPlayerStateSync(_networkClient, _dispatcher, _eventBus);
            ((IStartable)_sync).Start();
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
    }
}
