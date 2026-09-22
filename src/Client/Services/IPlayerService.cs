using GOILauncher.Multiplayer.Core.Data.Models;
using System.Collections.Generic;

namespace GOILauncher.Multiplayer.Client.Services
{
    /// <summary>
    /// 客户端玩家名单的唯一权威来源。其它模块只读取，不保存副本。
    /// </summary>
    public interface IPlayerService
    {
        PlayerInfo LocalPlayer { get; }
        ulong LocalMembershipId { get; }
        /// <summary>
        /// True if a scoped packet about <paramref name="playerId"/> should be applied: the scope
        /// matches our current membership pairing and the packet is about a remote player, not us.
        /// </summary>
        bool AcceptsRemote(int playerId, RoomPacketScope scope);
        void ReplaceRoomRoster(IEnumerable<RoomMemberInfo> members);

        /// <summary>
        /// 当前名单（含本地玩家）。这是内部字典的实时视图，只用于读取和遍历；
        /// 遍历过程中不要触发会改动名单的操作。
        /// </summary>
        IEnumerable<PlayerInfo> Players { get; }

        bool TryGetPlayer(int playerId, out PlayerInfo player);

        void SetLocalPlayerInfo(PlayerInfo info);
        void SetIsInGame(bool isInGame);
    }
}
