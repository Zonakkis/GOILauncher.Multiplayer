using System.Collections.Generic;
using Autofac;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Constants;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Core.Utils;
using GOILauncher.Multiplayer.Network;
using GOILauncher.Multiplayer.Server.Events;
using GOILauncher.Multiplayer.Server.Services;
using LiteNetLib;

namespace GOILauncher.Multiplayer.Server.Synchronization
{
    /// <summary>
    /// 转发皮肤：清单只发给同房成员，贴图字节按连接存一份、同房按需下发。
    /// </summary>
    /// <remarks>
    /// 服务端存字节不是为了跨玩家去重（两个人用同一张皮肤的概率接近 0），是为了让上传只发生
    /// 一次：一份贴图要发给房间里每个人，还要发给之后进来的人，让上传方按人头重传等于把它的
    /// 上行带宽乘以人数。存的粒度是每人一份，玩家换皮肤时旧那份直接扔掉——旧贴图没人会再要。
    /// 不落盘、不做 LRU：进程重启后所有人都会在下次进游戏时重新宣告。
    /// </remarks>
    public class SkinRelay : IStartable
    {
        private readonly INetworkServer _networkServer;
        private readonly IServerPacketDispatcher _dispatcher;
        private readonly IPlayerService _playerService;
        private readonly IRoomService _rooms;
        private readonly IServerEventBus _eventBus;
        private readonly ILogger<SkinRelay> _logger;

        // 按 (玩家, 槽位) 存：一名玩家可能同时有罐子和身体两份皮肤，各占一条。
        private readonly Dictionary<SkinKey, SkinState> _manifests = new Dictionary<SkinKey, SkinState>();
        private readonly Dictionary<SkinKey, SkinBlob> _blobs = new Dictionary<SkinKey, SkinBlob>();

        public SkinRelay(INetworkServer networkServer,
            IServerPacketDispatcher dispatcher,
            IPlayerService playerService,
            IRoomService rooms,
            IServerEventBus eventBus,
            ILogger<SkinRelay> logger)
        {
            _networkServer = networkServer;
            _dispatcher = dispatcher;
            _playerService = playerService;
            _rooms = rooms;
            _eventBus = eventBus;
            _logger = logger;
        }

        void IStartable.Start()
        {
            _dispatcher.RegisterStruct<C2SSkinManifestPacket>(OnManifest);
            _dispatcher.RegisterStruct<C2SSkinDataPacket>(OnData);
            _dispatcher.RegisterStruct<C2SSkinRequestPacket>(OnRequest);
            _eventBus.Subscribe<PlayerRoomEnteredEvent>(OnRoomEntered);
            _eventBus.Subscribe<ServerStoppedEvent>(e => { _manifests.Clear(); _blobs.Clear(); });
            _eventBus.Subscribe<ClientDisconnectedEvent>(OnClientDisconnected);
        }

        private void OnManifest(C2SSkinManifestPacket packet, PacketSender sender)
        {
            if (!IsKnownPlayer(sender.Id)) return;

            var state = packet.State;
            if (!SkinConstants.IsKnownSlot(state.Slot))
            {
                // 收到本 build 不认识的槽位说明对面比本服务端新，转发出去也没人渲染得了。
                _logger.Warn("Player {PlayerId} announced unknown skin slot {Slot}, ignored.",
                    sender.Id, state.Slot);
                return;
            }

            var key = new SkinKey(sender.Id, state.Slot);
            _manifests[key] = state;

            // 换了皮肤就把旧字节扔掉：留着只会让别人请求到过期内容。
            SkinBlob stored;
            if (_blobs.TryGetValue(key, out stored) && !stored.Hash.Equals(state.Hash))
            {
                _blobs.Remove(key);
            }

            // Do not announce a custom hash until its bytes can actually be served. A request
            // arriving between manifest and upload must not strand a peer on vanilla forever.
            BroadcastManifest(sender.Id, state.Slot);
        }

