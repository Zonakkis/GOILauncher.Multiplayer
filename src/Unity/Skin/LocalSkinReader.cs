using System;
using GOILauncher.Multiplayer.Core.Data.Constants;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Unity.Models;
using UnityEngine;

namespace GOILauncher.Multiplayer.Unity.Skin
{
    /// <summary>
    /// 读本地玩家罐子上现在挂的是什么皮肤。
    /// </summary>
    /// <remarks>
    /// 不认任何具体的皮肤 Mod——不读它的配置、不找它的目录、不碰它的 PlayerPrefs。皮肤 Mod
    /// 的实现各不相同，唯一稳定的是机制：它们最终都把 <c>Pot/Mesh</c> 那个 MeshRenderer 的
    /// 材质 <c>mainTexture</c> 换成一张 Texture2D。所以这里读的就是那张贴图本身，
    /// 谁换的、从哪儿加载的都无所谓。
    ///
    /// 读的是 <c>sharedMaterial</c>：<c>material</c> 会让 Unity 给本地玩家的 Renderer 拷一份
    /// 独立材质，等于替皮肤 Mod 改了它的对象。
    /// </remarks>
    public class LocalSkinReader
    {
        private readonly IGameManager _gameManager;
        private readonly ILogger<LocalSkinReader> _logger;

        // 只记住上一次探过的那张贴图和结果。Payload 为 null 表示"这张编不出来，按原版宣告"，
        // 记下来是为了别每次重开关卡都再撞一次同样的失败（游戏自带贴图必然失败，而且 Unity
        // 会往控制台打一行错）。同一张皮肤重开也不用再 EncodeToPNG——4K 贴图要几十毫秒。
        private Texture2D _probedTexture;
        private byte[] _probedPayload;
        private SkinHash _probedHash;

        public LocalSkinReader(IGameManager gameManager, ILogger<LocalSkinReader> logger)
        {
            _gameManager = gameManager;
            _logger = logger;
        }

        /// <summary>
        /// 读出本地皮肤。<paramref name="payload"/> 为 null 表示用原版贴图，
        /// 此时 <paramref name="state"/> 的哈希为空但金度仍然有效。
        /// 返回 false 表示场景里读不到罐子，什么都不该宣告。
        /// </summary>
        public bool TryRead(out SkinState state, out byte[] payload)
        {
            state = default(SkinState);
            payload = null;

            var material = GetPotSharedMaterial();
            if (material == null)
            {
                return false;
            }

            state = new SkinState
            {
                Slot = SkinConstants.PotSlot,
                Goldness = material.HasProperty(GameConstants.GoldnessProperty)
                    ? material.GetFloat(GameConstants.GoldnessProperty)
                    : 0f
            };

            var texture = material.mainTexture as Texture2D;
            if (texture == null)
            {
                return true;
            }

            byte[] bytes;
            SkinHash hash;
            if (!TryEncode(texture, out bytes, out hash))
            {
                // 编不出来就当没皮肤：别人渲染原版罐子，金度照旧。
                return true;
            }

            state.Hash = hash;
            payload = bytes;
            return true;
        }

        private Material GetPotSharedMaterial()
        {
            var player = _gameManager.Player;
            if (player == null)
            {
                return null;
            }

            var mesh = player.transform.Find(GameConstants.PotMeshPath);
            if (mesh == null)
            {
                _logger.Warn("{Path} not found on the local player, skin will not be announced.",
                    GameConstants.PotMeshPath);
                return null;
            }

            var renderer = mesh.GetComponent<Renderer>();
            return renderer == null ? null : renderer.sharedMaterial;
        }

        private bool TryEncode(Texture2D texture, out byte[] bytes, out SkinHash hash)
        {
            if (_probedTexture != null && ReferenceEquals(_probedTexture, texture))
            {
                bytes = _probedPayload;
                hash = _probedHash;
                return bytes != null;
            }

            bytes = null;
            hash = default(SkinHash);

            if (texture.width > SkinConstants.MaxTextureSize || texture.height > SkinConstants.MaxTextureSize)
            {
                _logger.Warn("Local skin texture is {Width}x{Height}, larger than the {Limit} limit; " +
                    "announcing vanilla instead.", texture.width, texture.height, SkinConstants.MaxTextureSize);
                Remember(texture, null, hash);
                return false;
            }

            byte[] encoded = null;
            try
            {
                encoded = ImageConversion.EncodeToPNG(texture);
            }
            catch (Exception ex)
            {
                // 游戏自带的贴图不可读（CPU 侧没有像素副本），编码会失败——"没装皮肤"走的正是
                // 这条路，所以它不是错误。这个 Unity 版本没有 Texture.isReadable，只能试一次；
                // 失败结果记进备忘录，同一张贴图不会再撞第二次。
                // 故意不做 RenderTexture 回读兜底：那会把原版贴图也传一遍，收端本来就自带一份。
                _logger.Debug("Local pot texture is not encodable ({Reason}); announcing vanilla.",
                    ex.Message);
            }

            if (encoded == null || encoded.Length == 0)
            {
                Remember(texture, null, hash);
                return false;
            }

            if (encoded.Length > SkinConstants.MaxPayloadBytes)
            {
                _logger.Warn("Local skin texture encodes to {Bytes} bytes, over the {Limit} limit; " +
                    "announcing vanilla instead.", encoded.Length, SkinConstants.MaxPayloadBytes);
                Remember(texture, null, hash);
                return false;
            }

            hash = SkinHash.Compute(encoded);
            Remember(texture, encoded, hash);
            bytes = encoded;
            return true;
        }

        private void Remember(Texture2D texture, byte[] payload, SkinHash hash)
        {
            _probedTexture = texture;
            _probedPayload = payload;
            _probedHash = hash;
        }
    }
}
