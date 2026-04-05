using GOILauncher.Multiplayer.UI.Theme;
using UnityEngine;
using UniverseLib;
using UniverseLib.UI.Models;

namespace GOILauncher.Multiplayer.Extensions
{
    public static class ButtonExtensions
    {
        private static ITheme _theme = Plugin.Theme;
        public static void SetTabActive(this ButtonRef button, bool active)
        {
            Color color = active ? _theme.TabEnabledButtonColor : _theme.TabDisabledButtonColor;
            SetColor(button, color);
        }

        public static void SetConfirm(this ButtonRef button)
        {
            SetColor(button, _theme.ConfirmButtonColor);
        }

        public static void SetCancel(this ButtonRef button)
        {
            SetColor(button, _theme.CancelButtonColor);
        }

        public static void SetColor(this ButtonRef button, Color color)
        {
            RuntimeHelper.SetColorBlock(button.Component, color, color * 1.2f);
        }
    }
}
