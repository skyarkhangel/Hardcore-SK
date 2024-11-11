using UnityEngine;

namespace StorageSelector.UI.Layout
{
    public static class LayoutConstants
    {
        public const float BaseHeaderHeight = 35f;
        public const float ProductIconSize = 32f;
        public const float HeaderIconOffset = 8f;
        public const float ContentOffset = 15f;
        public const float ColumnGap = 17f;
        public const float CloseButtonMargin = 4f;

        public const float RepeatModeHeight = 200f;
        public const float StorageModeHeight = 70f;
        public const float WorkerSelectionHeight = 85f;

        public const float SectionGapBefore = 4f;
        public const float SectionGapAfter = 4f;
        public const float SectionGap = 12f;

        public static float GetTotalHeaderHeight() => BaseHeaderHeight + HeaderIconOffset;

        public static float GetColumnWidth(Rect inRect) =>
            (inRect.width - 2f * ColumnGap) / 3f;

        public static float GetContentHeight(Rect inRect) =>
            inRect.height - GetTotalHeaderHeight() - CloseButtonMargin;
    }
}
