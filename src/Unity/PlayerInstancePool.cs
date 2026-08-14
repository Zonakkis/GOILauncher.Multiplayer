using System.Collections.Generic;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Unity.Models;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GOILauncher.Multiplayer.Unity
{
    public interface IPlayerInstancePool
    {
        /// <summary>
        /// 借出一个远端玩家实例：从空闲队列取，若空则按需实例化（动态扩容），并填充玩家信息。
        /// </summary>
        RemotePlayer Rent(PlayerInfo info);

        /// <summary>
        /// 归还一个远端玩家实例：清空信息并停用，放回空闲队列。
        /// </summary>
        void Return(RemotePlayer instance);

        /// <summary>
        /// 预实例化 count 个实例，避免游戏进行中逐个 Instantiate 造成卡顿。
        /// </summary>
        void WarmUp(int count);

        /// <summary>
        /// 销毁全部实例并清空队列，供退出游戏时释放内存。
        /// </summary>
        void Clear();
    }

    public class PlayerInstancePool : IPlayerInstancePool
    {
        private const int MaxInstances = 8;

        private readonly IGameManager _gameManager;
        private readonly ILogger<PlayerInstancePool> _logger;

        private readonly Queue<RemotePlayer> _available = new Queue<RemotePlayer>();
        private readonly List<RemotePlayer> _all = new List<RemotePlayer>();

        public PlayerInstancePool(IGameManager gameManager, ILogger<PlayerInstancePool> logger)
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

            PruneDestroyedInstances();

            RemotePlayer instance;
            if (_available.Count > 0)
            {
                instance = _available.Dequeue();
            }
            else if (_all.Count < MaxInstances)
            {
                instance = CreateInstance();
                if (instance == null)
                {
                    return null;
                }
            }
            else
            {
                _logger.Warn("Player instance pool exhausted, cannot create instance for player {PlayerId}", info.Id);
                return null;
            }

            instance.gameObject.SetActive(true);
            instance.Init(info);
            return instance;
        }

        public void Return(RemotePlayer instance)
        {
            if (instance == null || !_all.Contains(instance))
            {
                return;
            }

            instance.Reset();
            instance.gameObject.SetActive(false);
            if (!_available.Contains(instance))
            {
                _available.Enqueue(instance);
            }
        }

        public void WarmUp(int count)
        {
            PruneDestroyedInstances();
            var targetCount = count < MaxInstances ? count : MaxInstances;
            while (_all.Count < targetCount)
            {
                var instance = CreateInstance();
                if (instance == null)
                {
                    return;
                }
                _available.Enqueue(instance);
            }
        }

        public void Clear()
        {
            foreach (var instance in _all)
            {
                if (instance != null)
                {
                    Object.Destroy(instance.gameObject);
                }
            }
            _all.Clear();
            _available.Clear();
        }

        private void PruneDestroyedInstances()
        {
            _all.RemoveAll(instance => instance == null);

            var availableCount = _available.Count;
            for (var i = 0; i < availableCount; i++)
            {
                var instance = _available.Dequeue();
                if (instance != null && _all.Contains(instance))
                {
                    _available.Enqueue(instance);
                }
            }
        }

        private RemotePlayer CreateInstance()
        {
            var prefab = _gameManager.PlayerPrefab;
            if (prefab == null)
            {
                _logger.Warn("PlayerPrefab is null, cannot create player instance.");
                return null;
            }

            var go = Object.Instantiate(prefab) as GameObject;
            if (go == null)
            {
                _logger.Warn("Failed to instantiate player instance from PlayerPrefab.");
                return null;
            }

            go.name = "RemotePlayerInstance";
            var instance = go.GetComponent<RemotePlayer>();
            if (instance == null)
            {
                instance = go.AddComponent<RemotePlayer>();
            }
            go.SetActive(false);
            _all.Add(instance);
            return instance;
        }
    }
}
