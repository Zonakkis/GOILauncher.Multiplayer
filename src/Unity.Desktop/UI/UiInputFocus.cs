using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GOILauncher.Multiplayer.UI
{
    internal static class UiInputFocus
    {
        public static bool IsEditing
        {
            get
            {
                var system = EventSystem.current;
                var selected = system == null ? null : system.currentSelectedGameObject;
                var input = selected == null ? null : selected.GetComponent<InputField>();
                return input != null && input.isFocused;
            }
        }
    }
}
