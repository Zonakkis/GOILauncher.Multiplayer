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
        /// <summary>
        /// 进关卡后等多久读本地皮肤。皮肤 Mod 是在场景加载后自己去换贴图的，谁先跑没有保证，
        /// 等一下让它换完。只读这一次：宣告一次就够，一局里皮肤不会再变（换了要重开关卡）。
        /// </summary>
        private const float ReadDelaySeconds = 0.1f;

        /// <summary>清单还没到时按黑罐画——金罐是通关后的装饰，未知时按普通的来。</summary>
        private const float UnknownGoldness = 0f;

        public ClientSkinSync SkinSync { get; set; }
        public IEventBus EventBus { get; set; }
        public IGameManager GameManager { get; set; }
        public IPlayerManager PlayerManager { get; set; }
        public LocalSkinReader SkinReader { get; set; }
        public VanillaPotTexture VanillaTexture { get; set; }
        public ILogger<SkinSynchronizer> Logger { get; set; }

        private readonly Dictionary<SkinHash, Texture2D> _textures = new Dictionary<SkinHash, Texture2D>();
        private Coroutine _readRoutine;

        public void Init()
        {
            EventBus.Subscribe<GameStartedEvent>(OnGameStarted);
            EventBus.Subscribe<GameRestartedEvent>(OnGameRestarted);
            EventBus.Subscribe<RemotePlayerInstanceCreatedEvent>(OnRemotePlayerInstanceCreated);
            EventBus.Subscribe<PlayerSkinReceivedEvent>(OnPlayerSkinReceived);
            EventBus.Subscribe<ServerDisconnectedEvent>(OnServerDisconnected);
        }

        private void OnGameStarted(GameStartedEvent e)
        {
            BeginReadLocalSkin();
        }

        private void OnGameRestarted(GameRestartedEvent e)
        {
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
            // 用真实时间：读的时机跟游戏是否暂停、timeScale 是多少无关。
            yield return new WaitForSecondsRealtime(ReadDelaySeconds);
            _readRoutine = null;

            SkinState state;
            byte[] payload;
            if (!SkinReader.TryRead(out state, out payload))
            {
                yield break;
            }

            // 没连上也照样调用：ClientSkinSync 会先存着，握手完成后自己补发。
            SkinSync.Announce(state, payload);
        }

        private void OnRemotePlayerInstanceCreated(RemotePlayerInstanceCreatedEvent e)
        {
            var remote = PlayerManager.GetPlayer(e.PlayerId) as RemotePlayer;
            if (remote == null)
            {
                return;
            }

            SkinState state;
            byte[] payload;
            if (SkinSync.TryGetSkin(e.PlayerId, out state, out payload))
            {
                Apply(remote, state, payload);
                return;
            }

            // 清单还没到，但实例现在就要露面：它是从池子里借的，上面还留着前一个使用者的贴图，
            // 而模板自带的又是本地玩家的贴图。两种都不能给别人看，先画原版。
            remote.ApplySkin(VanillaTexture.Texture, UnknownGoldness);
        }

        private void OnPlayerSkinReceived(PlayerSkinReceivedEvent e)
        {
            // 拿不到实例不用管：他还在大厅或者实例池满了，实例建起来时会自己来取。
            var remote = PlayerManager.GetPlayer(e.PlayerId) as RemotePlayer;
            if (remote == null)
            {
                return;
            }

            Apply(remote, e.State, e.Payload);
        }

        private void OnServerDisconnected(ServerDisconnectedEvent e)
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
            if (!remote.ApplySkin(ResolveTexture(state, payload), state.Goldness))
            {
                Logger.Warn("Remote instance of player {PlayerId} has no {Path} renderer, " +
                    "skin not applied.", remote.Id, GameConstants.PotMeshPath);
            }

            // 贴到实例上之后再扫：现在没人在用的那些才是真的可以扔了。
            PruneTextures();
        }

        private Texture2D ResolveTexture(SkinState state, byte[] payload)
        {
            if (!state.HasTexture)
            {
                return VanillaTexture.Texture;
            }

            Texture2D texture;
            if (_textures.TryGetValue(state.Hash, out texture) && texture != null)
            {
                return texture;
            }

            if (payload == null)
            {
                // 字节还在路上，先画原版；到了会再发一次事件。
                return VanillaTexture.Texture;
            }

            texture = Decode(payload);
            if (texture == null)
            {
                return VanillaTexture.Texture;
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
