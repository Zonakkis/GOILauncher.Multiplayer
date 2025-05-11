using System;

namespace GOILauncher.Multiplayer.Shared.Utilities
{
    public class ByteConvertor
    {
        public static string Format(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            if (bytes == 0) return "0 B";
            var order = (int)Math.Log(bytes, 1024);
            var value = bytes / Math.Pow(1024, order);
            return $"{value:0.00} {sizes[order]}";
        }
    }
}
