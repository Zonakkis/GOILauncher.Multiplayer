using GOILauncher.Multiplayer.UI.Theme;
using UnityEngine;
using UniverseLib;
using UniverseLib.UI.Models;

namespace GOILauncher.Multiplayer.Extensions
{
    public static class ButtonExtensions
    {
        private static ITheme Theme => Plugin.Theme;

        /// <summary>
        /// 选中态。同一个方法服务两种控件：页面顶部的页签，以及房间弹窗里
        /// "保持 / 设置新密码 / 移除"那组分段按钮。两者都只是"一组里选一个"，
        /// 所以不必各写一套。
        /// </summary>
        public static void SetSelected(this ButtonRef button, bool selected)
        {
            button.SetColor(selected ? Theme.SelectionActiveColor : Theme.SelectionInactiveColor);
        }

        public static void SetConfirm(this ButtonRef button)
        {
            button.SetColor(Theme.ConfirmButtonColor);
        }

        public static void SetCancel(this ButtonRef button)
        {
            button.SetColor(Theme.CancelButtonColor);
        }

        public static void SetColor(this ButtonRef button, Color color)
        {
            RuntimeHelper.SetColorBlock(button.Component, color, color * 1.2f);
        }
    }
}