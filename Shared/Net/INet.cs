namespace GOILauncher.Multiplayer.Shared.Net
{
    public interface INet
    {
        long BytesSent { get; }
        long BytesReceived { get; }
        long PacketLossPercentage { get; }
    }
}
