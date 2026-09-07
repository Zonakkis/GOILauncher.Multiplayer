namespace GOILauncher.Multiplayer.Core.Data.Constants
{
    public static class RoomConstants
    {
        public const int LobbyId = 0;
        public const string LobbyName = "大厅";
        public const int MaxNameLength = 64;
        public const int MaxPasswordLength = 128;
        // Changing a wire layout must also change this key. There is no legacy protocol adapter.
        public const string ConnectionKey = "GOILauncher.Rooms.v1";
    }
}
