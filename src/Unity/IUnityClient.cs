using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GOILauncher.Multiplayer.Client.Models;
using GOILauncher.Multiplayer.Unity.Player;

namespace GOILauncher.Multiplayer.Unity
{
    /// <summary>
    /// UI 宿主访问客户端的唯一入口。约定见 docs/ui-facade.md：
    /// 新增能力挂在这里（或 IUnityServer）下面，不要为它新开一个根接口，
    /// 也不要让 UI 直接订阅 IEventBus。
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
        /// 连接建立。此时 IsConnected 已经为 true。
        /// </summary>
        event Action Connected;

        /// <summary>
        /// 连接断开，参数是原因文本。主动断开、连接失败和掉线都走这一个事件，
        /// 是哪一种由 UI 自己记录的意图判断（门面不猜 UI 的状态机）。
        /// </summary>
        event Action<string> Disconnected;

        /// <summary>
        /// 收到聊天消息，参数是这一条消息。UI 可以二选一：
        /// 忽略参数、从 ChatMessages 读全量重绘（要完整聊天记录时最省事），
        /// 或者按参数的 Type 过滤、自攒一份自己的记录（只关心部分消息时不用每次扫全量）。
        /// </summary>
        event Action<Message> ChatMessageReceived;

        /// <summary>
        /// 名单结构变化（加入 / 离开 / 进出游戏 / 断线）时触发。
        /// 距离这类每帧都在变的值不走事件，由 UI 自己按需读 PlayerView。
        /// </summary>
        event Action PlayerListUpdated;

        void Connect(string host, int port, string playerName);
        void Disconnect();
        void SendMessage(MessageType type, string message);

        /// <summary>
        /// 把本地玩家传送到指定玩家处。对方当前没有远端实例（在大厅、实例池已满、
        /// 本地自己不在游戏里）或传的是自己的 Id 时什么都不做。
        /// PlayerView.Distance 有值即"能传送过去"，UI 按它决定按钮的可用性。
        /// </summary>
        void TeleportTo(int playerId);
    }
}
