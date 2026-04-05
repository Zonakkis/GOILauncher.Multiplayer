using UnityEngine;
using UniverseLib.UI;

namespace GOILauncher.Multiplayer.UI.Theme
{
    public class DarkTheme : ITheme
    {
        public Color NormalButtonColor { get; } = new Color(0.2f, 0.2f, 0.2f);
        public Color TabEnabledButtonColor => UniversalUI.EnabledButtonColor;
        public Color TabDisabledButtonColor => UniversalUI.DisabledButtonColor;
        public Color ConfirmButtonColor { get; } = new Color(0.33f, 0.5f, 0.33f);
        public Color CancelButtonColor { get; } = new Color(0.3f, 0.2f, 0.2f);
    }
}
