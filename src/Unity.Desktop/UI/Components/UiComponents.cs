using System.Collections.Generic;

namespace GOILauncher.Multiplayer.UI.Components
{
    public sealed class RoomListItemViewData
    {
        public RoomListItemViewData(string roomName, string playerCountText)
        {
            RoomName = roomName;
            PlayerCountText = playerCountText;
        }

        public string RoomName { get; private set; }

        public string PlayerCountText { get; private set; }
    }

    public sealed class ChatMessageViewData
    {
        public ChatMessageViewData(string sender, string message)
        {
            Sender = sender;
            Message = message;
        }

        public string Sender { get; private set; }

        public string Message { get; private set; }
    }

    public interface IRoomListUiComponent
    {
        IEnumerable<RoomListItemViewData> GetRooms();

        void RefreshRooms();

        void JoinRoom(string roomName);

        string GetPlayerName();

        string GetServerIp();

        void Connect(string playerName, string serverIp);

        void Disconnect();
    }

    public interface IChatUiComponent
    {
        IEnumerable<ChatMessageViewData> GetMessages();

        void SendMessage(string message);
    }

    public interface IServerControlUiComponent
    {
        int GetListenPort();

        void StartServer(int port);

        void StopServer();
    }

    public interface IMultiplayerUiComponents
    {
        IRoomListUiComponent RoomList { get; }

        IChatUiComponent Chat { get; }

        IServerControlUiComponent ServerControl { get; }
    }
}
