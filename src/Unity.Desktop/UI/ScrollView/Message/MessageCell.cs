using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Widgets.ScrollView;

namespace GOILauncher.Multiplayer.UI.ScrollView.Message
{
    public class MessageCell : ICell
    {
        public bool Enabled => UIRoot.activeSelf;

        public RectTransform Rect { get; set; }
        public GameObject UIRoot { get; set; }

        public float DefaultHeight => 26f;

        public Text _messageText;

        public Client.Models.Message _message;

        public GameObject CreateContent(GameObject parent)
        {
            UIRoot = UIFactory.CreateHorizontalGroup(parent, "MessageCell",
             true, false, true, true, 5,
             new Vector4(5, 5, 1, 1), Color.clear);
            MakeImageTransparent(UIRoot);

            Rect = UIRoot.GetComponent<RectTransform>();
            UIFactory.SetLayoutElement(UIRoot, 400, 26, 9999, 0);

            _messageText = UIFactory.CreateLabel(UIRoot, "Message", "", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(_messageText.gameObject, 400, 22, 9999, 0);

            return UIRoot;
        }

        private static void MakeImageTransparent(GameObject gameObject)
        {
            Image image = gameObject.GetComponent<Image>();
            if (image == null)
                return;

            image.color = Color.clear;
            image.raycastTarget = false;
        }

        public void ConfigureCell(Client.Models.Message message)
        {
            _message = message;
            if (_messageText == null)
                return;

            if (message == null)
            {
                _messageText.text = string.Empty;
                return;
            }

            _messageText.text = $"[{message.Sender}] {message.Content}";
        }

        public void Enable()
        {
            UIRoot.SetActive(true);
        }

        public void Disable()
        {
            UIRoot.SetActive(false);
        }
    }
}
