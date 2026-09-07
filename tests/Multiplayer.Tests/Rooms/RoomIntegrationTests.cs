using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Client.Models;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using NUnit.Framework;

namespace GOILauncher.Multiplayer.Tests.Rooms
{
    [TestFixture]
    public class RoomIntegrationTests
    {
        private static void AssertRoomEntryMessage(RoomClientFixture client, string roomName)
        {
            var message = client.Chat.Messages.Should().ContainSingle().Subject;
            message.Type.Should().Be(MessageType.Server);
            message.Sender.Should().Be("服务器");
            message.Content.Should().Be("已进入" + roomName);
        }

        [Test]
        public void InitialLobbyEntry_AddsOneLocalServerMessage()
        {
            var link = new RoomLink();
            var client = link.Add(1);

            AssertRoomEntryMessage(client, "大厅");
            client.SentHistory.OfType<C2SChatMessagePacket>().Should().BeEmpty();
        }

        [Test]
        public void RoomEntryMessage_IsAddedAfterHistoryReset_AndIsNotBroadcast()
        {
            var link = new RoomLink();
            var joining = link.Add(1);
            var owner = link.Add(2);
            owner.Rooms.CreateRoom("目标房间", null, 0);
            link.Pump();
            joining.Chat.SendMessage(MessageType.System, "old history");
            var notifications = new List<string>();
            joining.Events.Subscribe<ChatHistoryResetEvent>(e => notifications.Add("reset:" + joining.Chat.Messages.Count));
            joining.Events.Subscribe<ChatMessageEvent>(e => notifications.Add("message:" + e.Message.Content));

            joining.Rooms.JoinRoom(owner.Rooms.CurrentRoom.Id, null);
            link.Pump();

            notifications.Should().Equal("reset:0", "message:已进入目标房间");
            AssertRoomEntryMessage(joining, "目标房间");
            joining.SentHistory.OfType<C2SChatMessagePacket>().Should().BeEmpty();
            owner.Chat.Messages.Where(m => m.Content == "已进入目标房间").Should().ContainSingle();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CreatingAndReturningToLobby_AnnouncesTheRoom(bool useJoinCommand)
        {
            var link = new RoomLink();
            var client = link.Add(1);
            client.Rooms.CreateRoom("新房间", null, 0);
            link.Pump();
            AssertRoomEntryMessage(client, "新房间");
            client.Chat.SendMessage(MessageType.System, "old room history");

            if (useJoinCommand) client.Rooms.JoinRoom(0, null);
            else client.Rooms.LeaveRoom();
            link.Pump();

            AssertRoomEntryMessage(client, "大厅");
        }

        [Test]
        public void RefreshEditAndRejoinCurrentRoom_DoNotRepeatTheEntryMessage()
        {
            var link = new RoomLink();
            var client = link.Add(1);
            client.Rooms.CreateRoom("原名", null, 0);
            link.Pump();
            var entry = client.Chat.Messages.Single();
            int appended = 0;
            client.Events.Subscribe<ChatMessageEvent>(e => appended++);

            client.Rooms.RefreshRooms();
            link.Pump();
            client.Rooms.UpdateRoom(client.Rooms.CurrentRoom.Id, "新名", 0, RoomPasswordChange.Keep, null);
            link.Pump();
            client.Rooms.JoinRoom(client.Rooms.CurrentRoom.Id, null);
            link.Pump();

            client.Chat.Messages.Should().Equal(entry);
            entry.Content.Should().Be("已进入原名");
            appended.Should().Be(0);
        }

        [Test]
        public void SocketIdentityAlone_DoesNotPermitRoomTraffic()
        {
            var client = new RoomClientFixture(0); client.Network.Sent.Clear();
            client.Rooms.CurrentRoom.Should().BeNull();
            client.State.Send(default(PlayerState));
            var payload = SkinTestData.Png(1); client.Skin.Announce(SkinTestData.State(payload), payload);
            client.Rooms.CreateRoom("not ready", null, 0);
            client.Network.Sent.Should().BeEmpty(); client.Rooms.IsOperationPending.Should().BeFalse();
        }
        [Test]
        public void PeerIdZero_IsARealOwner_NotTheAbsenceOfOne()
        {
            var link = new RoomLink(); var a = link.Add(0);
            a.Rooms.CreateRoom("owned", null, 0); link.Pump();
            a.Rooms.CurrentRoom.OwnerPlayerId.Should().Be(0); a.Players.LocalMembershipId.Should().NotBe(0);
            link.Server.Member(0).RoomId.Should().Be(a.Rooms.CurrentRoom.Id);
        }
        [Test]
        public void FailedJoinPreservesRosterAndChat_SuccessfulJoinClearsThem()
        {
            var link = new RoomLink(); var a = link.Add(1); var b = link.Add(2);
            b.Rooms.CreateRoom("locked", "secret", 0); link.Pump();
            a.Chat.SendMessage(MessageType.Player, "keep until success"); link.Pump();
            var oldMembership = a.Players.LocalMembershipId;
            int oldCount = a.Chat.Messages.Count; RoomError result = RoomError.None;
            a.Events.Subscribe<RoomOperationCompletedEvent>(e => result = e.Result.Error);
            a.Rooms.JoinRoom(b.Rooms.CurrentRoom.Id, "wrong"); link.Pump();
            result.Should().Be(RoomError.IncorrectPassword); a.Players.LocalMembershipId.Should().Be(oldMembership);
            a.Rooms.CurrentRoom.Id.Should().Be(0); a.Chat.Messages.Should().HaveCount(oldCount);
            a.Rooms.JoinRoom(b.Rooms.CurrentRoom.Id, "secret"); link.Pump();
            AssertRoomEntryMessage(a, "locked"); a.Players.LocalMembershipId.Should().BeGreaterThan(oldMembership);
            a.Players.Players.Select(p => p.Id).Should().BeEquivalentTo(new[] { 1, 2 });
            a.Players.LocalPlayer.IsInGame.Should().BeTrue();
        }
        [Test]
        public void MetadataEditsAndOwnershipChanges_DoNotResetTheRoomSession()
        {
            var link = new RoomLink(); var a = link.Add(1); var b = link.Add(2); var c = link.Add(3);
            a.Rooms.CreateRoom("before", null, 0); link.Pump();
            b.Rooms.JoinRoom(a.Rooms.CurrentRoom.Id, null); link.Pump();
            b.Chat.SendMessage(MessageType.System, "keep"); var count = b.Chat.Messages.Count;
            var membership = b.Players.LocalMembershipId; int changes = 0;
            b.Events.Subscribe<RoomMembershipChangedEvent>(e => changes++);
            a.Rooms.UpdateRoom(a.Rooms.CurrentRoom.Id, "after", 2, RoomPasswordChange.Set, "secret"); link.Pump();
            b.Rooms.CurrentRoom.Name.Should().Be("after"); b.Chat.Messages.Should().HaveCount(count);
            c.Rooms.Rooms.Single(r => r.Id == a.Rooms.CurrentRoom.Id).HasPassword.Should().BeTrue();
            a.Rooms.LeaveRoom(); link.Pump();
            b.Rooms.CurrentRoom.OwnerPlayerId.Should().Be(2); b.Players.LocalMembershipId.Should().Be(membership); changes.Should().Be(0);
        }
        [Test]
        public void DuplicateSubmitAndUnrelatedResult_DoNotUnlockOrRunAnotherOperation()
        {
            var link = new RoomLink(); var a = link.Add(1);
            a.Rooms.CreateRoom("once", null, 0);
            var request = a.Network.Sent.Select(s => s.Packet).OfType<C2SRoomOperationPacket>().Single();
            a.Rooms.CreateRoom("twice", null, 0);
            a.Network.Sent.Should().ContainSingle(); a.Rooms.IsOperationPending.Should().BeTrue();
            a.Dispatcher.Receive(new S2CRoomOperationResultPacket { Result = new RoomOperationResult
            { RequestId = request.RequestId + 1, Operation = request.Operation } });
            a.Rooms.IsOperationPending.Should().BeTrue();
            link.Pump();
            a.Rooms.IsOperationPending.Should().BeFalse(); a.Rooms.Rooms.Should().HaveCount(2);
            a.Rooms.CurrentRoom.Name.Should().Be("once");
        }
        [Test]
        public void SwitchingRooms_ClearsRemoteCaches_AndRejectsOldReceiverIncarnationsEvenOnReturn()
        {
            var link = new RoomLink(); var a = link.Add(1); var b = link.Add(2);
            var payload = SkinTestData.Png(1); var state = SkinTestData.State(payload);
            b.Skin.Announce(state, payload); link.Pump();
            var scope = new RoomPacketScope(a.Players.LocalMembershipId, b.Players.LocalMembershipId);
            a.Rooms.CreateRoom("away", null, 0); link.Pump();
            SkinState skin; byte[] bytes;
            a.Skin.TryGetSkin(2, out skin, out bytes).Should().BeFalse();
            int states = 0, skins = 0;
            a.Events.Subscribe<PlayerStateReceivedEvent>(e => states++); a.Events.Subscribe<PlayerSkinReceivedEvent>(e => skins++);
            a.Dispatcher.Receive(new S2CPlayerJoinedPacket { Scope = scope, PlayerId = 2, PlayerName = "late", IsInGame = true });
            a.Dispatcher.Receive(new S2CPlayerStatePacket { Scope = scope, PlayerId = 2, Sequence = 1000 });
            a.Dispatcher.Receive(new S2CSkinManifestPacket { Scope = scope, PlayerId = 2, State = state });
            a.Dispatcher.Receive(new S2CSkinDataPacket { Scope = scope, PlayerId = 2, Blob = SkinTestData.Blob(payload) });
            a.Dispatcher.Receive(new S2CChatMessagePacket { Scope = scope, PlayerId = 2, Content = "late chat" });
            states.Should().Be(0); skins.Should().Be(0); AssertRoomEntryMessage(a, "away");
            a.Players.Players.Select(p => p.Id).Should().Equal(1);
            a.Rooms.LeaveRoom(); link.Pump();
            a.Skin.TryGetSkin(2, out skin, out bytes).Should().BeTrue(); bytes.Should().Equal(payload);
            skins = 0;
            a.Dispatcher.Receive(new S2CPlayerLeftPacket { Scope = scope, PlayerId = 2 });
            a.Dispatcher.Receive(new S2CPlayerStatePacket { Scope = scope, PlayerId = 2, Sequence = 1000 });
            a.Dispatcher.Receive(new S2CSkinUnavailablePacket { Scope = scope, PlayerId = 2, Hash = state.Hash });
            a.Dispatcher.Receive(new S2CSkinDataPacket { Scope = scope, PlayerId = 2, Blob = SkinTestData.Blob(payload) });
            a.Players.Players.Select(p => p.Id).Should().BeEquivalentTo(new[] { 1, 2 });
            skins.Should().Be(0); states.Should().Be(0);
            b.State.Send(default(PlayerState)); link.Pump(); states.Should().Be(1);
        }
        [Test]
        public void SenderLeavesAndReturns_TheReceiverRejectsTheirOldIncarnation()
        {
            var link = new RoomLink(); var a = link.Add(1); var b = link.Add(2);
            var oldScope = new RoomPacketScope(a.Players.LocalMembershipId, b.Players.LocalMembershipId);
            b.Rooms.CreateRoom("away", null, 0); link.Pump(); b.Rooms.LeaveRoom(); link.Pump();
            a.Players.LocalMembershipId.Should().Be(oldScope.RecipientMembershipId);
            int received = 0; a.Events.Subscribe<PlayerStateReceivedEvent>(e => received++);
            a.Dispatcher.Receive(new S2CPlayerStatePacket { PlayerId = 2, Scope = oldScope, Sequence = 1000 });
            received.Should().Be(0);
            b.State.Send(default(PlayerState)); link.Pump(); received.Should().Be(1);
        }
        [Test]
        public void SkinRequests_CannotCrossRoomsOrUseOldMembershipIds()
        {
            var link = new RoomLink(); var a = link.Add(1); var b = link.Add(2);
            var payload = SkinTestData.Png(1); b.Skin.Announce(SkinTestData.State(payload), payload); link.Pump();
            var old = new RoomPacketScope(a.Players.LocalMembershipId, b.Players.LocalMembershipId);
            a.Rooms.CreateRoom("away", null, 0); link.Pump();
            link.Server.Dispatcher.Receive(new C2SSkinRequestPacket { Scope = new RoomPacketScope(a.Players.LocalMembershipId, b.Players.LocalMembershipId),
                PlayerId = 2, Hash = SkinHash.Compute(payload) }, 1);
            link.Server.Network.Sent.Should().BeEmpty();
            a.Rooms.LeaveRoom(); link.Pump();
            link.Server.Dispatcher.Receive(new C2SSkinRequestPacket { Scope = old, PlayerId = 2, Hash = SkinHash.Compute(payload) }, 1);
            link.Server.Network.Sent.Should().BeEmpty();
        }
        [Test]
        public void RoomEntry_ReusesServerSkinCacheWithoutUploadingTheBytesAgain()
        {
            var link = new RoomLink(); var a = link.Add(1); var b = link.Add(2);
            var payload = SkinTestData.Png(1); a.Skin.Announce(SkinTestData.State(payload), payload); link.Pump();
            int uploads = a.SentHistory.OfType<C2SSkinDataPacket>().Count();
            a.Rooms.CreateRoom("new", null, 0); link.Pump(); b.Rooms.JoinRoom(a.Rooms.CurrentRoom.Id, null); link.Pump();
            a.SentHistory.OfType<C2SSkinDataPacket>().Should().HaveCount(uploads);
            SkinState state; byte[] received;
            b.Skin.TryGetSkin(1, out state, out received).Should().BeTrue(); received.Should().Equal(payload);
        }
        [Test]
        public void UploadInFlightDuringRoomChange_CompletesOnlyInTheCurrentRoom()
        {
            var link = new RoomLink(); var a = link.Add(1); var b = link.Add(2); var c = link.Add(3);
            b.Rooms.CreateRoom("target", null, 0); link.Pump();
            var payload = SkinTestData.Png(1); a.Skin.Announce(SkinTestData.State(payload), payload);
            var upload = a.Network.Sent.Select(s => s.Packet).OfType<C2SSkinDataPacket>().Single();
            var manifest = a.Network.Sent.Select(s => s.Packet).OfType<C2SSkinManifestPacket>().Single();
            a.Network.Sent.Clear(); link.Server.Dispatcher.Receive(manifest, 1);
            link.Server.Network.Sent.Should().BeEmpty(); // not advertised before bytes are available
            a.Rooms.JoinRoom(b.Rooms.CurrentRoom.Id, null); link.Pump();
            link.Server.Dispatcher.Receive(upload, 1); link.Pump();
            SkinState state; byte[] received;
            b.Skin.TryGetSkin(1, out state, out received).Should().BeTrue(); received.Should().Equal(payload);
            c.Skin.TryGetSkin(1, out state, out received).Should().BeFalse();
        }
        [Test]
        public void SceneChangeQueuedAfterRoomRequest_IsNotOverwrittenByEntrySnapshot()
        {
            var link = new RoomLink(); var a = link.Add(1);
            a.Rooms.CreateRoom("new", null, 0); a.Players.SetIsInGame(false); link.Pump();
            a.Players.LocalPlayer.IsInGame.Should().BeFalse(); link.Server.Players.Players[1].IsInGame.Should().BeFalse();
        }
        [Test]
        public void DisconnectClearsRoomAndPendingState_ReconnectStartsInLobbyAndReuploadsOwnSkin()
        {
            var link = new RoomLink(); var a = link.Add(1); var b = link.Add(2);
            var payload = SkinTestData.Png(1); a.Skin.Announce(SkinTestData.State(payload), payload); link.Pump();
            a.Rooms.CreateRoom("old room", null, 0); link.Pump(); b.Rooms.JoinRoom(a.Rooms.CurrentRoom.Id, null); link.Pump();
            int uploads = a.SentHistory.OfType<C2SSkinDataPacket>().Count();
            a.Chat.SendMessage(MessageType.System, "old");
            a.Network.IsConnected = false; a.Events.Publish(new ServerDisconnectedEvent("lost"));
            link.Server.Events.Publish(new ClientDisconnectedEvent(1, "lost")); link.Pump();
            a.Rooms.CurrentRoom.Should().BeNull(); a.Rooms.Rooms.Should().BeEmpty(); a.Chat.Messages.Should().BeEmpty();
            a.Players.Players.Should().BeEmpty(); a.Rooms.IsOperationPending.Should().BeFalse();
            a.Network.IsConnected = true; a.Events.Publish(new ServerHandshakeEvent(1)); link.Pump();
            a.Rooms.CurrentRoom.Id.Should().Be(0); a.SentHistory.OfType<C2SSkinDataPacket>().Should().HaveCount(uploads + 1);
        }
    }
}
