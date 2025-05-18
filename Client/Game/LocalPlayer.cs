using GOILauncher.Multiplayer.Client.Interfaces;
using GOILauncher.Multiplayer.Shared.Utilities;
using UnityEngine;

namespace GOILauncher.Multiplayer.Client.Game
{
    public class LocalPlayer : PlayerBase
    {
        private PlayerControl _playerControl;

        public new void Awake()
        {
            base.Awake();
            _playerControl = Player.GetComponent<PlayerControl>();
        }

        public void Teleport(IGamePlayer remotePlayer)
        {
            var count = 0;
            var rigibodies = Player.GetComponentsInChildren<Rigidbody2D>();
            var remoteRigidBodies = remotePlayer.Player.GetComponentsInChildren<Rigidbody2D>();
            Physical2DUtility.SetAutoSimulation(false);
            while (count < 50)
            {
                count++;
                var synced = true;
                for (var i = 0; i < rigibodies.Length; i++)
                {
                    if (synced &&
                        Vector2.Distance(rigibodies[i].position, remoteRigidBodies[i].position) >= 0.05f)
                        synced = false;
                    rigibodies[i].position = remoteRigidBodies[i].position;
                    rigibodies[i].angularVelocity = 0;
                    rigibodies[i].velocity = Vector2.zero;
                }
                _playerControl.fakeCursor.position = _playerControl.tip.position;
                Physics2D.Simulate(Time.fixedDeltaTime);
                if (synced)
                    break;
            }
            Physical2DUtility.SetAutoSimulation(true);
        }
    }
}