        private void OnData(C2SSkinDataPacket packet, PacketSender sender)
        {
            if (!IsKnownPlayer(sender.Id)) return;

            // 只收当前宣告的那张。先宣告后上传是协议规定的顺序，否则服务端存的字节就没有
            // 任何东西能证明它属于哪张皮肤。
            var key = new SkinKey(sender.Id, packet.Slot);
            SkinState state;
            if (!_manifests.TryGetValue(key, out state) || !state.HasTexture)
            {
                _logger.Warn("Player {PlayerId} uploaded skin data for slot {Slot} without announcing a manifest, dropped.",
                    sender.Id, packet.Slot);
                return;
            }

            var blob = packet.Blob;
            if (!state.Hash.Equals(blob.Hash))
            {
                _logger.Warn("Player {PlayerId} uploaded skin {Uploaded} but announced {Announced}, dropped.",
                    sender.Id, blob.Hash, state.Hash);
                return;
            }

            string reason;
            if (!SkinPayloadValidator.IsValid(blob, out reason))
            {
                _logger.Warn("Player {PlayerId} uploaded an invalid skin: {Reason}.", sender.Id, reason);
                return;
            }

            _blobs[key] = blob;
            BroadcastManifest(sender.Id, packet.Slot);
            _logger.Info("Cached skin {Hash} ({Bytes} bytes) for player {PlayerId} slot {Slot}.",
                blob.Hash, blob.Data.Length, sender.Id, packet.Slot);
        }

        private void OnRequest(C2SSkinRequestPacket packet, PacketSender sender)
        {
            RoomPacketScope scope;
            if (!_rooms.TryGetScope(sender.Id, packet.PlayerId, out scope)
                || scope.RecipientMembershipId != packet.Scope.RecipientMembershipId
                || scope.PlayerMembershipId != packet.Scope.PlayerMembershipId) return;

            SkinBlob blob;
            if (_blobs.TryGetValue(new SkinKey(packet.PlayerId, packet.Slot), out blob) && blob.Hash.Equals(packet.Hash))
            {
                _networkServer.Send(sender.Id,
                    new S2CSkinDataPacket { Scope = scope, PlayerId = packet.PlayerId, Slot = packet.Slot, Blob = blob },
                    NetworkChannels.Skin, DeliveryMethod.ReliableOrdered);
                return;
            }
            _networkServer.Send(sender.Id,
                new S2CSkinUnavailablePacket { Scope = scope, PlayerId = packet.PlayerId, Slot = packet.Slot, Hash = packet.Hash },
                NetworkChannels.Skin, DeliveryMethod.ReliableOrdered);
        }

        private void OnRoomEntered(PlayerRoomEnteredEvent e)
        {
            foreach (var member in _rooms.GetMembers(e.PlayerId))
            {
                if (member.PlayerId == e.PlayerId) continue;
                // 每名玩家可能有多个槽位的皮肤，两个方向都要把已知槽位都补齐。
                for (var i = 0; i < SkinConstants.Slots.Length; i++)
                {
                    SendManifest(e.PlayerId, member.PlayerId, SkinConstants.Slots[i]);
                    SendManifest(member.PlayerId, e.PlayerId, SkinConstants.Slots[i]);
                }
            }
        }

        private void BroadcastManifest(int playerId, byte slot)
        {
            foreach (var member in _rooms.GetMembers(playerId))
                if (member.PlayerId != playerId) SendManifest(member.PlayerId, playerId, slot);
        }

        private void SendManifest(int recipientId, int playerId, byte slot)
        {
            SkinState state;
            SkinBlob blob;
            RoomPacketScope scope;
            var key = new SkinKey(playerId, slot);
            if (!_rooms.TryGetScope(recipientId, playerId, out scope) || !_manifests.TryGetValue(key, out state)) return;
            if (state.HasTexture && (!_blobs.TryGetValue(key, out blob) || !blob.Hash.Equals(state.Hash))) return;
            // Roster + manifest share one reliable control stream. Only large byte payloads
            // use the independent Skin channel, so an initial manifest cannot outrun its roster.
            _networkServer.Send(recipientId,
                new S2CSkinManifestPacket { Scope = scope, PlayerId = playerId, State = state },
                DeliveryMethod.ReliableOrdered);
        }

        private void OnClientDisconnected(ClientDisconnectedEvent e)
        {
            // 一名玩家的所有槽位都要清掉。
            for (var i = 0; i < SkinConstants.Slots.Length; i++)
            {
                var key = new SkinKey(e.ClientId, SkinConstants.Slots[i]);
                _manifests.Remove(key);
                _blobs.Remove(key);
            }
        }

        private bool IsKnownPlayer(int playerId)
        {
            PlayerInfo info;
            return _playerService.Players.TryGetValue(playerId, out info) && info != null;
        }

    }
}
