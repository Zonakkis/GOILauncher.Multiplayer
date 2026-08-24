using System.Collections.Generic;
using System.Linq;
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
    /// 转发皮肤：清单广播给所有人，贴图字节存一份按需下发。
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
        private readonly IEventBus _eventBus;
        private readonly ILogger<SkinRelay> _logger;

        private readonly Dictionary<int, SkinState> _manifests = new Dictionary<int, SkinState>();
        private readonly Dictionary<int, SkinBlob> _blobs = new Dictionary<int, SkinBlob>();

        public SkinRelay(INetworkServer networkServer,
            IServerPacketDispatcher dispatcher,
            IPlayerService playerService,
            IEventBus eventBus,
            ILogger<SkinRelay> logger)
        {
            _networkServer = networkServer;
            _dispatcher = dispatcher;
            _playerService = playerService;
            _eventBus = eventBus;
            _logger = logger;
        }

        void IStartable.Start()
        {
            _dispatcher.RegisterStruct<C2SSkinManifestPacket>(OnManifest);
            _dispatcher.RegisterStruct<C2SSkinDataPacket>(OnData);
            _dispatcher.RegisterStruct<C2SSkinRequestPacket>(OnRequest);
            _eventBus.Subscribe<ClientHandshakeEvent>(OnClientHandshake);
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

            var relayed = new S2CSkinManifestPacket { PlayerId = sender.Id, State = state };
            _networkServer.Multicast(OtherPlayerIds(sender.Id), relayed,
                NetworkChannels.Skin, DeliveryMethod.ReliableOrdered);
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
            _logger.Info("Cached skin {Hash} ({Bytes} bytes) for player {PlayerId}.",
                blob.Hash, blob.Data.Length, sender.Id);
        }

        private void OnRequest(C2SSkinRequestPacket packet, PacketSender sender)
        {
            if (!IsKnownPlayer(sender.Id)) return;

            SkinBlob blob;
            if (_blobs.TryGetValue(packet.PlayerId, out blob) && blob.Hash.Equals(packet.Hash))
            {
                _networkServer.Send(sender.Id,
                    new S2CSkinDataPacket { PlayerId = packet.PlayerId, Blob = blob },
                    NetworkChannels.Skin, DeliveryMethod.ReliableOrdered);
                return;
            }

            // 请求必须有回音，哪怕是"没有"：客户端不会重试，收不到东西那名玩家就永远停在原版上。
            _networkServer.Send(sender.Id,
                new S2CSkinUnavailablePacket { PlayerId = packet.PlayerId, Hash = packet.Hash },
                NetworkChannels.Skin, DeliveryMethod.ReliableOrdered);
        }

        private void OnClientHandshake(ClientHandshakeEvent e)
        {
            // 新来的人只收到玩家名单，名单里没有皮肤。不在这儿补一遍的话，他看到的所有人
            // 都是原版罐子，直到那些人各自重开一次关卡才会重新宣告。
            foreach (var pair in _manifests)
            {
                if (pair.Key == e.PlayerId) continue;

                _networkServer.Send(e.PlayerId,
                    new S2CSkinManifestPacket { PlayerId = pair.Key, State = pair.Value },
                    NetworkChannels.Skin, DeliveryMethod.ReliableOrdered);
            }
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

        private IEnumerable<int> OtherPlayerIds(int playerId)
        {
            // 不按 IsInGame 过滤：在大厅的人也该知道清单，这样他进游戏时贴图已经下载好了。
            return _playerService.Players.Values
                .Where(player => player != null && player.Id != playerId)
                .Select(player => player.Id);
        }
    }
}
