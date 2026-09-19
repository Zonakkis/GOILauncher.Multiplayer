using UnityEngine;

namespace GOILauncher.Multiplayer.UI.Theme
{
    /// <summary>
    /// 换主题也不会变的尺寸：行高、间距、列宽、字号。放静态类而不是 ITheme 成员，
    /// 是因为它没有"深色版/浅色版"之分，跟着主题接口走只会让每个实现重复抄一遍。
    ///
    /// 列宽尤其是重点：表头和数据行必须引用同一份常量。以前两处各写一组数字，
    /// 改一处漏一处就错位（见 PlayerListHandler 里那段"格子必须留着"的注释）。
    /// </summary>
    public static class Layout
    {
        public const int SpaceXs = 4;
        public const int SpaceSm = 6;
        public const int SpaceMd = 8;
        public const int SpaceLg = 12;

        public const int SectionHeaderHeight = 32;

        /// <summary>
        /// 表格单元格的最小高度。低于这个值文字就被切掉，所以表头行高必须留得下它——
        /// 见 <see cref="TableHeaderHeight"/>。改这里之前先看 UiKit.CreateTextCell。
        /// </summary>
        public const int TableCellHeight = 20;

        /// <summary>
        /// 表头行高 = 单元格最小高度 + 上下各一份 SpaceXs 内边距，正好放得下，不多不少。
        /// 写成算式而不是 28 这样的字面量：改 SpaceXs 时这里会跟着走，不会再出现
        /// "表头比单元格矮，字被切掉一半"的错位。
        /// </summary>
        public const int TableHeaderHeight = TableCellHeight + SpaceXs * 2;

        public const int RoomRowHeight = 30;
        public const int PrimaryButtonHeight = 34;
        public const int InlineButtonHeight = 24;

        /// <summary>
        /// 表单一行的高度 = 输入框高度 + 上下各一份 <see cref="SpaceXs"/> 内边距。
        /// 和 <see cref="TableHeaderHeight"/> 同一个道理：行高是输入框的预算，
        /// 定得比输入框矮，框就会被压扁（见 <see cref="UiKit.CreateFieldRow"/>）。
        /// </summary>
        public const int FieldRowHeight = InlineButtonHeight + SpaceXs * 2;

        public const int FontBody = 14;
        public const int FontSection = 15;
        public const int FontTableHeader = 12;
        public const int FontDetail = 13;

        /// <summary>房间目录的列宽。表头和数据行同时引用这里，别在一边单独写字面量。</summary>
        public static class RoomColumns
        {
            /// <summary>状态格（有密码 / 已满 / 当前房间），固定宽，居中。</summary>
            public const int StatusWidth = 84;

            /// <summary>人数格，固定宽，居中。</summary>
            public const int CountWidth = 84;

            /// <summary>操作格，固定宽。里面只有一个按钮。</summary>
            public const int ActionWidth = 72;

            /// <summary>行左右内边距。滚动内容自身另有 SpaceXs，加起来正好对齐表头。</summary>
            public const int RowPadding = 8;
        }
    }
}