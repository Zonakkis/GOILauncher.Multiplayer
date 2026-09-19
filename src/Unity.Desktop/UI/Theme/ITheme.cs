using UnityEngine;

namespace GOILauncher.Multiplayer.UI.Theme
{
    /// <summary>
    /// 颜色 token。面板里的底色、文字色一律从这里取，不再就地写 <c>new Color(...)</c>——
    /// 散在各页面的字面量改不动，等于没有主题。
    ///
    /// 只管颜色。行高、间距、字号这些换主题也不会变的尺寸在 <see cref="Layout"/> 上，
    /// 这样加深色/浅色主题只需要再实现一遍本接口。
    /// </summary>
    public interface ITheme
    {
        // 表面：由深到浅代表"离用户多远"。Sunken 是嵌进去的（滚动视口），
        // Raised 是浮在页面上的卡片，Header 是卡片内部更亮一档的行。
        Color SurfaceSunken { get; }
        Color SurfaceBase { get; }
        Color SurfaceRaised { get; }
        Color SurfaceHeader { get; }
        Color SurfaceHover { get; }

        // 文字。正文、次要说明、错误、强调四档，一般界面够用。
        Color TextPrimary { get; }
        Color TextSecondary { get; }
        Color TextDanger { get; }
        Color TextAccent { get; }

        /// <summary>"有密码"这类需要注意但不阻断的状态标记。</summary>
        Color WarningChipColor { get; }

        /// <summary>卡片内部分隔线。不是卡片之间的边界，所以比表面色差要轻。</summary>
        Color DividerColor { get; }

        // 选中态。名字从 Tab 改成 Selection：它同时用在页面页签和弹窗里的
        // 密码模式分段控件上（保持/设置/移除），叫 Tab 会误导。
        Color SelectionActiveColor { get; }
        Color SelectionInactiveColor { get; }
        Color ConfirmButtonColor { get; }
        Color CancelButtonColor { get; }
    }
}