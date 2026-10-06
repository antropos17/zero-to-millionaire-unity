using UnityEngine;using UnityEngine.UIElements;
namespace ZeroToMillionaire {
 public sealed class PixelEffects {
  readonly VisualElement root,overlay;VisualElement reveal;Image oldSurface;float revealAt,trailAt;Vector2 from,to;bool trail;
  public bool Revealing=>reveal!=null;
  public PixelEffects(VisualElement root){this.root=root;overlay=new VisualElement{name="reward-pixels",pickingMode=PickingMode.Ignore};overlay.style.position=Position.Absolute;overlay.style.left=overlay.style.top=0;overlay.style.width=overlay.style.height=Length.Percent(100);overlay.generateVisualContent+=Paint;root.Add(overlay);}
  public void Reward(VisualElement source,VisualElement destination,bool calm){if(calm||source==null||destination==null)return;from=root.WorldToLocal(source.worldBound.center);to=root.WorldToLocal(destination.worldBound.center);trailAt=Time.unscaledTime;trail=true;overlay.BringToFront();}
  public void Reveal(VisualElement element,Texture2D old,VisualElement roomStage,bool calm){ClearReveal();if(calm||element==null)return;reveal=element;reveal.style.opacity=0;revealAt=Time.unscaledTime;if(old!=null){oldSurface=new Image{image=old,scaleMode=ScaleMode.ScaleToFit,pickingMode=PickingMode.Ignore};oldSurface.style.width=oldSurface.style.height=Length.Percent(100);oldSurface.style.position=Position.Absolute;oldSurface.style.left=oldSurface.style.top=0;roomStage.Insert(0,oldSurface);}}
  public void Tick(){float now=Time.unscaledTime;if(reveal!=null){float t=Mathf.Clamp01((now-revealAt)/.22f);reveal.style.opacity=t;if(t>=1)ClearReveal();}if(trail){if(now-trailAt>.65f)trail=false;overlay.MarkDirtyRepaint();}}
  void Paint(MeshGenerationContext ctx){if(!trail)return;float t=(Time.unscaledTime-trailAt)/.65f;var p=ctx.painter2D;for(int i=0;i<4;i++){float u=Mathf.Clamp01(t-i*.09f);if(u<=0||u>=1)continue;float e=1-(1-u)*(1-u);Vector2 v=Vector2.Lerp(from,to,e);v.x+=Mathf.Sin(u*Mathf.PI)*(i%2==0?5:-5);v.x=Mathf.Round(v.x);v.y=Mathf.Round(v.y);p.fillColor=i%2==0?PixelUi.Hex(0xffb758):PixelUi.Hex(0xcaa97d);p.BeginPath();p.MoveTo(v);p.LineTo(v+new Vector2(3,0));p.LineTo(v+new Vector2(3,3));p.LineTo(v+new Vector2(0,3));p.ClosePath();p.Fill();}}
  void ClearReveal(){if(reveal!=null)reveal.style.opacity=1;reveal=null;oldSurface?.RemoveFromHierarchy();oldSurface=null;}
  public void Clear(){ClearReveal();trail=false;overlay.MarkDirtyRepaint();}
  public void Dispose(){Clear();overlay.RemoveFromHierarchy();}
 }
}
