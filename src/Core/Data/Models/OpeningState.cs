using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Core.Data.Models
{
    /// <summary>
    /// A snapshot of the opening animation's remaining game seconds (at normal time scale).
    /// Zero is a known completed state, not an absent announcement.
    /// </summary>
    public struct OpeningState : INetSerializable
    {
        public float RemainingSeconds { get; set; }

        public bool IsValid => RemainingSeconds >= 0f
            && !float.IsNaN(RemainingSeconds) && !float.IsInfinity(RemainingSeconds);

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(RemainingSeconds);
        }

        public void Deserialize(NetDataReader reader)
        {
            RemainingSeconds = reader.GetFloat();
        }
    }
}
