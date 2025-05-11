namespace GOILauncher.Multiplayer.Shared.Packets
{
    public enum PacketType : byte
    {
        ServerConnected,
        PlayerConnected,
        PlayerDisconnected,
        PlayerEntered,
        PlayerLeft,
        PlayerMove,
        ChatMessage,
    }
}
