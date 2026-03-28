using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace GOILauncher.Multiplayer.Utils
{
    public static class SpriteLoader
    {
        public static Sprite LoadFromFile(string fileName, Vector2 pivot = default)
        {
            var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(folder, fileName);
            byte[] data = File.ReadAllBytes(path);
            var texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);

            texture.LoadImage(data);
            return Sprite.Create(
                texture,
             new Rect(0, 0, texture.width, texture.height),
             pivot == default ? new Vector2(0.5f, 0.5f) : pivot,
             100f,
             0,
             SpriteMeshType.FullRect);
        }
    }
}
