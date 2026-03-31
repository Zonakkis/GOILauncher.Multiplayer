using UnityEngine;

namespace GOILauncher.Multiplayer.UI.Pages
{
    internal interface IPageView
    {
        string PageName { get; }

        GameObject Root { get; }

        void SetActive(bool active);
    }
}