namespace GOILauncher.Multiplayer.Core.Data.Models
{
    /// <summary>
    /// 玩家信息的不可变快照：创建后不可修改，外部 API 只能读取。
    /// 状态更新通过 WithXxx 方法生成新实例。
    /// </summary>
    public class PlayerInfo
    {
        private readonly int _id;
        private readonly string _name;
        private readonly Platform _platform;
        private readonly bool _isInGame;

        public int Id
        {
            get { return _id; }
        }

        public string Name
        {
            get { return _name; }
        }

        public Platform Platform
        {
            get { return _platform; }
        }

        public bool IsInGame
        {
            get { return _isInGame; }
        }

        public PlayerInfo(int id, string name, Platform platform, bool isInGame)
        {
            _id = id;
            _name = name;
            _platform = platform;
            _isInGame = isInGame;
        }

        public PlayerInfo WithIsInGame(bool isInGame)
        {
            return new PlayerInfo(_id, _name, _platform, isInGame);
        }
    }
}
