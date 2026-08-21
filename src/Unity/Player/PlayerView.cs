using GOILauncher.Multiplayer.Client.Services;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Unity.Models;

namespace GOILauncher.Multiplayer.Unity.Player
{
    /// <summary>
    /// UI 看到的一名玩家。它不保存任何事实，只记住 Id 和两个权威来源，每次读属性都回去取，
    /// 所以可以存在列表行上跨帧复用——换成存 PlayerInfo 就会在对方改名后读到旧值。
    ///
    /// 派生显示值（距离，以后可能的高度差、进度）放在这里，不放在 IUnityClient 上：
    /// 否则每加一列就得给门面加一个方法。也不要放进 PlayerInfo——那是协议模型，服务端也在用。
    ///
    /// 用值类型是为了让 UI 按帧重新枚举名单时不给每个玩家分配对象，
    /// 也省掉一份要跟着名单增删的 id→条目 字典。
    /// </summary>
    public struct PlayerView
    {
        private readonly int _id;
        private readonly IPlayerService _roster;
        private readonly IPlayerManager _instances;

        public PlayerView(int id, IPlayerService roster, IPlayerManager instances)
        {
            _id = id;
            _roster = roster;
            _instances = instances;
        }

        public int Id
        {
            get { return _id; }
        }

        public string Name
        {
            get
            {
                PlayerInfo info = Info;
                return info == null ? string.Empty : info.Name;
            }
        }

        public Platform Platform
        {
            get
            {
                PlayerInfo info = Info;
                return info == null ? default(Platform) : info.Platform;
            }
        }

        public bool IsInGame
        {
            get
            {
                PlayerInfo info = Info;
                return info != null && info.IsInGame;
            }
        }

        public bool IsLocal
        {
            get
            {
                if (_roster == null)
                    return false;

                PlayerInfo local = _roster.LocalPlayer;
                return local != null && local.Id == _id;
            }
        }

        /// <summary>
        /// 到本地玩家的直线距离（米）。null 表示这名玩家当前没有远端实例——在大厅、
        /// 实例池已满、首个状态包还没到都属于正常情况，本地玩家自己也是 null。
        ///
        /// 这个值同时就是"能不能传送过去"的判据：没有实例就没有目标位置。
        /// </summary>
        public float? Distance
        {
            get
            {
                if (_instances == null)
                    return null;

                RemotePlayer remote = _instances.GetPlayer(_id) as RemotePlayer;
                return remote == null ? (float?)null : remote.DistanceToLocalPlayer;
            }
        }

        // 名单里已经没有这个 Id（刚离开），或者拿到的是 default(PlayerView) 时返回 null，
        // 让调用方读到空值而不是抛异常。
        private PlayerInfo Info
        {
            get
            {
                PlayerInfo info;
                if (_roster != null && _roster.TryGetPlayer(_id, out info))
                    return info;

                return null;
            }
        }
    }
}
