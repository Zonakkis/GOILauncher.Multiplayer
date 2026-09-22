using System;

namespace GOILauncher.Multiplayer.Core.Data.Models
{
    /// <summary>
    /// 一名玩家某个皮肤槽位的键：<see cref="PlayerId"/> + <see cref="Slot"/>。
    /// 收发两端的按槽位清单/字节字典用它当键——一名玩家现在可能同时有罐子和身体两份皮肤，
    /// 只按 playerId 存会互相覆盖。
    /// </summary>
    /// <remarks>不走网络：槽位在 <see cref="SkinState.Slot"/> 和各包的 <c>Slot</c> 字段里传，
    /// 这个键只是本地字典的组合键，所以不实现 <c>INetSerializable</c>。</remarks>
    public struct SkinKey : IEquatable<SkinKey>
    {
        public readonly int PlayerId;
        public readonly byte Slot;

        public SkinKey(int playerId, byte slot)
        {
            PlayerId = playerId;
            Slot = slot;
        }

        public bool Equals(SkinKey other)
        {
            return PlayerId == other.PlayerId && Slot == other.Slot;
        }

        public override bool Equals(object obj)
        {
            return obj is SkinKey && Equals((SkinKey)obj);
        }

        public override int GetHashCode()
        {
            // net35 没有 HashCode.Combine；playerId 左移 8 位腾出字节给 slot 即可。
            return (PlayerId << 8) ^ Slot;
        }
    }
}
