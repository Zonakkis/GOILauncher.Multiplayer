using System.Collections.Generic;
using System.Linq;
using Autofac;
using FluentAssertions;
using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Client.Synchronization;
using GOILauncher.Multiplayer.Core.Data.Constants;
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
    public class ClientSkinSyncTests
    {
        private const int RemoteId = 4;

        private RecordingClientDispatcher _dispatcher;
        private FakeNetworkClient _networkClient;
        private EventBus _eventBus;
        private ClientSkinSync _sync;
        private List<PlayerSkinReceivedEvent> _received;

        [SetUp]
        public void Setup()
        {
            _dispatcher = new RecordingClientDispatcher();
            _networkClient = new FakeNetworkClient();
            _eventBus = new EventBus(new Mock<ILogger<EventBus>>().Object);
            _sync = new ClientSkinSync(_networkClient, _dispatcher, _eventBus,
                new Mock<ILogger<ClientSkinSync>>().Object);
            ((IStartable)_sync).Start();

            _received = new List<PlayerSkinReceivedEvent>();
            _eventBus.Subscribe<PlayerSkinReceivedEvent>(e => _received.Add(e));
        }

        [Test]
        public void Announce_SendsTheManifestThenTheBytesOnTheSkinChannel()
        {
            var payload = SkinTestData.Png(0x01);

            _sync.Announce(SkinTestData.State(payload, 1f), payload);

            _networkClient.Sent.Select(s => s.Packet.GetType()).Should().Equal(
                typeof(C2SSkinManifestPacket), typeof(C2SSkinDataPacket));
            _networkClient.Sent.Should().OnlyContain(s =>
                s.Channel == NetworkChannels.Skin && s.Method == DeliveryMethod.ReliableOrdered);
            ((C2SSkinManifestPacket)_networkClient.Sent[0].Packet).State.Goldness.Should().Be(1f);
            ((C2SSkinDataPacket)_networkClient.Sent[1].Packet).Blob.Data.Should().Equal(payload);
        }

        /// <summary>
        /// No texture still means an announcement: goldness lives in the manifest, and switching
        /// back from a custom skin to vanilla is something everyone else has to hear about.
        /// </summary>
        [Test]
        public void Announce_WithoutATexture_SendsTheManifestAlone()
        {
            _sync.Announce(SkinTestData.Vanilla(1f), null);

            _networkClient.Sent.Select(s => s.Packet.GetType()).Should().Equal(
                typeof(C2SSkinManifestPacket));
            ((C2SSkinManifestPacket)_networkClient.Sent[0].Packet).State.Goldness.Should().Be(1f);
        }

        /// <summary>
        /// Announcing a hash the client cannot back with bytes would leave everyone requesting a
        /// texture that never arrives. It is a caller bug, so nothing goes on the wire.
        /// </summary>
        [Test]
        public void Announce_WithAHashButNoBytes_SendsNothing()
        {
            _sync.Announce(SkinTestData.State(SkinTestData.Png(0x01)), null);

            _networkClient.Sent.Should().BeEmpty();
        }

        [Test]
        public void ReAnnouncingTheSameSkin_ResendsTheManifestButNotTheBytes()
        {
            var payload = SkinTestData.Png(0x01);
            var state = SkinTestData.State(payload);

            _sync.Announce(state, payload);
            _sync.Announce(state, payload);

            _networkClient.Sent.Select(s => s.Packet.GetType()).Should().Equal(
                typeof(C2SSkinManifestPacket),
                typeof(C2SSkinDataPacket),
                typeof(C2SSkinManifestPacket));
        }

        /// <summary>
        /// The server keeps one blob per player, so announcing B evicts A there. Going back to A
        /// has to upload it again even though this client uploaded it once already.
        /// </summary>
        [Test]
        public void SwitchingAwayAndBack_UploadsTheBytesAgain()
        {
            var first = SkinTestData.Png(0x01);
            var second = SkinTestData.Png(0x02);

            _sync.Announce(SkinTestData.State(first), first);
            _sync.Announce(SkinTestData.State(second), second);
            _sync.Announce(SkinTestData.State(first), first);

            _networkClient.Sent.Count(s => s.Packet is C2SSkinDataPacket).Should().Be(3);
        }

        /// <summary>
        /// Vanilla clears the upload bookkeeping too: the server dropped the bytes when it saw the
        /// textureless manifest, so the next custom skin must be uploaded even if it is the same one.
        /// </summary>
        [Test]
        public void SwitchingThroughVanillaAndBack_UploadsTheBytesAgain()
        {
            var payload = SkinTestData.Png(0x01);

            _sync.Announce(SkinTestData.State(payload), payload);
            _sync.Announce(SkinTestData.Vanilla(0f), null);
            _sync.Announce(SkinTestData.State(payload), payload);

            _networkClient.Sent.Count(s => s.Packet is C2SSkinDataPacket).Should().Be(2);
        }

        /// <summary>
        /// The local read happens 0.1 s after entering the level, which can be well before the
        /// player connects to a server. The state is kept and announced once the handshake lands.
        /// </summary>
        [Test]
        public void AnnouncedWhileDisconnected_IsSentOnceTheLocalPlayerIsReady()
        {
            var payload = SkinTestData.Png(0x01);
            _networkClient.IsConnected = false;

            _sync.Announce(SkinTestData.State(payload), payload);
            _networkClient.Sent.Should().BeEmpty();

            _networkClient.IsConnected = true;
            _eventBus.Publish(new LocalPlayerReadyEvent(new PlayerInfo(1, "me", Platform.PC, true)));

            _networkClient.Sent.Select(s => s.Packet.GetType()).Should().Equal(
                typeof(C2SSkinManifestPacket), typeof(C2SSkinDataPacket));
        }

        [Test]
        public void LocalPlayerReady_WithNothingRead_SendsNothing()
        {
            _eventBus.Publish(new LocalPlayerReadyEvent(new PlayerInfo(1, "me", Platform.PC, true)));

            _networkClient.Sent.Should().BeEmpty();
        }

        [Test]
        public void UnknownManifest_RequestsTheBytesAndPublishesNothingYet()
        {
            var payload = SkinTestData.Png(0x01);

            ReceiveManifest(RemoteId, SkinTestData.State(payload));

            var request = (C2SSkinRequestPacket)_networkClient.Sent.Single().Packet;
            request.PlayerId.Should().Be(RemoteId);
            request.Hash.Matches(payload).Should().BeTrue();
            _networkClient.Sent.Single().Channel.Should().Be(NetworkChannels.Skin);
            _received.Should().BeEmpty();
        }

        [Test]
        public void VanillaManifest_PublishesImmediatelyWithNoBytes()
        {
            ReceiveManifest(RemoteId, SkinTestData.Vanilla(1f));

            _networkClient.Sent.Should().BeEmpty();
            var published = _received.Single();
            published.PlayerId.Should().Be(RemoteId);
            published.Payload.Should().BeNull();
            published.State.Goldness.Should().Be(1f);
        }

        [Test]
        public void ReceivedBytes_ArePublishedAndReadableThroughTryGetSkin()
        {
            var payload = SkinTestData.Png(0x01);
            ReceiveManifest(RemoteId, SkinTestData.State(payload, 1f));

            ReceiveData(RemoteId, payload);

            _received.Single().Payload.Should().Equal(payload);
            _received.Single().State.Goldness.Should().Be(1f);

            SkinState state;
            byte[] stored;
            _sync.TryGetSkin(RemoteId, out state, out stored).Should().BeTrue();
            stored.Should().Equal(payload);
        }

        /// <summary>
        /// A cached hash needs no round trip. This is what makes a player who keeps switching
        /// between two skins cheap after the first time.
        /// </summary>
        [Test]
        public void AlreadyCachedHash_IsPublishedWithoutARequest()
        {
            var payload = SkinTestData.Png(0x01);
            ReceiveManifest(RemoteId, SkinTestData.State(payload));
            ReceiveData(RemoteId, payload);
            _networkClient.Sent.Clear();
            _received.Clear();

            // Another player turns out to be wearing the same texture.
            ReceiveManifest(9, SkinTestData.State(payload));

            _networkClient.Sent.Should().BeEmpty();
            _received.Single().Payload.Should().Equal(payload);
        }

        /// <summary>
        /// The server relays bytes another client uploaded, and the server itself may be someone
        /// else's. Validation runs again on arrival.
        /// </summary>
        [Test]
        public void InvalidBytes_AreNotPublishedAndNotCached()
        {
            var payload = SkinTestData.Png(0x01);
            ReceiveManifest(RemoteId, SkinTestData.State(payload));
            _networkClient.Sent.Clear();

            // Right hash on the wire, wrong bytes behind it.
            _dispatcher.Receive(new S2CSkinDataPacket
            {
                PlayerId = RemoteId,
                Blob = new SkinBlob { Hash = SkinHash.Compute(payload), Data = SkinTestData.Png(0x02) }
            });

            _received.Should().BeEmpty();

            // Nothing was cached, so the same manifest still asks for the bytes.
            ReceiveManifest(RemoteId, SkinTestData.State(payload));
            _networkClient.Sent.Single().Packet.Should().BeOfType<C2SSkinRequestPacket>();
        }

        /// <summary>
        /// A player can change skins while their previous bytes are in flight. The late arrival
        /// must not repaint them with the texture they already replaced.
        /// </summary>
        [Test]
        public void BytesForAHashNoLongerAnnounced_AreNotPublished()
        {
            var stale = SkinTestData.Png(0x01);
            var current = SkinTestData.Png(0x02);
            ReceiveManifest(RemoteId, SkinTestData.State(stale));
            ReceiveManifest(RemoteId, SkinTestData.State(current));
            _received.Clear();

            ReceiveData(RemoteId, stale);

            _received.Should().BeEmpty();
        }

        [Test]
        public void Unavailable_FallsBackToVanillaWithoutRetrying()
        {
            var payload = SkinTestData.Png(0x01);
            ReceiveManifest(RemoteId, SkinTestData.State(payload, 1f));
            _networkClient.Sent.Clear();

            _dispatcher.Receive(new S2CSkinUnavailablePacket
            {
                PlayerId = RemoteId,
                Hash = SkinHash.Compute(payload)
            });

            var published = _received.Single();
            published.Payload.Should().BeNull();
            // Goldness survives the fallback: it never depended on the texture.
            published.State.Goldness.Should().Be(1f);
            _networkClient.Sent.Should().BeEmpty();
        }

        [Test]
        public void Unavailable_ForAStaleHash_IsIgnored()
        {
            var current = SkinTestData.Png(0x01);
            ReceiveManifest(RemoteId, SkinTestData.State(current));
            _received.Clear();

            _dispatcher.Receive(new S2CSkinUnavailablePacket
            {
                PlayerId = RemoteId,
                Hash = SkinHash.Compute(SkinTestData.Png(0x02))
            });

            _received.Should().BeEmpty();
        }

        [Test]
        public void TryGetSkin_IsFalseForAPlayerWithNoManifestYet()
        {
            SkinState state;
            byte[] payload;

            _sync.TryGetSkin(RemoteId, out state, out payload).Should().BeFalse();
        }

        /// <summary>
        /// Between the manifest and the bytes a player is known but not renderable yet. That has to
        /// read as "vanilla for now", not as "unknown player" — the instance is already on screen.
        /// </summary>
        [Test]
        public void TryGetSkin_IsTrueWithNoBytesWhileTheyAreStillInFlight()
        {
            ReceiveManifest(RemoteId, SkinTestData.State(SkinTestData.Png(0x01), 1f));

            SkinState state;
            byte[] payload;
            _sync.TryGetSkin(RemoteId, out state, out payload).Should().BeTrue();
            payload.Should().BeNull();
            state.Goldness.Should().Be(1f);
        }

        [Test]
        public void PlayerLeft_ForgetsTheirManifest()
        {
            var payload = SkinTestData.Png(0x01);
            ReceiveManifest(RemoteId, SkinTestData.State(payload));
            ReceiveData(RemoteId, payload);

            _eventBus.Publish(new PlayerLeftEvent(RemoteId, "remote", Platform.PC));

            SkinState state;
            byte[] stored;
            _sync.TryGetSkin(RemoteId, out state, out stored).Should().BeFalse();
            _sync.IsHashReferenced(SkinHash.Compute(payload)).Should().BeFalse();
        }

        /// <summary>
        /// Decoded textures are the expensive half (a 4096² RGBA32 is ~64 MB), and the Unity layer
        /// decides what to destroy by asking this. Bytes nobody references must stop being referenced.
        /// </summary>
        [Test]
        public void IsHashReferenced_FollowsWhatPlayersCurrentlyWear()
        {
            var first = SkinTestData.Png(0x01);
            var second = SkinTestData.Png(0x02);

            ReceiveManifest(RemoteId, SkinTestData.State(first));
            ReceiveData(RemoteId, first);
            _sync.IsHashReferenced(SkinHash.Compute(first)).Should().BeTrue();

            ReceiveManifest(RemoteId, SkinTestData.State(second));
            _sync.IsHashReferenced(SkinHash.Compute(first)).Should().BeFalse();
            _sync.IsHashReferenced(SkinHash.Compute(second)).Should().BeTrue();
        }

        /// <summary>
        /// Dropping a hash also drops its bytes, so wearing it again means downloading it again.
        /// The upload side is what makes that work: the announcer re-uploads on A→B→A.
        /// </summary>
        [Test]
        public void PrunedBytes_AreRequestedAgainWhenTheSkinComesBack()
        {
            var first = SkinTestData.Png(0x01);
            ReceiveManifest(RemoteId, SkinTestData.State(first));
            ReceiveData(RemoteId, first);
            ReceiveManifest(RemoteId, SkinTestData.State(SkinTestData.Png(0x02)));
            _networkClient.Sent.Clear();

            ReceiveManifest(RemoteId, SkinTestData.State(first));

            _networkClient.Sent.Single().Packet.Should().BeOfType<C2SSkinRequestPacket>();
        }

        /// <summary>
        /// Reconnecting means a different server, or the same one after a restart — its cache is
        /// gone either way. The local skin is kept (it did not change) but must be uploaded again.
        /// </summary>
        [Test]
        public void Disconnect_ClearsRemoteStateAndForcesAReUploadOnReconnect()
        {
            var localPayload = SkinTestData.Png(0x0A);
            var remotePayload = SkinTestData.Png(0x0B);
            _sync.Announce(SkinTestData.State(localPayload), localPayload);
            ReceiveManifest(RemoteId, SkinTestData.State(remotePayload));
            ReceiveData(RemoteId, remotePayload);

            _eventBus.Publish(new ServerDisconnectedEvent("closed"));

            SkinState state;
            byte[] stored;
            _sync.TryGetSkin(RemoteId, out state, out stored).Should().BeFalse();

            _networkClient.Sent.Clear();
            _eventBus.Publish(new LocalPlayerReadyEvent(new PlayerInfo(1, "me", Platform.PC, true)));

            _networkClient.Sent.Select(s => s.Packet.GetType()).Should().Equal(
                typeof(C2SSkinManifestPacket), typeof(C2SSkinDataPacket));
        }

        private void ReceiveManifest(int playerId, SkinState state)
        {
            _dispatcher.Receive(new S2CSkinManifestPacket { PlayerId = playerId, State = state });
        }

        private void ReceiveData(int playerId, byte[] payload)
        {
            _dispatcher.Receive(new S2CSkinDataPacket
            {
                PlayerId = playerId,
                Blob = SkinTestData.Blob(payload)
            });
        }
    }
}
