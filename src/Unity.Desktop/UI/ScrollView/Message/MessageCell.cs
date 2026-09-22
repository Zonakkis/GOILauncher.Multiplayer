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
             // UIFactory 按 (top, bottom, left, right) 取这个 Vector4，不是直觉的
             // (left, top, right, bottom)。原来的 (5,5,1,1) 实际是左右各 1——
             // 消息文字贴着左边框；上下各 5 又让 26 高的行只剩 16，比 22 高的文本还矮。
             new Vector4(1, 1, 5, 5), Color.clear);
            ImageUtility.MakeTransparent(UIRoot);

            Rect = UIRoot.GetComponent<RectTransform>();
            UIFactory.SetLayoutElement(UIRoot, 400, 26, 9999, 0);

            _messageText = UIFactory.CreateLabel(UIRoot, "Message", "", TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(_messageText.gameObject, 400, 22, 9999, 0);

            return UIRoot;
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
