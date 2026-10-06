using UnityEngine;
using UnityEngine.UIElements;

namespace ZeroToMillionaire
{
    public static class UiTypography
    {
        public const int Title = 16, Meta = 12, Counter = 13, Body = 15;
        public const int DialogueBody = 14, SmallAction = 13;
        static Font font;
        public static Font Font => font != null ? font : (font = Resources.Load<Font>("Fonts/PixelifySans"));

        public static void Apply(VisualElement element, int size, Color color, bool emphasis)
        {
            element.style.unityFont = Font;
            element.style.unityFontDefinition = FontDefinition.FromFont(Font);
            element.style.fontSize = size;
            element.style.color = color;
            element.style.unityFontStyleAndWeight = emphasis ? FontStyle.Bold : FontStyle.Normal;
        }
    }
}
