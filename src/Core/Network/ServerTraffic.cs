namespace GOILauncher.Multiplayer.Network
{
    /// <summary>
    /// LiteNetLib's aggregate traffic counters for all server connections. Values are cumulative
    /// for the current server run and never represent a rate.
    /// </summary>
    public struct ServerTraffic
    {
        public long PacketsSent;
        public long PacketsReceived;
        public long BytesSent;
        public long BytesReceived;
        public long PacketLoss;
        public long PacketLossPercent;

        public ServerTraffic(long packetsSent, long packetsReceived, long bytesSent,
            long bytesReceived, long packetLoss, long packetLossPercent)
        {
            PacketsSent = packetsSent; PacketsReceived = packetsReceived; BytesSent = bytesSent;
            BytesReceived = bytesReceived; PacketLoss = packetLoss; PacketLossPercent = packetLossPercent;
        }

        public static ServerTraffic Empty => new ServerTraffic(0, 0, 0, 0, 0, 0);
    }
}
