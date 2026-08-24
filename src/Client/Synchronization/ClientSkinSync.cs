using System.Collections.Generic;
using Autofac;
using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Constants;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Core.Utils;
using GOILauncher.Multiplayer.Network;
using LiteNetLib;

namespace GOILauncher.Multiplayer.Client.Synchronization
{
    /// <summary>
    /// 皮肤的收发两头：把本地读到的皮肤宣告出去，把别人的清单换成可渲染的字节。
    /// 不碰 Unity——贴图解码和材质都在 Unity 层，本模块只管协议和缓存。
    /// </summary>
    /// <remarks>
    /// 字节按哈希缓存，所以同一张贴图在一次连接里只会下载一次；断线时连缓存一起清掉，
    /// 因为重连后服务端的那份缓存可能已经是别人的了。
    /// </remarks>
    public class ClientSkinSync : IStartable
    {
        private readonly INetworkClient _networkClient;
        private readonly IClientPacketDispatcher _dispatcher;
        private readonly IEventBus _eventBus;
        private readonly ILogger<ClientSkinSync> _logger;

        private readonly Dictionary<int, SkinState> _manifests = new Dictionary<int, SkinState>();
        private readonly Dictionary<SkinHash, byte[]> _payloads = new Dictionary<SkinHash, byte[]>();

        /// <summary>
        /// 每个槽位最后真正上传过的哈希。服务端每人每槽只留一份字节，所以这里存的是"服务端
        /// 手上那份是哪张"，不是"传过哪些张"——A→B→A 时 A 那份已经被 B 顶掉了，必须重传。
        /// </summary>
        private readonly Dictionary<byte, SkinHash> _uploadedHashBySlot = new Dictionary<byte, SkinHash>();

        private SkinState _localState;
        private byte[] _localPayload;
        private bool _hasLocalSkin;

        public bool IsConnected => _networkClient.IsConnected;

        public ClientSkinSync(INetworkClient networkClient,
            IClientPacketDispatcher dispatcher,
            IEventBus eventBus,
            ILogger<ClientSkinSync> logger)
        {
            _networkClient = networkClient;
            _dispatcher = dispatcher;
            _eventBus = eventBus;
            _logger = logger;
        }

        void IStartable.Start()
        {
            _dispatcher.RegisterStruct<S2CSkinManifestPacket>(OnManifest);
            _dispatcher.RegisterStruct<S2CSkinDataPacket>(OnData);
            _dispatcher.RegisterStruct<S2CSkinUnavailablePacket>(OnUnavailable);
            _eventBus.Subscribe<LocalPlayerReadyEvent>(OnLocalPlayerReady);
            _eventBus.Subscribe<ServerDisconnectedEvent>(OnServerDisconnected);
            _eventBus.Subscribe<PlayerLeftEvent>(OnPlayerLeft);
        }

        /// <summary>
        /// 宣告本地皮肤。<paramref name="payload"/> 是 PNG 字节，原版贴图传 null。
        /// 没连上也可以调用：状态先留着，握手完成后自动补发。
        /// </summary>
        public void Announce(SkinState state, byte[] payload)
        {
            if (state.HasTexture && (payload == null || payload.Length == 0))
            {
                // 宣告了哈希却拿不出字节，别人只会请求到"没有"。这是调用方的 bug，不是网络问题。
                _logger.Error("Refusing to announce skin {Hash} without payload bytes.", state.Hash);
                return;
            }

            _localState = state;
            _localPayload = payload;
            _hasLocalSkin = true;
            SendLocalSkin();
        }

        /// <summary>
        /// 取一名玩家当前该渲染成什么样。<paramref name="payload"/> 为 null 表示按原版贴图渲染，
        /// 可能是他就用原版，也可能是字节还在路上——两种情况现在都该画原版，字节到了会再发事件。
        /// 返回 false 表示还没收到这名玩家的清单，什么都别改。
        /// </summary>
        public bool TryGetSkin(int playerId, out SkinState state, out byte[] payload)
        {
            payload = null;
            if (!_manifests.TryGetValue(playerId, out state))
            {
                return false;
            }

            if (state.HasTexture)
            {
                _payloads.TryGetValue(state.Hash, out payload);
            }
            return true;
        }

        /// <summary>
        /// 现在还有玩家的清单在用这张贴图吗。给 Unity 层扫解码后的贴图缓存用：一张 4K 贴图
        /// 解开是几十 MB，比字节缓存值钱得多，换过就该扔。
        /// </summary>
        public bool IsHashReferenced(SkinHash hash)
        {
            foreach (var state in _manifests.Values)
            {
                if (state.HasTexture && state.Hash.Equals(hash))
                {
                    return true;
                }
            }
            return false;
        }

