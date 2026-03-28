using System.IO;
using System.Reflection;
using UnityEngine;

namespace GOILauncher.Multiplayer.Utils
{
    public static class SpriteLoader
    {
        public static Sprite LoadFromFile(string fileName, Rect rect = default, Vector2 pivot = default, Vector4 border = default)
        {
            var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(folder, fileName);
            byte[] data = File.ReadAllBytes(path);
            var texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);

            texture.LoadImage(data);
            return Sprite.Create(
                texture,
             rect == default ? new Rect(0, 0, texture.width, texture.height) : rect,
             pivot == default ? new Vector2(0.5f, 0.5f) : pivot,
             100f,
             0,
             SpriteMeshType.FullRect,
             border == default ? new Vector4(0, 0, 0, 0) : border
             );
        }
    }
}
