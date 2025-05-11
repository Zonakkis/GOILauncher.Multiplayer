using GOILauncher.Multiplayer.Shared.Utilities;

namespace GOILauncher.Multiplayer.Shared.Extensions
{
    public static class LongExtensions
    {
        public static string FormatBytes(this long bytes)
        {
            return ByteConvertor.Format(bytes);
        }
    }
}
