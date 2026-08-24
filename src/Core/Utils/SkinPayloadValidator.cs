using GOILauncher.Multiplayer.Core.Data.Constants;
using GOILauncher.Multiplayer.Core.Data.Models;

namespace GOILauncher.Multiplayer.Core.Utils
{
    /// <summary>
    /// 皮肤字节的准入检查。客户端和服务端跑的是同一套：服务端不能相信上传方，客户端也
    /// 不能相信服务端——服务端可能是别人开的，它转发什么字节都由它说。
    /// </summary>
    public static class SkinPayloadValidator
    {
        public static bool IsValid(SkinBlob blob, out string reason)
        {
            var data = blob.Data;
            if (data == null || data.Length == 0)
            {
                reason = "payload is empty";
                return false;
            }

            if (data.Length > SkinConstants.MaxPayloadBytes)
            {
                reason = string.Format("payload is {0} bytes, over the {1} byte limit",
                    data.Length, SkinConstants.MaxPayloadBytes);
                return false;
            }

            int width, height;
            if (!PngHeader.TryReadSize(data, out width, out height))
            {
                reason = "payload is not a PNG";
                return false;
            }

            if (width > SkinConstants.MaxTextureSize || height > SkinConstants.MaxTextureSize)
            {
                reason = string.Format("texture is {0}x{1}, over the {2} pixel limit",
                    width, height, SkinConstants.MaxTextureSize);
                return false;
            }

            // 哈希放最后：它要扫全部字节，上面几项都是常数时间。
            if (!blob.Hash.Matches(data))
            {
                reason = "payload does not match the announced hash";
                return false;
            }

            reason = null;
            return true;
        }
    }
}