        private void SendLocalSkin()
        {
            if (!_networkClient.IsConnected)
            {
                return;
            }

            _networkClient.Send(new C2SSkinManifestPacket { State = _localState },
                NetworkChannels.Skin, DeliveryMethod.ReliableOrdered);

            if (!_localState.HasTexture)
            {
                _uploadedHashBySlot.Remove(_localState.Slot);
                return;
            }

            SkinHash uploaded;
            if (_uploadedHashBySlot.TryGetValue(_localState.Slot, out uploaded) &&
                uploaded.Equals(_localState.Hash))
            {
                return;
            }

            // 不等别人来要：一张贴图迟早每个人都要，服务端存一份能省掉按人头的重传。
            _networkClient.Send(
                new C2SSkinDataPacket { Blob = new SkinBlob { Hash = _localState.Hash, Data = _localPayload } },
                NetworkChannels.Skin, DeliveryMethod.ReliableOrdered);
            _uploadedHashBySlot[_localState.Slot] = _localState.Hash;
        }

        private void OnManifest(S2CSkinManifestPacket packet, PacketSender _)
        {
            var state = packet.State;
            _manifests[packet.PlayerId] = state;
            PruneUnreferencedPayloads();

            if (!state.HasTexture)
            {
                // 原版贴图也要发事件：金度照样要设，而且他可能是从自定义皮肤换回原版的。
                _eventBus.Publish(new PlayerSkinReceivedEvent(packet.PlayerId, state, null));
                return;
            }

            byte[] payload;
            if (_payloads.TryGetValue(state.Hash, out payload))
            {
                _eventBus.Publish(new PlayerSkinReceivedEvent(packet.PlayerId, state, payload));
                return;
            }

            _networkClient.Send(new C2SSkinRequestPacket { PlayerId = packet.PlayerId, Hash = state.Hash },
                NetworkChannels.Skin, DeliveryMethod.ReliableOrdered);
        }

        private void OnData(S2CSkinDataPacket packet, PacketSender _)
        {
            var blob = packet.Blob;

            // 服务端也不能全信：它转发的是别的客户端上传的字节，校验和上限得在这儿再走一遍。
            string reason;
            if (!SkinPayloadValidator.IsValid(blob, out reason))
            {
                _logger.Warn("Received an invalid skin for player {PlayerId}: {Reason}.",
                    packet.PlayerId, reason);
                return;
            }

            _payloads[blob.Hash] = blob.Data;

            SkinState state;
            if (!_manifests.TryGetValue(packet.PlayerId, out state) || !state.Hash.Equals(blob.Hash))
            {
                // 字节在路上时他又换了一张，这份已经过期了。缓存留着没意义，扫掉。
                PruneUnreferencedPayloads();
                return;
            }

            _eventBus.Publish(new PlayerSkinReceivedEvent(packet.PlayerId, state, blob.Data));
        }

        private void OnUnavailable(S2CSkinUnavailablePacket packet, PacketSender _)
        {
            SkinState state;
            if (!_manifests.TryGetValue(packet.PlayerId, out state) || !state.Hash.Equals(packet.Hash))
            {
                return;
            }

            // 不重试：服务端没有就是没有，要等那名玩家下次重开关卡重新宣告。金度仍然生效。
            _logger.Warn("Skin {Hash} of player {PlayerId} is unavailable, rendering vanilla.",
                packet.Hash, packet.PlayerId);
            _eventBus.Publish(new PlayerSkinReceivedEvent(packet.PlayerId, state, null));
        }

        private void OnLocalPlayerReady(LocalPlayerReadyEvent e)
        {
            // 本地那次读取可能发生在连上之前（先进游戏再连服务器），那时 Announce 只存了状态。
            // 上传去重保证这里不会重复传字节。
            if (_hasLocalSkin)
            {
                SendLocalSkin();
            }
        }

        private void OnPlayerLeft(PlayerLeftEvent e)
        {
            _manifests.Remove(e.PlayerId);
            PruneUnreferencedPayloads();
        }

        private void OnServerDisconnected(ServerDisconnectedEvent e)
        {
            _manifests.Clear();
            _payloads.Clear();
            // 本地状态留着：皮肤没变，重连后照原样再宣告一次。但服务端那份缓存不算数了。
            _uploadedHashBySlot.Clear();
        }

        /// <summary>
        /// 扔掉没有任何清单在用的字节。不扫的话一次连接里换过的每张皮肤都会一直占着内存。
        /// 代价是那张要是又被换回来，得重新下一次——服务端那时也会重新拿到上传方的字节。
        /// </summary>
        private void PruneUnreferencedPayloads()
        {
            if (_payloads.Count == 0)
            {
                return;
            }

            var referenced = new HashSet<SkinHash>();
            foreach (var state in _manifests.Values)
            {
                if (state.HasTexture)
                {
                    referenced.Add(state.Hash);
                }
            }

            List<SkinHash> stale = null;
            foreach (var hash in _payloads.Keys)
            {
                if (referenced.Contains(hash)) continue;
                if (stale == null) stale = new List<SkinHash>();
                stale.Add(hash);
            }

            if (stale == null)
            {
                return;
            }

            foreach (var hash in stale)
            {
                _payloads.Remove(hash);
            }
        }
    }
}
