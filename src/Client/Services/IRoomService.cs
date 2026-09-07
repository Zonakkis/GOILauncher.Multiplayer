using System.Collections.Generic;
using GOILauncher.Multiplayer.Core.Data.Models;

namespace GOILauncher.Multiplayer.Client.Services
{
    public interface IRoomService
    {
        IEnumerable<RoomInfo> Rooms { get; }
        RoomInfo CurrentRoom { get; }
        bool IsOperationPending { get; }
        void RefreshRooms();
        void CreateRoom(string name, string password, int maxPlayers);
        void JoinRoom(int roomId, string password);
        void LeaveRoom();
        void UpdateRoom(int roomId, string name, int maxPlayers, RoomPasswordChange passwordChange, string password);
    }
}
