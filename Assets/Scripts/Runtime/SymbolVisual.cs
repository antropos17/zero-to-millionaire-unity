using UnityEngine;
using UnityEngine.UIElements;
namespace ZeroToMillionaire
{
    // Optical bounds measured from the approved individual textures. Keep
    // their original pixels, aspect ratio and Point filtering; ignore padding.
    public static class SymbolVisual
    {
        public static void SetTexture(Image image, Texture2D texture)
        { image.image=texture;if(texture!=null)image.sourceRect=Bounds(texture); }
        public static Rect Bounds(Texture2D texture)
        { switch(texture.name) {
                case "cash": return new Rect(9,19,110,89);
                case "door-owned": return new Rect(34,9,59,110);
                case "door": return new Rect(36,9,56,110);
                case "floorpatch": return new Rect(9,25,110,77);
                case "lamp": return new Rect(41,9,46,110);
                case "mattress-owned": return new Rect(9,24,110,80);
                case "mattress": return new Rect(9,31,110,66);
                case "stool": return new Rect(15,9,97,110);
                case "wallpaper": return new Rect(9,13,110,101);
                case "wallrepair": return new Rect(11,9,106,110);
                case "window-owned": return new Rect(25,9,78,110);
                case "window": return new Rect(27,9,73,110);
                default:return new Rect(0,0,texture.width,texture.height);
            }
        }
    }
}
