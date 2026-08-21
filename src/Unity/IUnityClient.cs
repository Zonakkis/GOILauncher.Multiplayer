using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GOILauncher.Multiplayer.Client.Models;
using GOILauncher.Multiplayer.Unity.Player;

namespace GOILauncher.Multiplayer.Unity
{
    /// <summary>
    /// UI 宿主访问客户端的唯一入口。约定见 docs/ui-facade.md：
    /// 新增能力挂在这里（或 IUnityServer）下面，不要为它新开一个根接口。
    /// </summary>
    public interface IUnityClient
    {
        bool IsConnected { get; }
        ReadOnlyCollection<Message> ChatMessages { get; }

        /// <summary>
        /// 当前名单（含本地玩家）。条目是读透权威来源的视图而不是快照，
        /// 可以存在 UI 行上跨帧复用。
        /// </summary>
        IEnumerable<PlayerView> Players { get; }

        bool TryGetPlayer(int playerId, out PlayerView player);

        /// <summary>
        /// 名单结构变化（加入 / 离开 / 进出游戏 / 断线）时触发。
        /// 距离这类每帧都在变的值不走事件，由 UI 自己按需读 PlayerView。
        /// </summary>
        event EventHandler PlayerListUpdated;

        void Connect(string host, int port, string playerName);
        void Disconnect();
        void SendMessage(MessageType type, string message);
    }
}
