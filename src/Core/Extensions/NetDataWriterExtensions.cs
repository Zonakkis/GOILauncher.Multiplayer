using GOILauncher.Multiplayer.Core.Data.Models;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Extensions
{
    public static class NetDataWriterExtensions
    {
        public static void Put(this NetDataWriter writer, Platform platform)
        {
            writer.Put((byte)platform);
        }
    }
}
