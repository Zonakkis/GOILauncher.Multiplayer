using System.Collections.Generic;

namespace GOILauncher.Multiplayer.UI.Components
{
    public sealed class DefaultMultiplayerUiComponents : IMultiplayerUiComponents
    {
        public DefaultMultiplayerUiComponents()
        {
            RoomList = new DefaultRoomListUiComponent();
            Chat = new DefaultChatUiComponent();
            PlayerList = new DefaultPlayerListUiComponent();
            ServerControl = new DefaultServerControlUiComponent();
        }

        public IRoomListUiComponent RoomList { get; private set; }

        public IChatUiComponent Chat { get; private set; }

        public IPlayerListUiComponent PlayerList { get; private set; }

        public IServerControlUiComponent ServerControl { get; private set; }
    }

    public sealed class DefaultRoomListUiComponent : IRoomListUiComponent
    {
        private readonly List<RoomListItemViewData> rooms = new List<RoomListItemViewData>
        {
            new RoomListItemViewData("新手休闲房", "1/4"),
            new RoomListItemViewData("双人协作", "2/2"),
            new RoomListItemViewData("速通挑战", "3/4"),
            new RoomListItemViewData("中文交流房", "2/6"),
            new RoomListItemViewData("公开大厅 #1", "5/8"),
            new RoomListItemViewData("公开大厅 #2", "0/8")
        };

        private string playerName = "玩家";
        private string serverIp = "127.0.0.1:7777";

        public IEnumerable<RoomListItemViewData> GetRooms()
        {
            return rooms;
        }

        public void RefreshRooms()
        {
            // Default implementation keeps static demo data.
        }

        public void JoinRoom(string roomName)
        {
            Plugin.Logger.LogInfo("Join requested: " + roomName);
        }

        public string GetPlayerName()
        {
            return playerName;
        }

        public string GetServerIp()
        {
            return serverIp;
        }

        public void Connect(string playerName, string serverIp)
        {
            this.playerName = string.IsNullOrWhiteSpace(playerName) ? "玩家" : playerName.Trim();
            this.serverIp = string.IsNullOrWhiteSpace(serverIp) ? "127.0.0.1:7777" : serverIp.Trim();
            Plugin.Logger.LogInfo("Connect requested: " + this.playerName + " @ " + this.serverIp);
        }

        public void Disconnect()
        {
            Plugin.Logger.LogInfo("Disconnect requested.");
        }
    }

    public sealed class DefaultChatUiComponent : IChatUiComponent
    {
        private readonly List<ChatMessageViewData> messages = new List<ChatMessageViewData>
        {
            new ChatMessageViewData("系统", "欢迎来到多人模式。"),
            new ChatMessageViewData("队友A", "准备好了吗？")
        };

        public IEnumerable<ChatMessageViewData> GetMessages()
        {
            return messages;
        }

        public void SendMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            messages.Add(new ChatMessageViewData("我", message.Trim()));
        }
    }

    public sealed class DefaultPlayerListUiComponent : IPlayerListUiComponent
    {
        private readonly List<PlayerListItemViewData> players = new List<PlayerListItemViewData>
        {
            new PlayerListItemViewData("玩家A", "PC"),
            new PlayerListItemViewData("玩家B", "PC"),
            new PlayerListItemViewData("玩家C", "SteamDeck")
        };

        public IEnumerable<PlayerListItemViewData> GetPlayers()
        {
            return players;
        }
    }

    public sealed class DefaultServerControlUiComponent : IServerControlUiComponent
    {
        private int listenPort = 9027;

        public int GetListenPort()
        {
            return listenPort;
        }

        public void StartServer(int port)
        {
            listenPort = port <= 0 ? 9027 : port;
            Plugin.Logger.LogInfo("Server start requested on port: " + listenPort);
        }

        public void StopServer()
        {
            Plugin.Logger.LogInfo("Server stop requested.");
        }
    }
}