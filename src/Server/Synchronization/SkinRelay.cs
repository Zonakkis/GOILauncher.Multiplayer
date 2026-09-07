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

        private readonly Dictionary<int, SkinState> _manifests = new Dictionary<int, SkinState>();
        private readonly Dictionary<int, SkinBlob> _blobs = new Dictionary<int, SkinBlob>();

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
            if (state.Slot != SkinConstants.PotSlot)
            {
                // 槽位是为了以后加部件留的，现在只认 Pot。收到别的槽位说明对面比本服务端新，
                // 转发出去也没人渲染得了。
                _logger.Warn("Player {PlayerId} announced unknown skin slot {Slot}, ignored.",
                    sender.Id, state.Slot);
                return;
            }

            _manifests[sender.Id] = state;

            // 换了皮肤就把旧字节扔掉：留着只会让别人请求到过期内容。
            SkinBlob stored;
            if (_blobs.TryGetValue(sender.Id, out stored) && !stored.Hash.Equals(state.Hash))
            {
                _blobs.Remove(sender.Id);
            }

            // Do not announce a custom hash until its bytes can actually be served. A request
            // arriving between manifest and upload must not strand a peer on vanilla forever.
            BroadcastManifest(sender.Id);
        }

        private void OnData(C2SSkinDataPacket packet, PacketSender sender)
        {
            if (!IsKnownPlayer(sender.Id)) return;

            // 只收当前宣告的那张。先宣告后上传是协议规定的顺序，否则服务端存的字节就没有
            // 任何东西能证明它属于哪张皮肤。
            SkinState state;
            if (!_manifests.TryGetValue(sender.Id, out state) || !state.HasTexture)
            {
                _logger.Warn("Player {PlayerId} uploaded skin data without announcing a manifest, dropped.",
                    sender.Id);
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

            _blobs[sender.Id] = blob;
            BroadcastManifest(sender.Id);
            _logger.Info("Cached skin {Hash} ({Bytes} bytes) for player {PlayerId}.",
                blob.Hash, blob.Data.Length, sender.Id);
        }

        private void OnRequest(C2SSkinRequestPacket packet, PacketSender sender)
        {
            RoomPacketScope scope;
            if (!_rooms.TryGetScope(sender.Id, packet.PlayerId, out scope)
                || scope.RecipientMembershipId != packet.Scope.RecipientMembershipId
                || scope.PlayerMembershipId != packet.Scope.PlayerMembershipId) return;

            SkinBlob blob;
            if (_blobs.TryGetValue(packet.PlayerId, out blob) && blob.Hash.Equals(packet.Hash))
            {
                _networkServer.Send(sender.Id,
                    new S2CSkinDataPacket { Scope = scope, PlayerId = packet.PlayerId, Blob = blob },
                    NetworkChannels.Skin, DeliveryMethod.ReliableOrdered);
                return;
            }
            _networkServer.Send(sender.Id,
                new S2CSkinUnavailablePacket { Scope = scope, PlayerId = packet.PlayerId, Hash = packet.Hash },
                NetworkChannels.Skin, DeliveryMethod.ReliableOrdered);
        }

        private void OnRoomEntered(PlayerRoomEnteredEvent e)
        {
            foreach (var member in _rooms.GetMembers(e.PlayerId))
            {
                if (member.PlayerId == e.PlayerId) continue;
                SendManifest(e.PlayerId, member.PlayerId);
                SendManifest(member.PlayerId, e.PlayerId);
            }
        }

        private void BroadcastManifest(int playerId)
        {
            foreach (var member in _rooms.GetMembers(playerId))
                if (member.PlayerId != playerId) SendManifest(member.PlayerId, playerId);
        }

        private void SendManifest(int recipientId, int playerId)
        {
            SkinState state;
            SkinBlob blob;
            RoomPacketScope scope;
            if (!_rooms.TryGetScope(recipientId, playerId, out scope) || !_manifests.TryGetValue(playerId, out state)) return;
            if (state.HasTexture && (!_blobs.TryGetValue(playerId, out blob) || !blob.Hash.Equals(state.Hash))) return;
            // Roster + manifest share one reliable control stream. Only large byte payloads
            // use the independent Skin channel, so an initial manifest cannot outrun its roster.
            _networkServer.Send(recipientId,
                new S2CSkinManifestPacket { Scope = scope, PlayerId = playerId, State = state },
                DeliveryMethod.ReliableOrdered);
        }

        private void OnClientDisconnected(ClientDisconnectedEvent e)
        {
            _manifests.Remove(e.ClientId);
            _blobs.Remove(e.ClientId);
        }

        private bool IsKnownPlayer(int playerId)
        {
            PlayerInfo info;
            return _playerService.Players.TryGetValue(playerId, out info) && info != null;
        }

    }
}
