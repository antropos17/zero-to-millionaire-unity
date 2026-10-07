using UnityEngine;using UnityEngine.UIElements;
namespace ZeroToMillionaire {
 // Presentation only. Every effect starts after the transaction is saved.
 public sealed class PixelEffects {
  readonly VisualElement root,overlay;VisualElement reveal;Image oldSurface;float revealAt,trailAt,holdUntil,dustAt;Vector2 from,to,dustOrigin;bool trail,dust,settleFurniture;
  public bool Revealing=>reveal!=null;
  public bool PresentationBusy=>Revealing||Time.unscaledTime<holdUntil;
  public PixelEffects(VisualElement root){this.root=root;overlay=new VisualElement{name="reward-pixels",pickingMode=PickingMode.Ignore};overlay.style.position=Position.Absolute;overlay.style.left=overlay.style.top=0;overlay.style.width=overlay.style.height=Length.Percent(100);overlay.generateVisualContent+=Paint;root.Add(overlay);}
  public void Reward(VisualElement source,VisualElement destination,bool calm){if(calm||source==null||destination==null)return;from=root.WorldToLocal(source.worldBound.center);to=root.WorldToLocal(destination.worldBound.center);trailAt=Time.unscaledTime;trail=true;overlay.BringToFront();}
  public void Reveal(VisualElement element,Texture2D old,VisualElement roomStage,bool calm,bool furniture=false){ClearReveal();dust=false;holdUntil=0;if(calm||element==null){holdUntil=Time.unscaledTime+(calm?.20f:.65f);return;}reveal=element;settleFurniture=furniture;reveal.style.opacity=0;revealAt=Time.unscaledTime;
   if(old!=null){oldSurface=new Image{image=old,scaleMode=ScaleMode.ScaleToFit,pickingMode=PickingMode.Ignore};oldSurface.style.width=oldSurface.style.height=Length.Percent(100);oldSurface.style.position=Position.Absolute;oldSurface.style.left=oldSurface.style.top=0;roomStage.Insert(0,oldSurface);}
   // Resolve the rebuilt room layout before anchoring the soft installation dust.
   element.schedule.Execute(()=>{if(reveal!=element||!UiInputCoordinator.IsShown(element))return;dustOrigin=root.WorldToLocal(element.worldBound.center);if(!furniture)dustOrigin.y+=element.worldBound.height*.18f;dustAt=Time.unscaledTime;dust=true;overlay.BringToFront();});
  }
  public void Tick(){bool repaint=trail||dust;float now=Time.unscaledTime;if(reveal!=null){float t=Mathf.Clamp01((now-revealAt)/.22f),ease=1-Mathf.Pow(1-t,3);reveal.style.opacity=ease;if(settleFurniture){reveal.style.translate=new Translate(0,5*(1-ease),0);reveal.style.scale=new Scale(new Vector3(.96f+.04f*ease,.96f+.04f*ease,1));}if(t>=1){ClearReveal();holdUntil=now+.65f;}}if(trail&&now-trailAt>.65f)trail=false;if(dust&&now-dustAt>.42f)dust=false;if(repaint)overlay.MarkDirtyRepaint();}
  static void Square(Painter2D p,Vector2 v,float size,Color color){v=new Vector2(Mathf.Round(v.x),Mathf.Round(v.y));p.fillColor=color;p.BeginPath();p.MoveTo(v);p.LineTo(v+new Vector2(size,0));p.LineTo(v+new Vector2(size,size));p.LineTo(v+new Vector2(0,size));p.ClosePath();p.Fill();}
  void Paint(MeshGenerationContext ctx){var p=ctx.painter2D;
   if(trail){float t=(Time.unscaledTime-trailAt)/.65f;for(int i=0;i<8;i++){float u=Mathf.Clamp01(t-i*.045f);if(u<=0||u>=1)continue;float e=1-(1-u)*(1-u);Vector2 v=Vector2.Lerp(from,to,e);v.x+=Mathf.Sin(u*Mathf.PI)*(i%2==0?10:-10);v.y-=Mathf.Sin(u*Mathf.PI)*16;Square(p,v,i%3==0?4:3,i%2==0?PixelUi.Hex(0xffb758):PixelUi.Hex(0xcaa97d));}}
   if(dust){float t=Mathf.Clamp01((Time.unscaledTime-dustAt)/.42f);for(int i=0;i<6;i++){float angle=(i+.5f)*Mathf.PI/3;Vector2 v=dustOrigin+new Vector2(Mathf.Cos(angle)*18*t,Mathf.Sin(angle)*8*t-5*t);Color c=PixelUi.Hex(0xcaa97d);c.a=.7f*(1-t);Square(p,v,2,c);}}
  }
  void ClearReveal(){if(reveal!=null){reveal.style.opacity=1;reveal.style.translate=new Translate(0,0,0);reveal.style.scale=new Scale(Vector3.one);}reveal=null;settleFurniture=false;oldSurface?.RemoveFromHierarchy();oldSurface=null;}
  public void Clear(){ClearReveal();holdUntil=0;trail=dust=false;overlay.MarkDirtyRepaint();}
  public void Dispose(){Clear();overlay.RemoveFromHierarchy();}
 }
}
