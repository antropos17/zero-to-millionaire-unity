using UnityEngine;
using UnityEngine.UIElements;

namespace ZeroToMillionaire
{
    public static class UiTypography
    {
        public const int Title = 16, Meta = 12, Counter = 13, Body = 15;
        public const int DialogueBody = 15, SmallAction = 14;
        static Font font, emphasisFont, brandFont;
        public static Font Font => font != null ? font : (font = Resources.Load<Font>("Fonts/ZTMReadable-Regular"));
        public static Font EmphasisFont => emphasisFont != null ? emphasisFont : (emphasisFont = Resources.Load<Font>("Fonts/ZTMReadable-Semibold"));
        public static Font BrandFont => brandFont != null ? brandFont : (brandFont = Resources.Load<Font>("Fonts/PixelifySans"));

        public static void Apply(VisualElement element, int size, Color color, bool emphasis)
        {
            var selected = emphasis ? EmphasisFont : Font;
            element.style.unityFont = selected;
            element.style.unityFontDefinition = FontDefinition.FromFont(selected);
            element.style.fontSize = size;
            element.style.color = color;
            element.style.unityFontStyleAndWeight = FontStyle.Normal;
        }
        public static void Brand(VisualElement element)
        {
            element.style.unityFont = BrandFont;
            element.style.unityFontDefinition = FontDefinition.FromFont(BrandFont);
        }
    }
}
