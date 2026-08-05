using System.Collections.Generic;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Unity.Models;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GOILauncher.Multiplayer.Unity
{
    public interface IPlayerAvatarPool
    {
        /// <summary>
        /// 借出一个远端玩家化身：从空闲队列取，若空则按需实例化（动态扩容），并填充玩家信息。
        /// </summary>
        RemotePlayer Rent(PlayerInfo info);

        /// <summary>
        /// 归还一个远端玩家化身：清空信息并停用，放回空闲队列。
        /// </summary>
        void Return(RemotePlayer avatar);

        /// <summary>
        /// 预实例化 count 个化身，避免游戏进行中逐个 Instantiate 造成卡顿。
        /// </summary>
        void WarmUp(int count);

        /// <summary>
        /// 销毁全部化身实例并清空队列，供退出游戏时释放内存。
        /// </summary>
        void Clear();
    }

    public class PlayerAvatarPool : IPlayerAvatarPool
    {
        private const int MaxAvatars = 8;

        private readonly IGameManager _gameManager;
        private readonly ILogger<PlayerAvatarPool> _logger;

        private readonly Queue<RemotePlayer> _available = new Queue<RemotePlayer>();
        private readonly List<RemotePlayer> _all = new List<RemotePlayer>();

        public PlayerAvatarPool(IGameManager gameManager, ILogger<PlayerAvatarPool> logger)
        {
            _gameManager = gameManager;
            _logger = logger;
        }

        public RemotePlayer Rent(PlayerInfo info)
        {
            if (info == null)
            {
                return null;
            }

            RemotePlayer avatar;
            if (_available.Count > 0)
            {
                avatar = _available.Dequeue();
            }
            else if (_all.Count < MaxAvatars)
            {
                avatar = CreateAvatar();
                if (avatar == null)
                {
                    return null;
                }
            }
            else
            {
                _logger.Warn("Player avatar pool exhausted, cannot create avatar for player {PlayerId}", info.Id);
                return null;
            }

            avatar.gameObject.SetActive(true);
            avatar.Init(info);
            return avatar;
        }

        public void Return(RemotePlayer avatar)
        {
            if (avatar == null || !_all.Contains(avatar))
            {
                return;
            }

            avatar.Reset();
            avatar.gameObject.SetActive(false);
            if (!_available.Contains(avatar))
            {
                _available.Enqueue(avatar);
            }
        }

        public void WarmUp(int count)
        {
            while (_all.Count < count)
            {
                if (CreateAvatar() == null)
                {
                    return;
                }
            }
        }

        public void Clear()
        {
            foreach (var avatar in _all)
            {
                if (avatar != null)
                {
                    Object.Destroy(avatar.gameObject);
                }
            }
            _all.Clear();
            _available.Clear();
        }

        private RemotePlayer CreateAvatar()
        {
            var prefab = _gameManager.PlayerPrefab;
            if (prefab == null)
            {
                _logger.Warn("PlayerPrefab is null, cannot create player avatar.");
                return null;
            }

            var instance = Object.Instantiate(prefab) as GameObject;
            if (instance == null)
            {
                _logger.Warn("Failed to instantiate player avatar from PlayerPrefab.");
                return null;
            }

            instance.name = "RemotePlayerAvatar";
            var avatar = instance.GetComponent<RemotePlayer>();
            if (avatar == null)
            {
                avatar = instance.AddComponent<RemotePlayer>();
            }
            instance.SetActive(false);
            _all.Add(avatar);
            _available.Enqueue(avatar);
            return avatar;
        }
    }
}
