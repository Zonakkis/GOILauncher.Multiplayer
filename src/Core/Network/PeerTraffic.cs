namespace GOILauncher.Multiplayer.Network
{
    /// <summary>
    /// One connection's traffic counters, read off LiteNetLib's per-peer statistics.
    /// Lives in Core so the network abstraction can hand diagnostics out without
    /// leaking NetStatistics/NetPeer to the observation layer.
    /// </summary>
    public struct PeerTraffic
    {
        public int ClientId;
        public long PacketsSent;
        public long PacketsReceived;
        public long BytesSent;
        public long BytesReceived;
        public long PacketLoss;
        public long PacketLossPercent;

        public PeerTraffic(int clientId, long packetsSent, long packetsReceived, long bytesSent,
            long bytesReceived, long packetLoss, long packetLossPercent)
        {
            ClientId = clientId; PacketsSent = packetsSent; PacketsReceived = packetsReceived;
            BytesSent = bytesSent; BytesReceived = bytesReceived; PacketLoss = packetLoss;
            PacketLossPercent = packetLossPercent;
        }
    }
}
