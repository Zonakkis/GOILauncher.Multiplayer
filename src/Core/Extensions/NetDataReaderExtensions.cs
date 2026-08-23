using GOILauncher.Multiplayer.Core.Data.Models;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Extensions
{
    public static class NetDataReaderExtensions
    {

        public static Platform GetPlatform(this NetDataReader reader)
        {
            return (Platform)reader.GetByte();
        }
    }
}
