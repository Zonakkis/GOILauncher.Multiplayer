using System;
using LiteNetLib.Utils;
using UnityEngine;

namespace GOILauncher.Multiplayer.Shared.Extensions
{
    public static class NetDataWriterExtensions
    {
        public static void Put(this NetDataWriter writer, Vector3 value)
        {
            writer.Put(value.x);
            writer.Put(value.y);
            writer.Put(value.z);
        }

        public static void Put(this NetDataWriter writer, Quaternion value)
        {
            writer.Put(value.x);
            writer.Put(value.y);
            writer.Put(value.z);
            writer.Put(value.w);
        }
    }
}
