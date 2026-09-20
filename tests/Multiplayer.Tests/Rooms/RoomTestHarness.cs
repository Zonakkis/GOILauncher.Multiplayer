using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Tests.Client;
using GOILauncher.Multiplayer.Tests.Server;
using Moq;
using NUnit.Framework;
using C = GOILauncher.Multiplayer.Client.Services;
using S = GOILauncher.Multiplayer.Server.Services;
using CS = GOILauncher.Multiplayer.Client.Synchronization;
using SS = GOILauncher.Multiplayer.Server.Synchronization;

namespace GOILauncher.Multiplayer.Tests.Rooms
{
    internal sealed class RoomServerFixture
    {
        public readonly FakeNetworkServer Network = new FakeNetworkServer();
        public readonly RecordingServerDispatcher Dispatcher = new RecordingServerDispatcher();
        public readonly ServerEventBus Events = new ServerEventBus(new Mock<ILogger<EventBus>>().Object);
        public readonly S.PlayerService Players;
        public readonly S.RoomService Rooms;
        private uint _requestId;
        public RoomServerFixture()
        {
            Players = new S.PlayerService(Dispatcher, Events);
            Rooms = new S.RoomService(Players, Network, Dispatcher, Events, new Mock<ILogger<S.RoomService>>().Object);
            foreach (var module in new IStartable[] { Players, Rooms,
                new S.ChatService(Rooms, Players, Network, Dispatcher, Events), new SS.PlayerStateRelay(Network, Dispatcher, Players, Rooms),
                new SS.SkinRelay(Network, Dispatcher, Players, Rooms, Events, new Mock<ILogger<SS.SkinRelay>>().Object) }) module.Start();
        }
        public void Add(int id, bool isInGame = true)
        { Dispatcher.Receive(new C2SClientHandShakePacket { PlayerName = "p" + id, Platform = Platform.PC, IsInGame = isInGame }, id); }
        public S.RoomMembership Member(int id)
        { S.RoomMembership member; Assert.That(Rooms.TryGetMembership(id, out member), Is.True); return member; }
        public RoomInfo Room(int id) => Rooms.Rooms.Single(r => r.Id == id);
        public RoomOperationResult Execute(int id, RoomOperation operation, int roomId = 0, string name = "Room", int max = 0,
            string password = null, RoomPasswordChange passwordChange = RoomPasswordChange.Keep, ulong? membership = null)
        {
            Network.Sent.Clear();
            Dispatcher.Receive(new C2SRoomOperationPacket { RequestId = ++_requestId, MembershipId = membership ?? Member(id).Id,
                Operation = operation, RoomId = roomId, Name = name, MaxPlayers = max, Password = password, PasswordChange = passwordChange }, id);
            return Network.Sent.Where(s => s.ClientId == id).Select(s => s.Packet).OfType<S2CRoomOperationResultPacket>().Single().Result;
        }
        public int Create(int id, string name = "Room", int max = 0, string password = null)
        {
            Assert.That(Execute(id, RoomOperation.Create, name: name, max: max, password: password,
                passwordChange: password == null ? RoomPasswordChange.Remove : RoomPasswordChange.Set).Error, Is.EqualTo(RoomError.None));
            return Member(id).RoomId;
        }
        public void Join(int id, int roomId, string password = null)
        { Assert.That(Execute(id, RoomOperation.Join, roomId, password: password).Error, Is.EqualTo(RoomError.None)); }
    }
    internal sealed class RoomClientFixture
    {
        public readonly int Id;
        public readonly List<LiteNetLib.Utils.INetSerializable> SentHistory = new List<LiteNetLib.Utils.INetSerializable>();
        public readonly FakeNetworkClient Network = new FakeNetworkClient();
        public readonly RecordingClientDispatcher Dispatcher = new RecordingClientDispatcher();
        public readonly ClientEventBus Events = new ClientEventBus(new Mock<ILogger<EventBus>>().Object);
        public readonly C.PlayerService Players;
        public readonly C.RoomService Rooms;
        public readonly C.ChatService Chat;
        public readonly CS.ClientSkinSync Skin;
        public readonly CS.ClientPlayerStateSync State;
        public RoomClientFixture(int id)
        {
            Id = id;
            Players = new C.PlayerService(Network, Dispatcher, Events, new Mock<ILogger<C.PlayerService>>().Object);
            Rooms = new C.RoomService(Network, Dispatcher, Events, Players);
            Chat = new C.ChatService(Network, Dispatcher, Events, new Mock<ILogger<C.ChatService>>().Object, Players, Rooms);
            Skin = new CS.ClientSkinSync(Network, Dispatcher, Events, new Mock<ILogger<CS.ClientSkinSync>>().Object, Players);
            State = new CS.ClientPlayerStateSync(Network, Dispatcher, Events, Players);
            foreach (var module in new IStartable[] { Players, Rooms, Chat, Skin, State }) module.Start();
            Players.SetLocalPlayerInfo(new PlayerInfo(0, "p" + id, Platform.PC, true));
            Events.Publish(new ServerHandshakeEvent(id));
        }
    }
    internal sealed class RoomLink
    {
        public readonly RoomServerFixture Server = new RoomServerFixture();
        public readonly Dictionary<int, RoomClientFixture> Clients = new Dictionary<int, RoomClientFixture>();
        public RoomClientFixture Add(int id)
        {
            var client = new RoomClientFixture(id); Clients.Add(id, client); Pump(); return client;
        }
        public void Pump()
        {
            // Each queue is drained before dispatch, preserving reliable control-stream order.
            for (int turn = 0; turn < 100; turn++)
            {
                bool work = false;
                foreach (var client in Clients.Values)
                {
                    var outgoing = client.Network.Sent.ToArray(); client.Network.Sent.Clear();
                    work |= outgoing.Length != 0;
                    foreach (var sent in outgoing)
                    { client.SentHistory.Add(sent.Packet); Server.Dispatcher.ReceivePacket(sent.Packet, client.Id); }
                }
                var incoming = Server.Network.Sent.ToArray(); Server.Network.Sent.Clear();
                work |= incoming.Length != 0;
                foreach (var sent in incoming)
                {
                    RoomClientFixture client;
                    if (Clients.TryGetValue(sent.ClientId, out client)) client.Dispatcher.ReceivePacket(sent.Packet);
                }
                if (!work) return;
            }
            Assert.Fail("Room protocol did not quiesce.");
        }
    }
}
