using GOILauncher.Multiplayer.UI.Theme;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace GOILauncher.Multiplayer.UI
{
    /// <summary>
    /// UIFactory 的补充，分两块：
    ///
    /// - **结构**：区块头、分隔线、表格单元格。顺带解决层次感问题——UIFactory 的每个
    ///   Group 都自带一张 Image，不显式指定底色就全是同一个 0.17 的灰，嵌套几层之后
    ///   所有容器糊成一块。这里每个方法都要求调用方给出底色，逼着调用点想清楚
    ///   "这块相对旁边是浮起来还是陷进去"。
    /// - **控件**：按钮和输入框。UIFactory.CreateButton 只给背景色，文字色、内边距、
    ///   高度得自己补；补漏了就会出现"有的按钮字大有的字小""有的文字贴边"。
    ///
    /// 都在这里做掉，页面里只管写业务回调。
    /// </summary>
    internal static class UiKit
    {
        private static ITheme Theme => Plugin.Theme;

        #region 结构

        /// <summary>改掉 Group 自带的那张背景图（UIFactory 不传底色时是 0.17 灰）。</summary>
        public static void SetBackground(GameObject target, Color color)
        {
            Image image = target.GetComponent<Image>();
            if (image != null)
                image.color = color;
        }

        /// <summary>
        /// 卡片内部的分隔线。做成 1px 高的空对象而不是给相邻容器画边框——Unity 的 Image
        /// 不能只画一条边，用独立一行最省事，也不会打乱两边的 padding。
        /// </summary>
        public static GameObject CreateDivider(GameObject parent, Color color)
        {
            GameObject divider = UIFactory.CreateUIObject("Divider", parent);
            UIFactory.SetLayoutElement(divider, minHeight: 1, preferredHeight: 1, flexibleHeight: 0, flexibleWidth: 9999);

            Image image = divider.AddComponent<Image>();
            image.color = color;
            // 细线不该挡住下面控件的点击。
            image.raycastTarget = false;
            return divider;
        }

        /// <summary>
        /// 区块头：左边标题，右边留给调用方追加按钮。底色比它下面的卡片亮一档，
        /// 靠"标题 + 底色 + 间距"分区，不靠边框。
        /// </summary>
        public static GameObject CreateSectionHeader(GameObject parent, string name, string title)
        {
            GameObject header = UIFactory.CreateHorizontalGroup(
                parent, name, true, false, true, true, Layout.SpaceSm,
                // UIFactory 按 (top, bottom, left, right) 取这个 Vector4，不是直觉的
                // (left, top, right, bottom)。写成后者的话左边距会跑到上面去，
                // 标题被压下去、可用高度只剩 32-12-6=14，比标题自己要求的 20 还矮。
                new Vector4(Layout.SpaceSm, Layout.SpaceSm, Layout.SpaceLg, Layout.SpaceSm),
                Theme.SurfaceHeader, TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(header, minHeight: Layout.SectionHeaderHeight, preferredHeight: Layout.SectionHeaderHeight,
                flexibleHeight: 0, flexibleWidth: 9999);

            Text label = UIFactory.CreateLabel(header, name + "Title", title, TextAnchor.MiddleLeft,
                Theme.TextPrimary, false, Layout.FontSection);
            label.fontStyle = FontStyle.Bold;
            UIFactory.SetLayoutElement(label.gameObject, minHeight: 20, flexibleHeight: 0, flexibleWidth: 9999);
            return header;
        }

        /// <summary>
        /// 表格里的一格。<paramref name="width"/> 为 null 表示"吃掉剩余宽度"——一张表里
        /// 至多让一列这么干，否则几列会互相抢空间。
        ///
        /// 表头和数据行都走这里，列宽因此天然一致；两边各写一遍数字正是错位的来源。
        /// </summary>
        public static Text CreateTextCell(GameObject parent, string name, string text, int? width,
            TextAnchor alignment, Color color, int fontSize = Layout.FontBody)
        {
            Text label = UIFactory.CreateLabel(parent, name, text, alignment, color, false, fontSize);
            if (width.HasValue)
                UIFactory.SetLayoutElement(label.gameObject, minWidth: width.Value, preferredWidth: width.Value,
                    minHeight: 20, flexibleHeight: 0, flexibleWidth: 0);
            else
                UIFactory.SetLayoutElement(label.gameObject, minWidth: 0, minHeight: 20, flexibleHeight: 0, flexibleWidth: 9999);
            return label;
        }

        #endregion

        #region 控件

        /// <summary>普通按钮：中性底色。"刷新""关闭"这类不改变连接状态的动作用它。</summary>
        public static ButtonRef CreateButton(GameObject parent, string name, string text, int width = 0)
        {
            return CreateButton(parent, name, text, Theme.SurfaceHeader, width);
        }

        /// <summary>确认按钮：绿底。"加入""创建""连接""启动"这类会真的发生事情的动作用它。</summary>
        public static ButtonRef CreateConfirmButton(GameObject parent, string name, string text, int width = 0)
        {
            return CreateButton(parent, name, text, Theme.ConfirmButtonColor, width);
        }

        /// <summary>取消按钮：红底。"断开""停止"这类会中断当前状态的动作才用它，不要拿它当"关闭"。</summary>
        public static ButtonRef CreateCancelButton(GameObject parent, string name, string text, int width = 0)
        {
            return CreateButton(parent, name, text, Theme.CancelButtonColor, width);
        }

        /// <param name="width">0 表示横向撑满可用空间。</param>
        private static ButtonRef CreateButton(GameObject parent, string name, string text, Color color, int width)
        {
            ButtonRef button = UIFactory.CreateButton(parent, name, text);
            // 悬停/按下色自己算，不用 * 1.2f：那个乘法发生在 sRGB 空间，
            // 深色底乘完会往灰白跑，看着像换了个控件而不是变亮。
            RuntimeHelper.SetColorBlock(button.Component, color, Lighten(color, 0.08f), Darken(color, 0.06f));

            Text label = button.ButtonText;
            label.color = Theme.TextPrimary;
            // 深色主题下字号比字重更管用：Arial 的粗体在小字号下会糊。
            label.fontSize = Layout.FontBody;

            UIFactory.SetLayoutElement(
                button.Component.gameObject,
                minWidth: width,
                preferredWidth: width,
                minHeight: Layout.InlineButtonHeight,
                preferredHeight: Layout.InlineButtonHeight,
                flexibleWidth: width > 0 ? 0 : 9999,
                flexibleHeight: 0);

            return button;
        }

        /// <summary>
        /// 表单输入框。底色用 SurfaceSunken 而不是 UIFactory 默认的白色——
        /// 白色输入框在整套深色方案里会是最亮的一块，把视线全吸走。
        /// </summary>
        public static InputFieldRef CreateInputField(GameObject parent, string name, string placeholder, int width = 0)
        {
            InputFieldRef input = UIFactory.CreateInputField(parent, name, placeholder);
            input.Component.textComponent.color = Theme.TextPrimary;
            input.Component.textComponent.fontSize = Layout.FontBody;
            if (input.Component.placeholder is Text placeholderText)
            {
                placeholderText.color = Theme.TextSecondary;
                placeholderText.fontSize = Layout.FontBody;
            }

            Image background = input.GameObject.GetComponent<Image>();
            if (background != null)
                background.color = Theme.SurfaceSunken;

            UIFactory.SetLayoutElement(
                input.GameObject,
                minWidth: width,
                preferredWidth: width,
                minHeight: Layout.InlineButtonHeight,
                preferredHeight: Layout.InlineButtonHeight,
                flexibleWidth: width > 0 ? 0 : 9999,
                flexibleHeight: 0);

            return input;
        }

        /// <summary>
        /// 输入框左边的字段名。宽度固定，多个字段叠起来才会左右对齐；
        /// 也没有 placeholder——字段名说一遍就够了，重复说两遍是噪音。
        /// </summary>
        public static Text CreateFieldLabel(GameObject parent, string name, string text, int width = 72)
        {
            Text label = UIFactory.CreateLabel(parent, name, text, TextAnchor.MiddleLeft,
                Theme.TextSecondary, false, Layout.FontDetail);
            UIFactory.SetLayoutElement(label.gameObject, minWidth: width, preferredWidth: width,
                minHeight: 20, flexibleHeight: 0, flexibleWidth: 0);
            return label;
        }

        /// <summary>表单里的一行：左边字段名，右边控件。</summary>
        public static GameObject CreateFieldRow(GameObject parent, string name, int height = Layout.FieldRowHeight)
        {
            GameObject row = UIFactory.CreateHorizontalGroup(
                parent, name, false, false, true, true, Layout.SpaceSm,
                // UIFactory 取的是 (top, bottom, left, right)，不是直觉的 (left, top, right, bottom)。
                // 写成 (左右, 上下) 会让上边距吃掉下边距，输入框被压扁。
                new Vector4(Layout.SpaceXs, Layout.SpaceXs, Layout.SpaceSm, Layout.SpaceSm),
                Theme.SurfaceRaised);
            UIFactory.SetLayoutElement(row, minHeight: height, preferredHeight: height, flexibleHeight: 0, flexibleWidth: 9999);
            return row;
        }

        private static Color Lighten(Color color, float amount)
        {
            return new Color(
                Mathf.Min(1f, color.r + amount),
                Mathf.Min(1f, color.g + amount),
                Mathf.Min(1f, color.b + amount),
                color.a);
        }

        private static Color Darken(Color color, float amount)
        {
            return new Color(
                Mathf.Max(0f, color.r - amount),
                Mathf.Max(0f, color.g - amount),
                Mathf.Max(0f, color.b - amount),
                color.a);
        }

        #endregion
    }
}