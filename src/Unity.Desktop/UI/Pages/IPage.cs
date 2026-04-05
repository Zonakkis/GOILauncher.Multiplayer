using UnityEngine;

namespace GOILauncher.Multiplayer.UI.Pages
{
    internal interface IPage
    {
        GameObject Root { get; }

        void SetActive(bool active);
    }
}