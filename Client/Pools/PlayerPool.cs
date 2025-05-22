using System.Collections.Generic;
using System.Threading;
using GOILauncher.Multiplayer.Client.Game;
using UnityEngine;

namespace GOILauncher.Multiplayer.Client.Pools
{
    public class PlayerPool
    {
        private readonly GameObject _playerPrefab;
        private readonly Queue<GameObject> _players = new Queue<GameObject>();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public PlayerPool(GameObject playerPrefab)
        {
            _playerPrefab = playerPrefab;
        }

        public GameObject Get()
        {
            _lock.EnterWriteLock();
            try
            {
                if (_players.Count > 0)
                    return _players.Dequeue();
                var player = Object.Instantiate(_playerPrefab);
                player.AddComponent<RemotePlayer>();
                player.SetActive(true);
                return player;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Release(GameObject player)
        {
            _lock.EnterWriteLock();
            try
            {
                player.SetActive(false);
                _players.Enqueue(player);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
