using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Tests.Rooms;
using LiteNetLib.Utils;
using NUnit.Framework;

namespace GOILauncher.Multiplayer.Tests.Data
{
    [TestFixture]
    public class RoomPacketTests
    {
        private static T RoundTrip<T>(T packet) where T : INetSerializable, new()
        {
            var writer = new NetDataWriter(); packet.Serialize(writer);
            var result = new T(); var reader = new NetDataReader(writer.CopyData()); result.Deserialize(reader);
            reader.AvailableBytes.Should().Be(0); return result;
        }
        [TestCase(RoomOperation.Refresh)]
        [TestCase(RoomOperation.Create)]
        [TestCase(RoomOperation.Join)]
        [TestCase(RoomOperation.Leave)]
        [TestCase(RoomOperation.Update)]
        public void Operation_RoundTripsRequestIdentityContextAndAttributes(RoomOperation operation)
        {
            var expected = new C2SRoomOperationPacket { RequestId = 42, MembershipId = ulong.MaxValue, Operation = operation,
                RoomId = 123, Name = "同名房间", MaxPlayers = 6, Password = "case-sensitive", PasswordChange = RoomPasswordChange.Set };
            RoundTrip(expected).Should().BeEquivalentTo(expected);
            var result = new S2CRoomOperationResultPacket { Result = new RoomOperationResult
            { RequestId = 42, Operation = operation, Error = RoomError.RoomFull } };
            RoundTrip(result).Should().BeEquivalentTo(result);
        }
        [TestCase(null)]
        [TestCase(0)]
        [TestCase(23)]
        public void Metadata_RoundTripsNullableOwnerIncludingPlayerZero(int? owner)
        {
            var room = new RoomInfo(owner.HasValue ? 3 : 0, "room", owner.HasValue, 0, 2, owner);
            RoundTrip(room).Should().BeEquivalentTo(room);
        }
        [Test]
        public void RoomEntrySnapshot_RoundTripsMembershipIncarnationsAndSceneFacts()
        {
            var snapshot = new S2CPlayerListPacket { Room = new RoomInfo(2, "room", true, 3, 2, 0), Members = new List<RoomMemberInfo>
            {
                new RoomMemberInfo(new PlayerInfo(0, "owner", Platform.PC, true), 7),
                new RoomMemberInfo(new PlayerInfo(8, "other", Platform.PC, false), 11)
            } };
            RoundTrip(snapshot).Should().BeEquivalentTo(snapshot, options => options.ComparingByMembers<RoomMemberInfo>());
            var directory = new S2CRoomListPacket { Rooms = new List<RoomInfo> { new RoomInfo(0, "大厅", false, 0, 0, null), snapshot.Room } };
            RoundTrip(directory).Should().BeEquivalentTo(directory);
        }
        [Test]
        public void ActualPublicDirectoryAndRoster_DoNotSerializeTheRoomPassword()
        {
            const string secret = "never-public-abcdef";
            var server = new RoomServerFixture(); server.Add(1); server.Create(1, password: secret);
            foreach (var packet in server.Network.Sent.Select(s => s.Packet).Where(p => p is S2CRoomListPacket || p is S2CPlayerListPacket))
            {
                var writer = new NetDataWriter(); packet.Serialize(writer);
                Encoding.UTF8.GetString(writer.CopyData()).Should().NotContain(secret);
            }
            typeof(RoomInfo).GetProperties().Select(p => p.Name).Should().NotContain(new[] { "Password", "PasswordHash", "Salt" });
        }
        [TestCase(-1)]
        [TestCase(int.MaxValue)]
        public void ForgedCollectionLength_IsRejectedBeforeAllocation(int count)
        {
            var writer = new NetDataWriter(); writer.Put(count);
            Action read = () => new S2CRoomListPacket().Deserialize(new NetDataReader(writer.CopyData()));
            read.Should().Throw<ParseException>();
            writer.Reset(); new RoomInfo(0, "大厅", false, 0, 0, null).Serialize(writer); writer.Put(count);
            read = () => new S2CPlayerListPacket().Deserialize(new NetDataReader(writer.CopyData()));
            read.Should().Throw<ParseException>();
        }
        public static IEnumerable<INetSerializable> ScopedPackets()
        {
            var scope = new RoomPacketScope(ulong.MaxValue - 1, ulong.MaxValue);
            yield return new S2CPlayerJoinedPacket { Scope = scope, PlayerId = 4, PlayerName = "name", Platform = Platform.PC, IsInGame = true };
            yield return new S2CPlayerLeftPacket { Scope = scope, PlayerId = 4 };
            yield return new S2CIsInGameUpdatePacket { Scope = scope, PlayerId = 4, IsInGame = true };
            yield return new S2CPlayerStatePacket { Scope = scope, PlayerId = 4, Sequence = 123 };
            yield return new S2CChatMessagePacket { Scope = scope, PlayerId = 4, Content = "chat", Timestamp = 123 };
            yield return new S2CSkinManifestPacket { Scope = scope, PlayerId = 4, State = SkinTestData.Vanilla(1) };
            yield return new S2CSkinDataPacket { Scope = scope, PlayerId = 4, Blob = SkinTestData.Blob(SkinTestData.Png(1)) };
            yield return new S2CSkinUnavailablePacket { Scope = scope, PlayerId = 4 };
            yield return new C2SSkinRequestPacket { Scope = scope, PlayerId = 4 };
        }
        [TestCaseSource(nameof(ScopedPackets))]
        public void EveryRoomDelivery_RoundTripsBothMemberships(INetSerializable expected)
        {
            var writer = new NetDataWriter(); expected.Serialize(writer);
            var actual = (INetSerializable)Activator.CreateInstance(expected.GetType());
            var reader = new NetDataReader(writer.CopyData()); actual.Deserialize(reader);
            reader.AvailableBytes.Should().Be(0);
            var scope = (RoomPacketScope)actual.GetType().GetProperty("Scope").GetValue(actual);
            scope.RecipientMembershipId.Should().Be(ulong.MaxValue - 1); scope.PlayerMembershipId.Should().Be(ulong.MaxValue);
            var second = new NetDataWriter(); actual.Serialize(second); second.CopyData().Should().Equal(writer.CopyData());
        }
    }
}
