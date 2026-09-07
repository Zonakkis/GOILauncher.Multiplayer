using System.Linq;
using GOILauncher.Multiplayer.Client.Services;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using Moq;

namespace GOILauncher.Multiplayer.Tests.Client
{
    internal static class TestClientRoster
    {
        public static IPlayerService Create()
        {
            // These module tests only read the roster; the real roster handlers are tested separately.
            var players = new PlayerService(new FakeNetworkClient(), new RecordingClientDispatcher(),
                new ClientEventBus(new Mock<ILogger<EventBus>>().Object), new Mock<ILogger<PlayerService>>().Object);
            players.SetLocalPlayerInfo(new PlayerInfo(1, "me", Platform.PC, true));
            players.ReplaceRoomRoster(Enumerable.Range(1, 9).Select(id =>
                new RoomMemberInfo(new PlayerInfo(id, "p" + id, Platform.PC, true), (ulong)id + 1)));
            return players;
        }
        public static RoomPacketScope Scope(int playerId) => new RoomPacketScope(2, (ulong)playerId + 1);
    }
}
