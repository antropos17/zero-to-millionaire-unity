using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZeroToMillionaire
{
    // Live scalable pixel bevels. Geometry is painted behind real UI controls.
    public static class PixelUi
    {
        public static bool ReducedMotion;
        sealed class PressState { public float from,to,started,duration;public IVisualElementScheduledItem item; }
        static readonly ConditionalWeakTable<VisualElement,PressState> Presses=new ConditionalWeakTable<VisualElement,PressState>();
        public static float RenderedFaceTravel(VisualElement element){if(!(element is Button)||!element.enabledInHierarchy)return 0;float target=FaceTravel(element);if(ReducedMotion||!Presses.TryGetValue(element,out var state))return target;float t=Mathf.Clamp01((Time.unscaledTime-state.started)/Mathf.Max(.001f,state.duration));return Mathf.Lerp(state.from,state.to,1-Mathf.Pow(1-t,3));}
        public enum Finish { Cream, Forest, Slot, Rust, Grey, Owned, Title }
        public static Color Hex(uint rgb) { return new Color32((byte)(rgb >> 16),(byte)(rgb >> 8),(byte)rgb,255); }
        public static void CashMarker(VisualElement element)
        {
            element.style.backgroundColor=Color.clear;
            element.generateVisualContent+=ctx=>{
                var p=ctx.painter2D;var rect=new Rect(0,0,element.layout.width,element.layout.height);
                bool reached=element.ClassListContains("reached");
                Polygon(p,rect,4,reached?Hex(0x142c28):Hex(0x777168));
                Polygon(p,Inset(rect,2),2,reached?Hex(0xffb13b):Hex(0xf9edd9));
            };
        }
        public static void Skin(VisualElement element, Finish finish)
        {
            element.style.backgroundColor=Color.clear;
            element.style.borderLeftWidth=element.style.borderRightWidth=element.style.borderTopWidth=element.style.borderBottomWidth=0;
            element.style.borderTopLeftRadius=element.style.borderTopRightRadius=element.style.borderBottomLeftRadius=element.style.borderBottomRightRadius=0;
            element.generateVisualContent += context => Draw(context,element,finish);
            element.RegisterCallback<GeometryChangedEvent>(_=>{UpdateCaption(element);element.MarkDirtyRepaint();});
            element.RegisterCallback<PointerDownEvent>(_=>SetPressed(element,true),TrickleDown.TrickleDown);
            element.RegisterCallback<PointerUpEvent>(_=>SetPressed(element,false));
            element.RegisterCallback<PointerLeaveEvent>(_=>SetPressed(element,false));
            element.RegisterCallback<PointerCancelEvent>(_=>SetPressed(element,false),TrickleDown.TrickleDown);
            element.RegisterCallback<PointerCaptureOutEvent>(_=>SetPressed(element,false),TrickleDown.TrickleDown);
            element.RegisterCallback<BlurEvent>(_=>SetPressed(element,false));
            element.RegisterCallback<DetachFromPanelEvent>(_=>SetPressed(element,false));
        }
        static float Depth(VisualElement element)
        {return element is Button?(element.name=="spin"||element.name=="recovery"?10:4):2;}
        public static void SetPressed(VisualElement element,bool pressed)
        {
            float current=RenderedFaceTravel(element);
            if(pressed)element.AddToClassList("pressed");else element.RemoveFromClassList("pressed");
            if(element is Button){var state=Presses.GetValue(element,_=>new PressState());state.item?.Pause();state.from=current;state.to=FaceTravel(element);state.started=Time.unscaledTime;state.duration=pressed?.08f:.12f;
                if(!ReducedMotion&&element.panel!=null&&Mathf.Abs(state.from-state.to)>.01f)state.item=element.schedule.Execute(()=>{UpdateCaption(element);element.MarkDirtyRepaint();if(Time.unscaledTime-state.started>=state.duration)state.item?.Pause();}).Every(16);
            }
            UpdateCaption(element);element.MarkDirtyRepaint();
        }
        static void UpdateCaption(VisualElement element)
        {
            if(!(element is Button))return;
            var caption=element.Q<Label>("button-caption");if(caption==null)return;
            float depth=Depth(element),press=RenderedFaceTravel(element);
            caption.style.translate=new Translate(0,-depth/2+press,0);
        }
        // Logical target; RenderedFaceTravel reports the eased geometry used by skin and caption.
        public static float FaceTravel(VisualElement element)
        {return element.ClassListContains("pressed")&&element.enabledInHierarchy?Depth(element)/2:0;}
        static void Polygon(Painter2D p,Rect r,float corner,Color color)
        {
            if(r.width<=0||r.height<=0)return;
            float c=Mathf.Min(corner,Mathf.Min(r.width,r.height)/3);
            p.fillColor=color;p.BeginPath();p.MoveTo(new Vector2(r.x+c,r.y));p.LineTo(new Vector2(r.xMax-c,r.y));
            p.LineTo(new Vector2(r.xMax,r.y+c));p.LineTo(new Vector2(r.xMax,r.yMax-c));p.LineTo(new Vector2(r.xMax-c,r.yMax));
            p.LineTo(new Vector2(r.x+c,r.yMax));p.LineTo(new Vector2(r.x,r.yMax-c));p.LineTo(new Vector2(r.x,r.y+c));p.ClosePath();p.Fill();
        }
        static Rect Inset(Rect r,float n) { return new Rect(r.x+n,r.y+n,r.width-2*n,r.height-2*n); }
        static void Draw(MeshGenerationContext context,VisualElement element,Finish requested)
        {
            Finish kind=!element.enabledInHierarchy && requested==Finish.Rust?Finish.Grey:requested;
            var p=context.painter2D;Rect r=new Rect(0,0,element.layout.width,element.layout.height);
            bool pressed=element is Button&&element.ClassListContains("pressed")&&element.enabledInHierarchy;
            if(requested==Finish.Rust){DrawRust(p,r,element,kind==Finish.Grey,pressed);return;}
            Color outer=Hex(0x122b25),edge=Hex(0x68826a),inner=Hex(0x12352b),face=Hex(0x142c28);
            float corner=5;
            switch(kind) {
                case Finish.Cream:edge=Hex(0xfff5db);inner=Hex(0xc8a478);face=Hex(0xf3dfbb);break;
                case Finish.Rust:outer=Hex(0x38261c);edge=Hex(0xf69655);inner=Hex(0xe16a36);face=Hex(0xbd4827);corner=7;break;
                case Finish.Grey:outer=Hex(0x625c54);edge=Hex(0xb4ad9d);inner=Hex(0x807e79);face=Hex(0x72756f);break;
                case Finish.Owned:edge=Hex(0x8eac73);inner=Hex(0x426e4b);face=Hex(0x356445);break;
                case Finish.Slot:edge=Hex(0x587c65);inner=Hex(0x264c3e);face=Hex(0x142c28);corner=4;break;
                case Finish.Title:outer=Hex(0x15222b);edge=Hex(0x25333c);inner=Hex(0x19272e);face=Hex(0x19272e);break;
            }
            Polygon(p,r,corner,outer);
            Polygon(p,Inset(r,1),corner-1,edge);
            Polygon(p,Inset(r,3),corner-2,inner);
            Rect front=Inset(r,4);
            if(kind==Finish.Owned||kind==Finish.Grey||kind==Finish.Forest){front.height-=Depth(element);front.y+=RenderedFaceTravel(element);}
            Polygon(p,front,Mathf.Max(1,corner-3),face);
        }
        static void DrawRust(Painter2D p,Rect r,VisualElement element,bool disabled,bool pressed)
        {
            float depth=Depth(element),travel=RenderedFaceTravel(element),corner=element.name=="spin"||element.name=="recovery"?8:5;
            // Fixed extruded base and bottom shadow occupy the full hit rectangle.
            Polygon(p,new Rect(0,2,r.width,r.height-2),corner,disabled?Hex(0x454440):Hex(0x302017));
            Polygon(p,new Rect(2,4,r.width-4,r.height-6),corner-1,disabled?Hex(0x64635c):Hex(0x6e2b18));
            Polygon(p,new Rect(4,r.height-depth,r.width-8,depth-3),corner-3,disabled?Hex(0x77756e):Hex(0x8b341c));
            Rect front=new Rect(0,travel,r.width,r.height-depth);
            Polygon(p,front,corner,disabled?Hex(0x55554f):Hex(0x492619));
            Polygon(p,Inset(front,1),corner-1,disabled?Hex(0xbbb5a3):Hex(0xffb16d));
            Polygon(p,Inset(front,3),corner-2,disabled?Hex(0x929084):Hex(0xec703b));
            Polygon(p,Inset(front,5),corner-3,disabled?Hex(0x6c6d67):Hex(0x9e3b21));
            Rect face=Inset(front,6);
            Polygon(p,face,Mathf.Max(1,corner-4),disabled?Hex(0x777a73):Hex(0xbe4a29));
            // Crisp horizontal bands give the reference's restrained face light.
            if(face.height>15){
                int bands=24;float bandHeight=(face.height-4)/bands;
                for(int i=0;i<bands;i++){
                    Color color=disabled?Color.Lerp(Hex(0x83867c),Hex(0x73766f),i/23f):Color.Lerp(Hex(0xc5522c),Hex(0xb94728),i/23f);
                    if(pressed&&!disabled){color*=0.94f;color.a=1;}
                    Polygon(p,new Rect(face.x+2,face.y+2+i*bandHeight,face.width-4,bandHeight+0.1f),0,color);
                }
            }
        }
    }
}
