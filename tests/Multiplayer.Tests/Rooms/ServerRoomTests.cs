using System.Linq;
using FluentAssertions;
using GOILauncher.Multiplayer.Core.Data.Constants;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using LiteNetLib;
using NUnit.Framework;

namespace GOILauncher.Multiplayer.Tests.Rooms
{
    [TestFixture]
    public class ServerRoomTests
    {
        private RoomServerFixture _server;
        [SetUp] public void Setup() { _server = new RoomServerFixture(); }
        [Test]
        public void Handshake_EntersPermanentUnlimitedOwnerlessLobby()
        {
            _server.Add(1, false);
            _server.Member(1).RoomId.Should().Be(RoomConstants.LobbyId);
            var lobby = _server.Room(0);
            lobby.Name.Should().Be("大厅"); lobby.PlayerCount.Should().Be(1);
            lobby.OwnerPlayerId.Should().BeNull(); lobby.MaxPlayers.Should().Be(0); lobby.HasPassword.Should().BeFalse();
            _server.Execute(1, RoomOperation.Leave).IsSuccess.Should().BeTrue();
            _server.Execute(1, RoomOperation.Update, 0).Error.Should().Be(RoomError.Forbidden);
            _server.Events.Publish(new ClientDisconnectedEvent(1, "gone"));
            _server.Room(0).PlayerCount.Should().Be(0);
        }
        [Test]
        public void Create_EntersRoomAndNamesMayRepeatButIdsDoNot()
        {
            _server.Add(1); _server.Add(2);
            int first = _server.Create(1, "same"); int second = _server.Create(2, "same");
            first.Should().NotBe(second); _server.Room(first).OwnerPlayerId.Should().Be(1);
            _server.Room(first).PlayerCount.Should().Be(1); _server.Room(0).PlayerCount.Should().Be(0);
            _server.Execute(1, RoomOperation.Leave);
            _server.Rooms.Rooms.Should().NotContain(r => r.Id == first);
            _server.Create(1, "same").Should().BeGreaterThan(second);
        }
        [TestCase("", 0, RoomError.InvalidName)]
        [TestCase("   ", 0, RoomError.InvalidName)]
        [TestCase("valid", -1, RoomError.InvalidCapacity)]
        public void FailedCreation_LeavesOriginalMembershipAndDirectoryUntouched(string name, int max, RoomError error)
        {
            _server.Add(1); var room = _server.Create(1); var membership = _server.Member(1).Id;
            _server.Execute(1, RoomOperation.Create, name: name, max: max).Error.Should().Be(error);
            _server.Member(1).Id.Should().Be(membership); _server.Rooms.Rooms.Should().HaveCount(2);
            _server.Room(room).OwnerPlayerId.Should().Be(1);
            _server.Network.Sent.Should().ContainSingle(s => s.Packet is S2CRoomOperationResultPacket);
        }
        [Test]
        public void FailedJoins_DoNotEvictPlayerOrTransferTheirOwnership()
        {
            _server.Add(1); _server.Add(2); _server.Add(3);
            int own = _server.Create(1); int locked = _server.Create(2, password: "secret");
            var membership = _server.Member(1).Id;
            _server.Execute(1, RoomOperation.Join, locked, password: "wrong").Error.Should().Be(RoomError.IncorrectPassword);
            _server.Execute(1, RoomOperation.Join, 999).Error.Should().Be(RoomError.RoomNotFound);
            _server.Member(1).Id.Should().Be(membership); _server.Room(own).OwnerPlayerId.Should().Be(1);
        }
        [Test]
        public void LastSlot_IsGrantedOnce_AndMainMenuMembersCount()
        {
            _server.Add(1); _server.Add(2, false); _server.Add(3);
            int room = _server.Create(1, max: 2);
            _server.Join(2, room);
            _server.Execute(3, RoomOperation.Join, room).Error.Should().Be(RoomError.RoomFull);
            _server.Room(room).PlayerCount.Should().Be(2); _server.Member(3).RoomId.Should().Be(0);
        }
        [Test]
        public void OwnerTransfer_UsesRoomJoinOrder_NotPlayerId_AndSceneExitDoesNotLeave()
        {
            _server.Add(9); _server.Add(8); _server.Add(1);
            int room = _server.Create(9); _server.Join(8, room); _server.Join(1, room);
            _server.Dispatcher.Receive(new C2SIsInGameUpdatePacket(false), 9);
            _server.Room(room).OwnerPlayerId.Should().Be(9); _server.Room(room).PlayerCount.Should().Be(3);
            _server.Execute(9, RoomOperation.Leave);
            _server.Room(room).OwnerPlayerId.Should().Be(8);
            _server.Events.Publish(new ClientDisconnectedEvent(8, "lost"));
            _server.Room(room).OwnerPlayerId.Should().Be(1);
            _server.Events.Publish(new ClientDisconnectedEvent(1, "lost"));
            _server.Rooms.Rooms.Should().NotContain(r => r.Id == room);
        }
        [Test]
        public void LeavingAndRejoining_GetsNewMembershipAndMovesToEndOfOwnerQueue()
        {
            _server.Add(1); _server.Add(2); _server.Add(3);
            int room = _server.Create(1); _server.Join(2, room); _server.Join(3, room);
            var old = _server.Member(2).Id;
            _server.Execute(2, RoomOperation.Leave); _server.Join(2, room);
            _server.Member(2).Id.Should().BeGreaterThan(old);
            _server.Execute(1, RoomOperation.Leave);
            _server.Room(room).OwnerPlayerId.Should().Be(3);
        }
        [Test]
        public void RepeatedJoinAndHandshake_DoNotResetMembershipOrOwner()
        {
            _server.Add(1); int room = _server.Create(1, max: 1, password: "secret");
            var member = _server.Member(1).Id;
            _server.Execute(1, RoomOperation.Join, room).IsSuccess.Should().BeTrue();
            _server.Network.Sent.Should().NotContain(s => s.Packet is S2CPlayerListPacket);
            _server.Add(1);
            _server.Member(1).Id.Should().Be(member); _server.Room(room).OwnerPlayerId.Should().Be(1);
        }
        [Test]
        public void Editing_RequiresCurrentOwner_AndDoesNotAllowCapacityBelowOccupancy()
        {
            _server.Add(1); _server.Add(2); int room = _server.Create(1); _server.Join(2, room);
            _server.Execute(2, RoomOperation.Update, room, "changed").Error.Should().Be(RoomError.NotOwner);
            _server.Execute(1, RoomOperation.Update, room, "changed", max: 1).Error.Should().Be(RoomError.InvalidCapacity);
            _server.Room(room).Name.Should().Be("Room"); _server.Room(room).MaxPlayers.Should().Be(0);
            _server.Execute(1, RoomOperation.Update, room, "changed", max: 2).IsSuccess.Should().BeTrue();
            _server.Room(room).Name.Should().Be("changed"); _server.Room(room).MaxPlayers.Should().Be(2);
            _server.Execute(1, RoomOperation.Update, room, max: 0).IsSuccess.Should().BeTrue();
        }
        [Test]
        public void Password_KeepReplaceRemove_OnlyAffectsFutureAdmissions()
        {
            _server.Add(1); _server.Add(2); _server.Add(3);
            int room = _server.Create(1, password: "old"); _server.Join(2, room, "old");
            var member = _server.Member(2).Id;
            _server.Execute(1, RoomOperation.Update, room, passwordChange: RoomPasswordChange.Keep).IsSuccess.Should().BeTrue();
            _server.Join(3, room, "old"); _server.Execute(3, RoomOperation.Leave);
            _server.Execute(1, RoomOperation.Update, room, password: "new", passwordChange: RoomPasswordChange.Set).IsSuccess.Should().BeTrue();
            _server.Member(2).Id.Should().Be(member);
            _server.Execute(3, RoomOperation.Join, room, password: "old").Error.Should().Be(RoomError.IncorrectPassword);
            _server.Join(3, room, "new"); _server.Execute(3, RoomOperation.Leave);
            _server.Execute(1, RoomOperation.Update, room, passwordChange: RoomPasswordChange.Remove).IsSuccess.Should().BeTrue();
            _server.Room(room).HasPassword.Should().BeFalse(); _server.Join(3, room);
        }
        [Test]
        public void StaleAndUnauthenticatedOperations_AreRejected()
        {
            _server.Add(1); var old = _server.Member(1).Id; _server.Create(1);
            _server.Execute(1, RoomOperation.Create, membership: old).Error.Should().Be(RoomError.StaleMembership);
            _server.Execute(99, RoomOperation.Create, membership: 0).Error.Should().Be(RoomError.NotReady);
            _server.Rooms.Rooms.Should().HaveCount(2);
        }
        [Test]
        public void DirectoryIsGlobal_ButRostersChatAndStateAreRoomLocal()
        {
            _server.Add(1); _server.Add(2); _server.Add(3); int room = _server.Create(1); _server.Join(2, room);
            _server.Network.Sent.Where(s => s.Packet is S2CRoomListPacket).Select(s => s.ClientId).Should().BeEquivalentTo(new[] { 1, 2, 3 });
            _server.Network.Sent.Where(s => s.Packet is S2CPlayerJoinedPacket).Select(s => s.ClientId).Should().Equal(1);
            _server.Network.Sent.Clear();
            _server.Dispatcher.Receive(new C2SChatMessagePacket("hi") { MembershipId = _server.Member(1).Id }, 1);
            _server.Dispatcher.Receive(new C2SPlayerStatePacket { MembershipId = _server.Member(1).Id, Sequence = 1 }, 1);
            _server.Dispatcher.Receive(new C2SIsInGameUpdatePacket(false), 1);
            _server.Network.Sent.Should().HaveCount(3).And.OnlyContain(s => s.ClientId == 2);
            _server.Network.Sent.Single(s => s.Packet is S2CPlayerStatePacket).Method.Should().Be(DeliveryMethod.Unreliable);
        }
        [Test]
        public void OldRoomOutgoingStateAndChat_CannotBeForwardedIntoNewRoom()
        {
            _server.Add(1); _server.Add(2); ulong old = _server.Member(1).Id;
            int room = _server.Create(1); _server.Join(2, room); _server.Network.Sent.Clear();
            _server.Dispatcher.Receive(new C2SChatMessagePacket("old room") { MembershipId = old }, 1);
            _server.Dispatcher.Receive(new C2SPlayerStatePacket { MembershipId = old }, 1);
            _server.Network.Sent.Should().BeEmpty();
        }
        [Test]
        public void StopExplicitlyClearsMembersRoomsAndSkins_WithoutDisconnectCallbacks()
        {
            _server.Add(1); _server.Add(2); int room = _server.Create(1); _server.Join(2, room);
            var payload = SkinTestData.Png(1);
            _server.Dispatcher.Receive(new C2SSkinManifestPacket { State = SkinTestData.State(payload) }, 1);
            _server.Dispatcher.Receive(new C2SSkinDataPacket { Blob = SkinTestData.Blob(payload) }, 1);
            _server.Events.Publish(new ServerStoppedEvent());
            _server.Players.Players.Should().BeEmpty(); _server.Rooms.Rooms.Should().ContainSingle(r => r.Id == 0 && r.PlayerCount == 0);
            _server.Add(1); _server.Add(2); _server.Network.Sent.Clear();
            _server.Dispatcher.Receive(new C2SSkinRequestPacket { Scope = new RoomPacketScope(_server.Member(2).Id, _server.Member(1).Id),
                PlayerId = 1, Hash = SkinHash.Compute(payload) }, 2);
            _server.Network.Sent.Select(s => s.Packet).Should().ContainSingle(p => p is S2CSkinUnavailablePacket);
        }
    }
}
