using UnityEngine;
using UnityEngine.UIElements;

namespace ZeroToMillionaire
{
    // Small articulated cutout rig. Keep the room's original feet anchor and
    // uniform adult canvas; limb joints and contact shadows are local visuals.
    public sealed class HeroineVisual
    {
        public const float CanvasHeight=212f,CanvasWidth=CanvasHeight*2f/3f;
        const float PixelScale=CanvasHeight/192;
        public readonly VisualElement Root;
        public int Phase {get;private set;}
        readonly Image upper=new Image{scaleMode=ScaleMode.StretchToFill,pickingMode=PickingMode.Ignore};
        readonly Texture2D[] body=new Texture2D[2],leftTextures=new Texture2D[2],rightTextures=new Texture2D[2];
        readonly Leg[] legs={new Leg(58),new Leg(76)};
        Vector2 heading=new Vector2(1,.35f).normalized;
        float strength,faceWait;bool back,left;
        sealed class Leg
        {
            public readonly float X;
            public readonly Image Thigh=new Image{scaleMode=ScaleMode.StretchToFill,pickingMode=PickingMode.Ignore};
            public readonly Image Shin=new Image{scaleMode=ScaleMode.StretchToFill,pickingMode=PickingMode.Ignore};
            public readonly VisualElement Shadow=new VisualElement{pickingMode=PickingMode.Ignore};
            public Vector2 Plant,Takeoff;public bool Initialized,Stance;
            public Leg(float x){X=x;}
        }
        public HeroineVisual()
        {
            Root=new VisualElement{name="walking-heroine",pickingMode=PickingMode.Ignore};
            Root.style.position=Position.Absolute;Root.style.width=Length.Percent(CanvasWidth/512*100);Root.style.height=Length.Percent(CanvasHeight/512*100);
            for(int i=0;i<2;i++){
                string view=i==0?"front":"back";
                body[i]=Resources.Load<Texture2D>("Art/animation/"+view+"-body");
                leftTextures[i]=Resources.Load<Texture2D>("Art/animation/"+view+"-leg-left");
                rightTextures[i]=Resources.Load<Texture2D>("Art/animation/"+view+"-leg-right");
            }
            foreach(var leg in legs){
                leg.Shadow.generateVisualContent+=ctx=>{
                    var p=ctx.painter2D;float w=leg.Shadow.contentRect.width,h=leg.Shadow.contentRect.height;
                    p.fillColor=new Color32(34,29,22,48);p.BeginPath();p.MoveTo(new Vector2(w*.2f,0));p.LineTo(new Vector2(w*.8f,0));p.LineTo(new Vector2(w,h*.5f));p.LineTo(new Vector2(w*.8f,h));p.LineTo(new Vector2(w*.2f,h));p.LineTo(new Vector2(0,h*.5f));p.ClosePath();p.Fill();
                };
                Root.Add(leg.Shadow);
            }
            foreach(var leg in legs){Root.Add(leg.Thigh);Root.Add(leg.Shin);}
            Root.Add(upper);upper.style.position=Position.Absolute;upper.style.left=0;upper.style.top=0;upper.style.width=Length.Percent(100);upper.style.height=Length.Percent(112f/192*100);
            upper.sourceRect=new Rect(0,0,128,112);
            upper.style.transformOrigin=new TransformOrigin(Length.Percent(50),Length.Percent(107f/112*100),0);
        }
        Vector2 ToWorld(Vector2 source,FloorPoint feet)
        {return new Vector2(RoomGeometry.PixelX(feet)+(left?-1:1)*(source.x-64)*PixelScale,RoomGeometry.PixelY(feet)+(source.y-178)*PixelScale);}
        Vector2 ToSource(Vector2 world,FloorPoint feet)
        {return new Vector2(64+(left?-1:1)*(world.x-RoomGeometry.PixelX(feet))/PixelScale,178+(world.y-RoomGeometry.PixelY(feet))/PixelScale);}
        public void SetPose(FloorPoint feet,FloorPoint delta,bool walking,float distance)
        {
            float dt=Mathf.Clamp(Time.unscaledDeltaTime,0,.1f);
            strength=Mathf.MoveTowards(strength,walking?1:0,dt*4);
            Vector2 move=new Vector2((delta.U-delta.V)*68,(delta.U+delta.V)*100f/3);
            if(walking&&move.sqrMagnitude>.00001f)heading=Vector2.Lerp(heading,move.normalized,1-Mathf.Exp(-dt*12)).normalized;
            float cycle=Mathf.Repeat(distance/.72f,1);
            bool wantedBack=heading.y<-.22f?true:heading.y>.22f?false:back;
            bool wantedLeft=heading.x<-.14f?true:heading.x>.14f?false:left;
            if(wantedBack!=back||wantedLeft!=left){
                faceWait+=dt;
                if(strength<.2f||Mathf.Abs(Mathf.Sin(cycle*Mathf.PI*2))<.25f||faceWait>.18f){
                    back=wantedBack;left=wantedLeft;faceWait=0;foreach(var leg in legs)leg.Initialized=false;
                }
            }else faceWait=0;
            Root.style.left=Length.Percent((RoomGeometry.PixelX(feet)-CanvasWidth/2)/512*100);
            Root.style.top=Length.Percent((RoomGeometry.PixelY(feet)-178f/192*CanvasHeight)/512*100);
            Root.style.scale=new Scale(new Vector3(left?-1:1,1,1));
            int view=back?1:0;upper.image=body[view];upper.sourceRect=new Rect(0,0,128,112);
            float gait=cycle*Mathf.PI*2;
            float bob=Mathf.Sin(gait*2)*.5f*strength+Mathf.Sin(Time.unscaledTime*1.7f)*.3f*(1-strength);
            float unit=Root.resolvedStyle.height/192;
            upper.style.translate=new Translate(Mathf.Sin(gait)*.3f*strength*unit,-bob*unit,0);
            upper.style.rotate=new Rotate(new Angle(heading.x*(left?-1:1)*1.25f*strength,AngleUnit.Degree));
            Phase=walking?Mathf.FloorToInt(cycle*4):-1;
            for(int i=0;i<2;i++)DrawLeg(legs[i],i,feet,Mathf.Repeat(cycle+i*.5f,1),view);
        }
        void DrawLeg(Leg leg,int side,FloorPoint feet,float phase,int view)
        {
            Vector2 neutral=new Vector2(leg.X,171);
            bool stance=phase<.56f;
            Vector2 ahead=ToWorld(neutral,feet)+heading*(6*strength*PixelScale);
            if(!leg.Initialized){leg.Plant=ToWorld(neutral,feet);leg.Takeoff=leg.Plant;leg.Stance=stance;leg.Initialized=true;}
            if(stance&&!leg.Stance)leg.Plant=ahead;
            if(!stance&&leg.Stance)leg.Takeoff=leg.Plant;
            leg.Stance=stance;
            float swing=Mathf.Clamp01((phase-.56f)/.44f),lift=stance?0:Mathf.Sin(swing*Mathf.PI)*6*strength;
            Vector2 world=stance?leg.Plant:Vector2.Lerp(leg.Takeoff,ahead,Mathf.SmoothStep(0,1,swing));
            Vector2 foot=Vector2.Lerp(neutral,ToSource(world,feet),strength);foot.y-=lift;
            Vector2 hip=new Vector2(leg.X,107+strength*.65f);
            Vector2 v=foot-hip;float d=Mathf.Clamp(v.magnitude,.01f,63.95f);Vector2 dir=v.normalized;
            float along=(34*34-30*30+d*d)/(2*d),height=Mathf.Sqrt(Mathf.Max(0,34*34-along*along));
            float bend=heading.x*(left?-1:1)<0?-1:1;
            Vector2 knee=hip+dir*along+new Vector2(dir.y,-dir.x)*height*bend;
            Vector2 ankle=hip+dir*d;
            Texture2D texture=side==0?leftTextures[view]:rightTextures[view];
            Rect thighCrop=side==0?new Rect(52,0,15,44):new Rect(66,0,19,44);
            DrawBone(leg.Thigh,texture,thighCrop,new Vector2(leg.X-thighCrop.x,5),hip,knee);
            DrawBone(leg.Shin,texture,new Rect(0,35,128,43),new Vector2(leg.X,4),knee,ankle);
            var shadow=leg.Shadow;shadow.style.position=Position.Absolute;
            shadow.style.left=Length.Percent((foot.x-7)/128*100);shadow.style.top=Length.Percent((foot.y+lift+6)/192*100);
            shadow.style.width=Length.Percent(14f/128*100);shadow.style.height=Length.Percent(3f/192*100);shadow.style.opacity=1-lift/14;
        }
        static void DrawBone(Image image,Texture2D texture,Rect crop,Vector2 pivot,Vector2 joint,Vector2 end)
        {
            image.image=texture;image.sourceRect=crop;image.style.position=Position.Absolute;
            image.style.left=Length.Percent((joint.x-pivot.x)/128*100);image.style.top=Length.Percent((joint.y-pivot.y)/192*100);
            image.style.width=Length.Percent(crop.width/128*100);image.style.height=Length.Percent(crop.height/192*100);
            image.style.transformOrigin=new TransformOrigin(Length.Percent(pivot.x/crop.width*100),Length.Percent(pivot.y/crop.height*100),0);
            Vector2 d=end-joint;image.style.rotate=new Rotate(new Angle(Mathf.Atan2(-d.x,d.y)*Mathf.Rad2Deg,AngleUnit.Degree));
        }
    }
}
