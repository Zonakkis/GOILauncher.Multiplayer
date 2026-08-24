using System.IO;
using GOILauncher.Multiplayer.Core.Log;
using UnityEngine;

namespace GOILauncher.Multiplayer.Unity.Skin
{
    /// <summary>
    /// 内置的原版罐子贴图。所有"这名玩家没有自定义皮肤"的情况都渲染成它。
    /// </summary>
    /// <remarks>
    /// 不能拿远端实例自带的贴图当原版：实例是从本地 Player 克隆的，本地玩家装了皮肤的话，
    /// 那张就是本地玩家的皮肤。原版贴图必须由我们自己带一份进来。
    /// 只有一份，全体没皮肤的玩家共用——它是只读的，谁都不会去改。
    /// </remarks>
    public class VanillaPotTexture
    {
        private const string ResourceName =
            "GOILauncher.Multiplayer.Unity.Resources.VanillaPot.png";

        private readonly ILogger<VanillaPotTexture> _logger;

        private Texture2D _texture;
        private bool _loadAttempted;

        public VanillaPotTexture(ILogger<VanillaPotTexture> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 原版贴图；资源缺失或解不开时为 null，调用方应当把它当作"别动贴图"。
        /// </summary>
        public Texture2D Texture
        {
            get
            {
                if (!_loadAttempted)
                {
                    _loadAttempted = true;
                    _texture = Load();
                }
                return _texture;
            }
        }

        private Texture2D Load()
        {
            var bytes = ReadResource();
            if (bytes == null)
            {
                return null;
            }

            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!ImageConversion.LoadImage(texture, bytes))
            {
                _logger.Warn("Failed to decode the embedded vanilla pot texture.");
                Object.Destroy(texture);
                return null;
            }

            texture.name = "VanillaPot";
            return texture;
        }

        private byte[] ReadResource()
        {
            using (var stream = GetType().Assembly.GetManifestResourceStream(ResourceName))
            {
                if (stream == null)
                {
                    // 只在第一次取贴图时抱怨一次：缺了它皮肤同步照样跑，只是没皮肤的远端玩家
                    // 会保持实例原有的贴图，看着像顶着本地玩家的皮肤。
                    _logger.Warn("Embedded resource {ResourceName} is missing; players without a " +
                        "custom skin will keep whatever texture their instance came with.",
                        ResourceName);
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
                        _logger.Warn("Embedded resource {ResourceName} is truncated.", ResourceName);
                        return null;
                    }
                    offset += read;
                }
                return bytes;
            }
        }
    }
}
