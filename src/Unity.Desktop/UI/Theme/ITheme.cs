using UnityEngine;

namespace GOILauncher.Multiplayer.UI.Theme
{
    public interface ITheme
    {
        Color NormalButtonColor { get; }
        Color TabEnabledButtonColor { get; }
        Color TabDisabledButtonColor { get; }
        Color ConfirmButtonColor { get; }
        Color CancelButtonColor { get; }
    }
}