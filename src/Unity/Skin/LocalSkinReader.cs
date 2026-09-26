using System;
using System.Collections.Generic;
using GOILauncher.Multiplayer.Core.Data.Constants;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Unity.Models;
using UnityEngine;

namespace GOILauncher.Multiplayer.Unity.Skin
{
    /// <summary>
    /// 读本地玩家各个部件（罐子、身体……）上现在挂的是什么皮肤。
    /// </summary>
    /// <remarks>
    /// 不认任何具体的皮肤 Mod——不读它的配置、不找它的目录、不碰它的 PlayerPrefs。皮肤 Mod
    /// 的实现各不相同，唯一稳定的是机制：它们最终都把对应槽位那个 Renderer 的材质
    /// <c>mainTexture</c> 换成一张 Texture2D。所以这里读的就是那张贴图本身，
    /// 谁换的、从哪儿加载的都无所谓。槽位到 mesh 路径的映射见 <c>GameConstants.SkinMeshPath</c>。
    ///
    /// 读的是 <c>sharedMaterial</c>：<c>material</c> 会让 Unity 给本地玩家的 Renderer 拷一份
    /// 独立材质，等于替皮肤 Mod 改了它的对象。
    /// </remarks>
    public class LocalSkinReader
    {
        private readonly IGameManager _gameManager;
        private readonly ILogger<LocalSkinReader> _logger;

        // 每个槽位只记住上一次探过的那张贴图和结果。Payload 为 null 表示"这张编不出来，按原版
        // 宣告"，记下来是为了别每次重开关卡都再撞一次同样的失败（游戏自带贴图必然失败，而且
        // Unity 会往控制台打一行错）。同一张皮肤重开也不用再 EncodeToPNG——4K 贴图要几十毫秒。
        // 按槽位分开存：罐子和身体是两个 mesh 两张贴图，共用一个字段会互相顶掉、每次都重编码。
        private readonly Dictionary<byte, Probe> _probes = new Dictionary<byte, Probe>();

        private struct Probe
        {
            public Texture2D Texture;
            public byte[] Payload;
            public SkinHash Hash;
        }

        public LocalSkinReader(IGameManager gameManager, ILogger<LocalSkinReader> logger)
        {
            _gameManager = gameManager;
            _logger = logger;
        }

        /// <summary>
        /// 读出本地所有已知槽位的皮肤，每个能读到的槽位产出一条。<c>Payload</c> 为 null 表示那个
        /// 部件用原版贴图，此时该条 <c>State</c> 的哈希为空但金度仍然有效。
        /// 某个槽位的 mesh 在场景里读不到就跳过它（比如身体没找到不影响罐子照常宣告）。
        /// </summary>
        public bool TryReadAll(out List<ReadResult> results)
        {
            results = new List<ReadResult>();

            var player = _gameManager.Player;
            if (player == null)
            {
                return false;
            }

            for (var i = 0; i < SkinConstants.Slots.Length; i++)
            {
                var slot = SkinConstants.Slots[i];
                SkinState state;
                byte[] payload;
                if (TryReadSlot(player.transform, slot, out state, out payload))
                {
                    results.Add(new ReadResult { State = state, Payload = payload });
                }
            }

            return results.Count > 0;
        }

        public struct ReadResult
        {
            public SkinState State;
            public byte[] Payload;
        }

        private bool TryReadSlot(Transform player, byte slot, out SkinState state, out byte[] payload)
        {
            state = default(SkinState);
            payload = null;

            var material = GetSharedMaterial(player, slot);
            if (material == null)
            {
                return false;
            }

            state = new SkinState
            {
                Slot = slot,
                Goldness = SkinMaterial.ReadGoldness(material)
            };

            var texture = material.mainTexture as Texture2D;
            if (texture == null)
            {
                return true;
            }

            byte[] bytes;
            SkinHash hash;
            if (!TryEncode(slot, texture, out bytes, out hash))
            {
                // 编不出来就当没皮肤：别人渲染原版，金度照旧。
                return true;
            }

            state.Hash = hash;
            payload = bytes;
            return true;
        }

        private Material GetSharedMaterial(Transform player, byte slot)
        {
            var path = GameConstants.SkinMeshPath(slot);
            if (path == null)
            {
                return null;
            }

            var mesh = player.Find(path);
            if (mesh == null)
            {
                _logger.Warn("{Path} not found on the local player, slot {Slot} will not be announced.",
                    path, slot);
                return null;
            }

            var renderer = mesh.GetComponent<Renderer>();
            return renderer == null ? null : renderer.sharedMaterial;
        }

        private bool TryEncode(byte slot, Texture2D texture, out byte[] bytes, out SkinHash hash)
        {
            Probe probe;
            if (_probes.TryGetValue(slot, out probe) && probe.Texture != null && ReferenceEquals(probe.Texture, texture))
            {
                bytes = probe.Payload;
                hash = probe.Hash;
                return bytes != null;
            }

            bytes = null;
            hash = default(SkinHash);

            if (texture.width > SkinConstants.MaxTextureSize || texture.height > SkinConstants.MaxTextureSize)
            {
                _logger.Warn("Local skin texture is {Width}x{Height}, larger than the {Limit} limit; " +
                    "announcing vanilla instead.", texture.width, texture.height, SkinConstants.MaxTextureSize);
                Remember(slot, texture, null, hash);
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
                _logger.Debug("Local skin texture for slot {Slot} is not encodable ({Reason}); announcing vanilla.",
                    slot, ex.Message);
            }

            if (encoded == null || encoded.Length == 0)
            {
                Remember(slot, texture, null, hash);
                return false;
            }

            if (encoded.Length > SkinConstants.MaxPayloadBytes)
            {
                _logger.Warn("Local skin texture encodes to {Bytes} bytes, over the {Limit} limit; " +
                    "announcing vanilla instead.", encoded.Length, SkinConstants.MaxPayloadBytes);
                Remember(slot, texture, null, hash);
                return false;
            }

            hash = SkinHash.Compute(encoded);
            Remember(slot, texture, encoded, hash);
            bytes = encoded;
            return true;
        }

        private void Remember(byte slot, Texture2D texture, byte[] payload, SkinHash hash)
        {
            _probes[slot] = new Probe { Texture = texture, Payload = payload, Hash = hash };
        }
    }
}
