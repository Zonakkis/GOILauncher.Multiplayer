using UniverseLib.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GOILauncher.Multiplayer.UI.Pages
{
    internal class ServerPageView : IPageView
    {
        public string PageName => "Server";

        public GameObject Root { get; private set; }

        public ServerPageView(GameObject pagesContainer)
        {
            Root = ConstructServerPage(pagesContainer);
        }

        public void SetActive(bool active)
        {
            if (Root != null)
                Root.SetActive(active);
        }

        private static GameObject ConstructServerPage(GameObject pagesContainer)
        {
            GameObject serverPage = UIFactory.CreateVerticalGroup(
                pagesContainer,
                "ServerPage",
                false,
                false,
                true,
                true,
                6,
                new Vector4(8, 8, 8, 8),
                new Color(0.12f, 0.12f, 0.12f, 0.95f));
            UIFactory.SetLayoutElement(serverPage, flexibleHeight: 9999, flexibleWidth: 9999);

            GameObject placeholder = UIFactory.CreateVerticalGroup(
                serverPage,
                "ServerPlaceholder",
                false,
                false,
                true,
                true,
                0,
                new Vector4(8, 8, 8, 8),
                new Color(0.1f, 0.1f, 0.1f, 1f),
                TextAnchor.MiddleCenter);
            UIFactory.SetLayoutElement(placeholder, flexibleHeight: 9999, flexibleWidth: 9999);

            Text placeholderText = UIFactory.CreateLabel(
                placeholder,
                "ServerPlaceholderText",
                "服务端 UI 预留区域",
                TextAnchor.MiddleCenter,
                Color.white,
                true,
                16);
            UIFactory.SetLayoutElement(placeholderText.gameObject, flexibleHeight: 9999, flexibleWidth: 9999);

            return serverPage;
        }
    }
}