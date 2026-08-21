using System;
using System.Collections.Generic;
using GOILauncher.Multiplayer.Core.Data.Models;

namespace GOILauncher.Multiplayer.Unity.Player
{
    /// <summary>
    /// 玩家列表 UI 的读模型：把两类互不重叠的事实组合起来，让 UI 只依赖一个接口，
    /// 而且不需要自己保存任何副本。
    ///
    /// - 名单事实（Id / Name / Platform / IsInGame）来自 IPlayerService，是网络权威状态；
    /// - 实例事实（有没有远端实例、离本地玩家多远）来自 IPlayerManager，只在场景里存在。
    ///
    /// 距离这类每帧都在变的值不要塞进 PlayerInfo：那是协议模型，服务端也在用。
    /// </summary>
    public interface IPlayerDirectory
    {
        int LocalPlayerId { get; }

        IEnumerable<PlayerInfo> Players { get; }

        bool TryGetPlayer(int playerId, out PlayerInfo player);

        /// <summary>
        /// 该玩家到本地玩家的距离。返回 false 表示他当前没有场景实例——在大厅、
        /// 实例池已满、首个状态包还没到都属于正常情况，UI 显示占位符即可。
        /// </summary>
        bool TryGetDistance(int playerId, out float meters);

        /// <summary>
        /// 名单结构变化（加入 / 离开 / 进出游戏 / 断线）时触发。
        /// 距离这类每帧变化的值不走事件，由 UI 自己按需拉取。
        /// </summary>
        event EventHandler RosterChanged;
    }
}
