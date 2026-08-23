using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Unity.Helpers;
using UnityEngine;

namespace GOILauncher.Multiplayer.Unity.Models
{
    public class LocalPlayer : PlayerBase
    {
        private Rigidbody2D[] _rigidbodies;
        private Rigidbody2D[] Rigidbodies
        {
            get
            {
                if (_rigidbodies == null)
                    _rigidbodies = GetComponentsInChildren<Rigidbody2D>();
                return _rigidbodies;
            }
        }
        private Saviour _saviour;
        private Saviour Saviour
        {
            get
            {
                if (_saviour == null)
                    _saviour = GetComponent<Saviour>();
                return _saviour;
            }
        }

        public bool TryCaptureState(out PlayerState state)
        {
            Transform slider;
            Transform nestedHandle;
            if (!TryGetStateTransforms(out slider, out nestedHandle))
            {
                state = default(PlayerState);
                return false;
            }

            state = new PlayerState
            {
                PlayerPosition = FromVector3(transform.position),
                PlayerRotation = FromQuaternion(transform.rotation),
                SliderPosition = FromVector3(slider.position),
                SliderRotation = FromQuaternion(slider.rotation),
                HandlePosition = FromVector3(nestedHandle.position),
                HandleRotation = FromQuaternion(nestedHandle.rotation)
            };
            return true;
        }

        /// <summary>
        /// 把本地玩家传送到 target 处。两个 Transform 在这里都拿得到：
        /// this.transform 就是本地玩家（PlayerManager 把本组件挂在 IGameManager.Player 上），
        /// target 是目标玩家的远端实例，和本地玩家出自同一个 PlayerPrefab，层级结构一致。
        /// </summary>
        public void TeleportTo(Transform target)
        {
            var targetRigidBodies = target.GetComponentsInChildren<Rigidbody2D>();
            if (Rigidbodies.Length != targetRigidBodies.Length)
                return;

            Physics2DHelper.SetAutoSimulation(false);
            try
            {
                for (int count = 0; count < 20; count++)
                {
                    bool synced = true;
                    for (var i = 0; i < Rigidbodies.Length; i++)
                    {
                        if (Vector2.Distance(Rigidbodies[i].position, targetRigidBodies[i].position) >= 0.01f)
                            synced = false;
                        Rigidbodies[i].position = targetRigidBodies[i].position;
                        // Rigidbodies[i].transform.rotation = targetRigidBodies[i].transform.rotation;
                        Rigidbodies[i].velocity = Vector2.zero;
                        Rigidbodies[i].angularVelocity = 0;
                    }
                    Saviour.pc.fakeCursor.position = Saviour.hammer.position;
                    Physics2D.Simulate(Time.fixedDeltaTime);

                    if (synced)
                        break;
                }
            }
            finally
            {
                Physics2DHelper.SetAutoSimulation(true);
            }
        }
    }
}
