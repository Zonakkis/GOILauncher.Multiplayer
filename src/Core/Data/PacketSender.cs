namespace GOILauncher.Multiplayer.Core.Data
{
    /// <summary>
    /// Transport-agnostic identity of whoever a packet arrived from.
    /// Keeps LiteNetLib's <c>NetPeer</c> out of the packet-handling surface so
    /// packet handlers can be exercised without a live socket.
    /// </summary>
    public struct PacketSender
    {
        /// <summary>
        /// Peer id assigned by the transport. On the server this is the authoritative
        /// player id; on the client it identifies the server connection and is unused.
        /// </summary>
        public readonly int Id;

        public PacketSender(int id)
        {
            Id = id;
        }
    }
}
