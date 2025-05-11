using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GOILauncher.Multiplayer.Shared.Extensions
{
    public static class RuntimePlatformExtensions
    {
        public static string ToReadableString(this RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
                case RuntimePlatform.LinuxPlayer:
                    return "Linux";
                case RuntimePlatform.OSXPlayer:
                    return "macOS";
                case RuntimePlatform.WindowsPlayer:
                    return "Windows";
                case RuntimePlatform.WebGLPlayer:
                    return "WebGL";
                default:
                    return platform.ToString();
            }
        }
    }
}
