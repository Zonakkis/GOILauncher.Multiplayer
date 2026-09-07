using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Autofac;
using FluentAssertions;
using GOILauncher.Multiplayer.Client.Extensions;
using GOILauncher.Multiplayer.Client.Models;
using GOILauncher.Multiplayer.Client.Synchronization;
using GOILauncher.Multiplayer.Core.Data.Constants;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Extensions;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Core.Network;
using GOILauncher.Multiplayer.Network;
using GOILauncher.Multiplayer.Server.Extensions;
using LiteNetLib;
using NUnit.Framework;
using C = GOILauncher.Multiplayer.Client.Services;
using S = GOILauncher.Multiplayer.Server.Services;

namespace GOILauncher.Multiplayer.Tests.Rooms
{
    [TestFixture, NonParallelizable]
    public class RoomTransportTests
    {
        private struct Probe { }
        private static IContainer BuildHost(out NetManager socket)
        {
            var builder = new ContainerBuilder();
            builder.RegisterMultiplayerCore().WithServer().WithClient();
            NetManager created = null;
            // Keep the actual socket only to discover its OS-assigned loopback port.
            builder.Register(ctx =>
            {
                created = new NetManager(ctx.Resolve<NetworkServerListener>()) { ChannelsCount = NetworkChannels.Count };
                return new NetworkServer(created, ctx.Resolve<IServerEventBus>(), ctx.Resolve<ILogger<NetworkServer>>());
            }).As<INetworkServer>().SingleInstance();
            var container = builder.Build(); socket = created; return container;
        }
        private static void WaitUntil(Func<bool> condition, Action poll)
        {
            var timer = Stopwatch.StartNew();
            while (!condition() && timer.Elapsed < TimeSpan.FromSeconds(5)) { poll(); Thread.Sleep(5); }
            Assert.That(condition(), Is.True, "Loopback room protocol timed out.");
        }
        [Test]
        public void MixedRoleComposition_UsesSeparateEventBusesAndFullyActivatedRoomModules()
        {
            NetManager socket;
            using (var host = BuildHost(out socket))
            {
                var clientBus = host.Resolve<IClientEventBus>(); var serverBus = host.Resolve<IServerEventBus>();
                clientBus.Should().NotBeSameAs(serverBus);
                int clientSeen = 0, serverSeen = 0;
                clientBus.Subscribe<Probe>(e => clientSeen++); serverBus.Subscribe<Probe>(e => serverSeen++);
                serverBus.Publish(new Probe()); clientSeen.Should().Be(0); serverSeen.Should().Be(1);
                clientBus.Publish(new Probe()); clientSeen.Should().Be(1); serverSeen.Should().Be(1);
                host.Resolve<C.IRoomService>().Should().NotBeNull(); host.Resolve<S.IRoomService>().Should().NotBeNull();
            }
        }
        [Test]
        public void RealUdp_HandshakePasswordJoinChatAndSkin_WorkWithAnEmbeddedHost()
        {
            NetManager socket;
            using (var host = BuildHost(out socket))
            {
                var builder = new ContainerBuilder(); builder.RegisterMultiplayerCore().WithClient();
                using (var guest = builder.Build())
                {
                    var server = host.Resolve<INetworkServer>(); server.Start(0);
                    var first = host.Resolve<C.IClientService>(); var second = guest.Resolve<C.IClientService>();
                    host.Resolve<C.IPlayerService>().SetLocalPlayerInfo(new PlayerInfo(0, "host", Platform.PC, true));
                    guest.Resolve<C.IPlayerService>().SetLocalPlayerInfo(new PlayerInfo(0, "guest", Platform.PC, true));
                    var ownRooms = host.Resolve<C.IRoomService>(); var guestRooms = guest.Resolve<C.IRoomService>();
                    Action poll = () => { server.Poll(); first.Poll(); second.Poll(); };
                    first.Connect("127.0.0.1", socket.LocalPort); second.Connect("127.0.0.1", socket.LocalPort);
                    WaitUntil(() => ownRooms.CurrentRoom != null && guestRooms.CurrentRoom != null, poll);
                    ownRooms.CurrentRoom.Id.Should().Be(0); guestRooms.CurrentRoom.Id.Should().Be(0);
                    ownRooms.CreateRoom("wire room", "password", 2);
                    WaitUntil(() => ownRooms.CurrentRoom.Id != 0 && !ownRooms.IsOperationPending, poll);
                    int id = ownRooms.CurrentRoom.Id;
                    guestRooms.JoinRoom(id, "password");
                    WaitUntil(() => guestRooms.CurrentRoom.Id == id && !guestRooms.IsOperationPending, poll);
                    host.Resolve<C.IChatService>().SendMessage(MessageType.Player, "room-only wire message");
                    WaitUntil(() => guest.Resolve<C.IChatService>().Messages.Any(m => m.Content == "room-only wire message"), poll);
                    var payload = SkinTestData.Png(1);
                    host.Resolve<ClientSkinSync>().Announce(SkinTestData.State(payload), payload);
                    int ownerId = host.Resolve<C.IPlayerService>().LocalPlayer.Id;
                    WaitUntil(() =>
                    {
                        SkinState state; byte[] bytes;
                        return guest.Resolve<ClientSkinSync>().TryGetSkin(ownerId, out state, out bytes) && bytes != null;
                    }, poll);
                    int stops = 0; host.Resolve<IServerEventBus>().Subscribe<ServerStoppedEvent>(e => stops++);
                    server.Stop(); server.Stop();
                    stops.Should().Be(1); host.Resolve<S.IPlayerService>().Players.Should().BeEmpty();
                    host.Resolve<S.IRoomService>().Rooms.Should().ContainSingle(r => r.IsLobby && r.PlayerCount == 0);
                }
            }
        }
        [Test]
        public void LegacyConnectionKey_IsRejectedBeforeAnyPlayerOrRoomMembershipIsCreated()
        {
            NetManager socket;
            using (var host = BuildHost(out socket))
            {
                var server = host.Resolve<INetworkServer>(); server.Start(0);
                var listener = new EventBasedNetListener();
                DisconnectReason? reason = null;
                listener.PeerDisconnectedEvent += (peer, info) => reason = info.Reason;
                var legacy = new NetManager(listener);
                try
                {
                    legacy.Start(); legacy.Connect("127.0.0.1", socket.LocalPort, "GOILauncher");
                    WaitUntil(() => reason.HasValue, () => { server.Poll(); legacy.PollEvents(); });
                    reason.Should().Be(DisconnectReason.ConnectionRejected);
                    host.Resolve<S.IPlayerService>().Players.Should().BeEmpty();
                    host.Resolve<S.IRoomService>().Rooms.Single().PlayerCount.Should().Be(0);
                }
                finally { legacy.Stop(); }
            }
        }
    }
}
