using System;
using System.Collections;
using System.Collections.Generic;
using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Client.Synchronization;
using GOILauncher.Multiplayer.Core.Data.Constants;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Core.Utils;
using GOILauncher.Multiplayer.Unity.Events;
using GOILauncher.Multiplayer.Unity.Models;
using GOILauncher.Multiplayer.Unity.Player;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GOILauncher.Multiplayer.Unity.Skin
{
    /// <summary>
    /// 皮肤同步的 Unity 适配层：进关卡后读一次本地皮肤宣告出去，把收到的字节解成贴图贴到
    /// 远端实例上。协议和缓存在 <see cref="ClientSkinSync"/>，这里只管 Unity 那一半。
    /// </summary>
    public class SkinSynchronizer : MonoBehaviour
    {

        /// <summary>清单还没到时按黑罐画——金罐是通关后的装饰，未知时按普通的来。</summary>
        private const float UnknownGoldness = 0f;

        public ClientSkinSync SkinSync { get; set; }
        public IClientEventBus EventBus { get; set; }
        public IGameManager GameManager { get; set; }
        public IPlayerManager PlayerManager { get; set; }
        public LocalSkinReader SkinReader { get; set; }
        public VanillaSkins VanillaSkins { get; set; }
        public ILogger<SkinSynchronizer> Logger { get; set; }

        private readonly Dictionary<SkinHash, Texture2D> _textures = new Dictionary<SkinHash, Texture2D>();
        private Coroutine _readRoutine;

        // 探针：本地玩家那份材质每局只记一次，够用来跟远端比对生成贴图是否共用了。
        private bool _localProbeLogged;

        public void Init()
        {
            EventBus.Subscribe<GameStartedEvent>(OnGameStarted);
            EventBus.Subscribe<GameRestartedEvent>(OnGameRestarted);
            EventBus.Subscribe<RemotePlayerInstanceCreatedEvent>(OnRemotePlayerInstanceCreated);
            EventBus.Subscribe<PlayerSkinReceivedEvent>(OnPlayerSkinReceived);
            EventBus.Subscribe<ServerDisconnectedEvent>(OnServerDisconnected);
            EventBus.Subscribe<RoomMembershipChangedEvent>(e => ClearRemoteTextures());
        }

        private void OnGameStarted(GameStartedEvent e)
        {
            _localProbeLogged = false;
            BeginReadLocalSkin();
        }

        private void OnGameRestarted(GameRestartedEvent e)
        {
            _localProbeLogged = false;
            // 重开关卡是玩家换皮肤后让它生效的唯一方式，所以这里必须重新读。
            BeginReadLocalSkin();
        }

        private void BeginReadLocalSkin()
        {
            if (_readRoutine != null)
            {
                StopCoroutine(_readRoutine);
            }
            _readRoutine = StartCoroutine(ReadLocalSkin());
        }

        private IEnumerator ReadLocalSkin()
        {
            // 等待一帧
            yield return null;
            _readRoutine = null;

            List<LocalSkinReader.ReadResult> results;
            if (!SkinReader.TryReadAll(out results))
            {
                yield break;
            }

            // 没连上也照样调用：ClientSkinSync 会先存着，握手完成后自己补发。每个槽位各宣告一次。
            foreach (var result in results)
            {
                SkinSync.Announce(result.State, result.Payload);
            }
        }

        private void OnRemotePlayerInstanceCreated(RemotePlayerInstanceCreatedEvent e)
        {
            var remote = PlayerManager.GetPlayer(e.PlayerId) as RemotePlayer;
            if (remote == null)
            {
                return;
            }

            // 每个槽位各自处理：可能罐子清单到了、身体还没到（或反之）。
            for (var i = 0; i < SkinConstants.Slots.Length; i++)
            {
                var slot = SkinConstants.Slots[i];
                SkinState state;
                byte[] payload;
                if (SkinSync.TryGetSkin(e.PlayerId, slot, out state, out payload))
                {
                    Apply(remote, state, payload);
                    continue;
                }

                // 清单还没到，但实例现在就要露面：它是从池子里借的，上面还留着前一个使用者的贴图，
                // 而模板自带的又是本地玩家的贴图。两种都不能给别人看，先画原版。
                remote.ApplySkin(slot, VanillaSkins.Get(slot), UnknownGoldness, false);
            }
        }

        private void OnPlayerSkinReceived(PlayerSkinReceivedEvent e)
        {
            // 拿不到实例不用管：他尚未进入关卡或者实例池满了，实例建起来时会自己来取。
            var remote = PlayerManager.GetPlayer(e.PlayerId) as RemotePlayer;
            if (remote == null)
            {
                return;
            }

            Apply(remote, e.State, e.Payload);
        }

        private void OnServerDisconnected(ServerDisconnectedEvent e) { ClearRemoteTextures(); }

        private void ClearRemoteTextures()
        {
            foreach (var texture in _textures.Values)
            {
                if (texture != null)
                {
                    Object.Destroy(texture);
                }
            }
            _textures.Clear();
        }

        private void Apply(RemotePlayer remote, SkinState state, byte[] payload)
        {
            // 诊断（Debug 级，平时不刷）：写前后各取一次材质描述，用来看金度到底有没有
            // 落到真正显示的那张贴图上。判读方法见 docs/agent/game-runtime.md 的 Pot Skin。
            var material = FindMaterial(remote.transform, state.Slot, false);
            Logger.Debug("[skin-probe] p{PlayerId} slot{Slot} gold={Goldness} custom={Custom} BEFORE {Desc}",
                remote.Id, state.Slot, state.Goldness, state.HasTexture, SkinMaterial.Describe(material));

            if (!remote.ApplySkin(state.Slot, ResolveTexture(state, payload), state.Goldness, state.HasTexture))
            {
                Logger.Warn("Remote instance of player {PlayerId} has no renderer at {Path} for slot {Slot}, " +
                    "skin not applied.", remote.Id, GameConstants.SkinMeshPath(state.Slot), state.Slot);
            }
            else
            {
                Logger.Debug("[skin-probe] p{PlayerId} slot{Slot} gold={Goldness} custom={Custom} AFTER  {Desc}",
                    remote.Id, state.Slot, state.Goldness, state.HasTexture, SkinMaterial.Describe(material));
            }

            if (!_localProbeLogged)
            {
                _localProbeLogged = true;
                Logger.Debug("[skin-probe] LOCAL slot{Slot} {Desc}",
                    SkinConstants.PotSlot, DescribeLocal(SkinConstants.PotSlot));
            }

            // 贴到实例上之后再扫：现在没人在用的那些才是真的可以扔了。
            PruneTextures();
        }

        /// <summary>
        /// 诊断用：读本地玩家某个槽位 <c>sharedMaterial</c> 的描述（只读）。
        /// 拿它跟远端那份的 <c>generated[...]</c> 实例 ID 比对，就能知道 substance
        /// 生成的贴图是不是被多个材质共用。
        /// </summary>
        private string DescribeLocal(byte slot)
        {
            var player = GameManager == null ? null : GameManager.Player;
            if (player == null)
            {
                return "no-local-player";
            }

            var material = FindMaterial(player.transform, slot, true);
            return material == null ? "no-material" : SkinMaterial.Describe(material);
        }

        /// <summary>
        /// 诊断用：按槽位找材质。<paramref name="shared"/> 为 false 时取实例自己那份副本
        /// （远端实例要看的、也是 <see cref="RemotePlayer.ApplySkin"/> 会写的那一份），
        /// 为 true 时取共享材质——本地玩家必须用这个，否则读一下 <c>material</c> 就替皮肤 Mod
        /// 改了它的对象。返回 null 表示找不到该槽位的 Renderer。
        /// </summary>
        private static Material FindMaterial(Transform root, byte slot, bool shared)
        {
            var path = GameConstants.SkinMeshPath(slot);
            if (path == null)
            {
                return null;
            }

            var mesh = root.Find(path);
            if (mesh == null)
            {
                return null;
            }

            var renderer = mesh.GetComponent<Renderer>();
            if (renderer == null)
            {
                return null;
            }

            return shared ? renderer.sharedMaterial : renderer.material;
        }

        private Texture2D ResolveTexture(SkinState state, byte[] payload)
        {
            if (!state.HasTexture)
            {
                return VanillaSkins.Get(state.Slot);
            }

            Texture2D texture;
            if (_textures.TryGetValue(state.Hash, out texture) && texture != null)
            {
                return texture;
            }

            if (payload == null)
            {
                // 字节还在路上，先画原版；到了会再发一次事件。
                return VanillaSkins.Get(state.Slot);
            }

            texture = Decode(payload);
            if (texture == null)
            {
                return VanillaSkins.Get(state.Slot);
            }

            _textures[state.Hash] = texture;
            return texture;
        }

        private Texture2D Decode(byte[] payload)
        {
            // 尺寸必须在 LoadImage 之前从 PNG 头里读：字节数拦不住压缩炸弹，
            // 一张 100KB 的 PNG 可以解成几个 GB 的像素。
            int width;
            int height;
            if (!PngHeader.TryReadSize(payload, out width, out height) ||
                width > SkinConstants.MaxTextureSize || height > SkinConstants.MaxTextureSize)
            {
                Logger.Warn("Rejected a skin texture: {Width}x{Height} exceeds the {Limit} limit.",
                    width, height, SkinConstants.MaxTextureSize);
                return null;
            }

            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!ImageConversion.LoadImage(texture, payload))
            {
                Logger.Warn("Failed to decode a skin texture ({Bytes} bytes).", payload.Length);
                Object.Destroy(texture);
                return null;
            }
            return texture;
        }

        /// <summary>
        /// 扔掉没有任何玩家在用的贴图。这是本模块里最占内存的东西——4096² 的 RGBA32 是 64MB，
        /// 一次连接里换过几张皮肤就能把内存吃光。
        /// </summary>
        private void PruneTextures()
        {
            if (_textures.Count == 0)
            {
                return;
            }

            List<SkinHash> stale = null;
            foreach (var pair in _textures)
            {
                if (SkinSync.IsHashReferenced(pair.Key)) continue;
                if (stale == null) stale = new List<SkinHash>();
                stale.Add(pair.Key);
            }

            if (stale == null)
            {
                return;
            }

            foreach (var hash in stale)
            {
                Texture2D texture;
                if (_textures.TryGetValue(hash, out texture) && texture != null)
                {
                    Object.Destroy(texture);
                }
                _textures.Remove(hash);
            }
        }
    }
}
