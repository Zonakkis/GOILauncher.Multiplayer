using System.Collections.Generic;
using System.IO;
using GOILauncher.Multiplayer.Core.Data.Constants;
using GOILauncher.Multiplayer.Core.Log;
using UnityEngine;

namespace GOILauncher.Multiplayer.Unity.Skin
{
    /// <summary>
    /// 内置的原版皮肤贴图，按槽位各一张。所有"这名玩家某个部件没有自定义皮肤"的情况都渲染成它。
    /// </summary>
    /// <remarks>
    /// 不能拿远端实例自带的贴图当原版：实例是从本地 Player 克隆的，本地玩家装了皮肤的话，
    /// 那张就是本地玩家的皮肤。原版贴图必须由我们自己带一份进来。
    /// 每个槽位只加载一份，全体没皮肤的玩家共用——它是只读的，谁都不会去改。
    /// </remarks>
    public class VanillaSkins
    {
        // 槽位 → 内嵌资源名。加部件就往这里加一行。
        private static readonly Dictionary<byte, string> ResourceNames = new Dictionary<byte, string>
        {
            { SkinConstants.PotSlot, "GOILauncher.Multiplayer.Unity.Resources.VanillaPot.png" },
            { SkinConstants.BodySlot, "GOILauncher.Multiplayer.Unity.Resources.VanillaDiogenes.png" },
        };

        private readonly ILogger<VanillaSkins> _logger;

        // 按槽位懒加载后记住。存 null 也算"试过了"，避免每次没皮肤的远端露面都再撞一次缺资源。
        private readonly Dictionary<byte, Texture2D> _textures = new Dictionary<byte, Texture2D>();

        public VanillaSkins(ILogger<VanillaSkins> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 某个槽位的原版贴图；资源缺失或解不开时为 null，调用方应当把它当作"别动贴图"。
        /// </summary>
        public Texture2D Get(byte slot)
        {
            Texture2D texture;
            if (_textures.TryGetValue(slot, out texture))
            {
                return texture;
            }

            texture = Load(slot);
            _textures[slot] = texture;
            return texture;
        }

        private Texture2D Load(byte slot)
        {
            string resourceName;
            if (!ResourceNames.TryGetValue(slot, out resourceName))
            {
                _logger.Warn("No vanilla texture is registered for skin slot {Slot}.", slot);
                return null;
            }

            var bytes = ReadResource(resourceName);
            if (bytes == null)
            {
                return null;
            }

            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!ImageConversion.LoadImage(texture, bytes))
            {
                _logger.Warn("Failed to decode the embedded vanilla texture {ResourceName}.", resourceName);
                Object.Destroy(texture);
                return null;
            }

            texture.name = resourceName;
            return texture;
        }

        private byte[] ReadResource(string resourceName)
        {
            using (var stream = GetType().Assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    // 只在第一次取贴图时抱怨一次：缺了它皮肤同步照样跑，只是没皮肤的远端玩家
                    // 会保持实例原有的贴图，看着像顶着本地玩家的皮肤。
                    _logger.Warn("Embedded resource {ResourceName} is missing; players without a " +
                        "custom skin will keep whatever texture their instance came with.",
                        resourceName);
                    return null;
                }

                var bytes = new byte[stream.Length];
                var offset = 0;
                while (offset < bytes.Length)
                {
                    // Stream.CopyTo 是 .NET 4 才有的，这里的目标框架是 3.5。
                    var read = stream.Read(bytes, offset, bytes.Length - offset);
                    if (read <= 0)
                    {
                        _logger.Warn("Embedded resource {ResourceName} is truncated.", resourceName);
                        return null;
                    }
                    offset += read;
                }
                return bytes;
            }
        }
    }
}
