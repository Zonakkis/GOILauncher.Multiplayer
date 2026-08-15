using GOILauncher.Multiplayer.Core.Data.Models;
using UnityEngine;

namespace GOILauncher.Multiplayer.Unity.Models
{
    public class LocalPlayer : PlayerBase
    {
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
    }
}
