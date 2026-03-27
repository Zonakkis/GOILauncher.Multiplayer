using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GOILauncher.Multiplayer.Client.Models;

namespace GOILauncher.Multiplayer.Client.Extensions
{
    public static class ClientPlayerExtensions
    {
        public static string Format(this ClientPlayer player)
        {
            return $"[{player.Id}][{player.Platform}]{player.Name}";
        }
    }
}
