using GOILauncher.Multiplayer.Core.Data.Models;
using UnityEngine;

namespace GOILauncher.Multiplayer.Unity.Extensions
{
    public static class PlatformExtensions
    {
        public static string ToReadableString(this Platform platform)
        {
            switch (platform)
            {
                case Platform.Android:
                    return "Android";
                case Platform.iOS:
                    return "iOS";
                case Platform.PC:
                    return "Windows";
                default:
                    return platform.ToString();
            }
        }
    }

    internal static class RuntimePlatformExtensions
    {
        public static Platform ToPlatform(this RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    return Platform.PC;
                case RuntimePlatform.IPhonePlayer:
                    return Platform.iOS;
                case RuntimePlatform.Android:
                    return Platform.Android;
                default:
                    return Platform.Unknown;
            }
        }
    }
}
