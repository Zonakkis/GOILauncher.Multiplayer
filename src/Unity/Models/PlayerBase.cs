using GOILauncher.Multiplayer.Core.Data.Models;
using UnityEngine;

namespace GOILauncher.Multiplayer.Unity.Models
{
    public abstract class PlayerBase : MonoBehaviour
    {
        private Transform _slider;
        private Transform _nestedHandle;

        public int Id { get; set; }
        public string Name { get; set; }

        protected bool TryGetStateTransforms(out Transform slider, out Transform nestedHandle)
        {
            if (_slider == null)
            {
                _slider = transform.Find("Hub/Slider");
            }
            if (_nestedHandle == null && _slider != null)
            {
                _nestedHandle = _slider.Find("Handle");
            }

            slider = _slider;
            nestedHandle = _nestedHandle;
            return slider != null && nestedHandle != null;
        }

        protected static UnityVector3 FromVector3(Vector3 value)
        {
            return new UnityVector3 { X = value.x, Y = value.y, Z = value.z };
        }

        protected static UnityQuaternion FromQuaternion(Quaternion value)
        {
            return new UnityQuaternion { X = value.x, Y = value.y, Z = value.z, W = value.w };
        }

        public virtual void Init(PlayerInfo info)
        {
            if (info == null)
            {
                return;
            }
            Id = info.Id;
            Name = info.Name;
        }

        public virtual void Reset()
        {
            Id = 0;
            Name = null;
            _slider = null;
            _nestedHandle = null;
        }
    }
}
