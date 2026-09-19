using UnityEngine;
using UniverseLib.UI;

namespace GOILauncher.Multiplayer.UI.Theme
{
    public class DarkTheme : ITheme
    {
        // 表面色由深到浅：Sunken 嵌进去，Raised 浮起来，Header 再亮一档。
        // 三档之间的差值是有意做小的——深色区间里人眼对亮度差的感知被压缩，
        // 相差 0.04 就已经能看出层次，再拉大反而显得脏。
        public Color SurfaceSunken { get; } = new Color(0.09f, 0.09f, 0.09f);
        public Color SurfaceBase { get; } = new Color(0.12f, 0.12f, 0.12f);
        public Color SurfaceRaised { get; } = new Color(0.16f, 0.16f, 0.16f);
        public Color SurfaceHeader { get; } = new Color(0.20f, 0.20f, 0.20f);
        public Color SurfaceHover { get; } = new Color(0.22f, 0.22f, 0.22f);

        // 正文不用纯白：在 SurfaceBase 上对比度过高会刺眼，而且会和表头用的
        // TextSecondary 失去区分（两者至少要拉开 0.15）。
        public Color TextPrimary { get; } = new Color(0.92f, 0.92f, 0.92f);

        // 次要文字同时承担"禁用"/"空"/"占位"三种弱化语义，0.62 在深底上仍可读。
        public Color TextSecondary { get; } = new Color(0.62f, 0.62f, 0.62f);
        public Color TextDanger { get; } = new Color(1f, 0.65f, 0.5f);
        public Color TextAccent { get; } = new Color(0.55f, 0.78f, 0.95f);
        public Color WarningChipColor { get; } = new Color(0.85f, 0.7f, 0.35f);

        // 分隔线画在卡片内部（表头与数据行之间），所以要比 Header 亮得明显才看得出来；
        // 但也不能亮到抢过文字。0.30 在 0.20 的表头上是一条看得见但不吵的细线。
        public Color DividerColor { get; } = new Color(0.30f, 0.30f, 0.30f);

        public Color SelectionActiveColor => UniversalUI.EnabledButtonColor;
        public Color SelectionInactiveColor => UniversalUI.DisabledButtonColor;
        public Color ConfirmButtonColor { get; } = new Color(0.33f, 0.5f, 0.33f);
        public Color CancelButtonColor { get; } = new Color(0.3f, 0.2f, 0.2f);
    }
}