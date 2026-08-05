using GOILauncher.Multiplayer.Core.Data.Models;
using UnityEngine;

namespace GOILauncher.Multiplayer.Unity.Models
{
    public abstract class PlayerBase : MonoBehaviour
    {
        public int Id { get; set; }
        public string Name { get; set; }

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
        }
    }
}
