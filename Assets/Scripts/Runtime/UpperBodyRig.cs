using UnityEngine;
using UnityEngine.UIElements;
namespace ZeroToMillionaire
{
 public sealed class UpperBodyRig
 {
  public readonly VisualElement Root=new VisualElement{name="articulated-upper-body",pickingMode=PickingMode.Ignore};
  readonly Image torso=new Image{scaleMode=ScaleMode.StretchToFill,pickingMode=PickingMode.Ignore},head=new Image{scaleMode=ScaleMode.StretchToFill,pickingMode=PickingMode.Ignore};
  readonly Image[] upper={new Image{scaleMode=ScaleMode.StretchToFill,pickingMode=PickingMode.Ignore},new Image{scaleMode=ScaleMode.StretchToFill,pickingMode=PickingMode.Ignore}};
  readonly Image[] fore={new Image{scaleMode=ScaleMode.StretchToFill,pickingMode=PickingMode.Ignore},new Image{scaleMode=ScaleMode.StretchToFill,pickingMode=PickingMode.Ignore}};
  readonly Texture2D[,] textures=new Texture2D[2,5];int previousView=-1;
  readonly Vector2[] shoulders=new Vector2[2],elbows=new Vector2[2];
  public float LeftAngle{get;private set;} public float RightAngle{get;private set;}
  public float JointGap{get{float gap=0;for(int side=0;side<2;side++){float sx=upper[side].resolvedStyle.width/128,sy=upper[side].resolvedStyle.height/34;Vector2 a=upper[side].LocalToWorld(new Vector2(elbows[side].x*sx,(elbows[side].y-55)*sy));float fx=fore[side].resolvedStyle.width/128,fy=fore[side].resolvedStyle.height/25;Vector2 b=fore[side].LocalToWorld(new Vector2(elbows[side].x*fx,(elbows[side].y-87)*fy));gap=Mathf.Max(gap,Vector2.Distance(a,b));}return gap;}}
  public UpperBodyRig(){Root.style.position=Position.Absolute;Root.style.left=0;Root.style.top=0;Root.style.width=Length.Percent(100);Root.style.height=Length.Percent(100);Root.style.transformOrigin=new TransformOrigin(Length.Percent(50),Length.Percent(107f/192*100),0);
   string[] parts={"torso","upper-left","fore-left","upper-right","fore-right"};for(int v=0;v<2;v++)for(int i=0;i<5;i++){textures[v,i]=Resources.Load<Texture2D>("Art/animation/"+(v==0?"front":"back")+"-rig-"+parts[i]);if(textures[v,i]==null)throw new System.InvalidOperationException("Missing2D arm texture: "+parts[i]);}
   Root.Add(upper[1]);Root.Add(fore[1]);Root.Add(torso);Root.Add(upper[0]);Root.Add(fore[0]);Root.Add(head);
   SetRect(torso,0,54,128,58);SetRect(head,0,0,128,55);head.style.transformOrigin=new TransformOrigin(Length.Percent(50),Length.Percent(52f/55*100),0);
  }
  static void SetRect(Image image,float x,float y,float width,float height){image.style.position=Position.Absolute;image.style.left=Length.Percent(x/128*100);image.style.top=Length.Percent(y/192*100);image.style.width=Length.Percent(width/128*100);image.style.height=Length.Percent(height/192*100);}
  static Vector2 Rotate(Vector2 v,float degrees){float a=degrees*Mathf.Deg2Rad;return new Vector2(v.x*Mathf.Cos(a)-v.y*Mathf.Sin(a),v.x*Mathf.Sin(a)+v.y*Mathf.Cos(a));}
  static void Bone(Image image,Texture2D texture,int top,int rows,Vector2 joint,Vector2 end,Vector2 sourceJoint,Vector2 sourceEnd){image.image=texture;image.sourceRect=new Rect(0,top,128,rows);SetRect(image,joint.x-sourceJoint.x,joint.y-(sourceJoint.y-top),128,rows);image.style.transformOrigin=new TransformOrigin(Length.Percent(sourceJoint.x/128*100),Length.Percent((sourceJoint.y-top)/rows*100),0);var d=end-joint;var original=sourceEnd-sourceJoint;image.style.rotate=new Rotate(new Angle((Mathf.Atan2(-d.x,d.y)-Mathf.Atan2(-original.x,original.y))*Mathf.Rad2Deg,AngleUnit.Degree));}
  public void Pose(int view,int headView,float gait,float strength,float bob,float lean,float headTurn,bool mirrorHead){
   torso.image=textures[view,0];torso.sourceRect=new Rect(0,54,128,58);head.image=textures[headView,0];head.sourceRect=new Rect(0,0,128,55);head.style.scale=new Scale(new Vector3(mirrorHead?-1:1,1,1));head.style.rotate=new Rotate(new Angle(headTurn,AngleUnit.Degree));
   float unit=Root.resolvedStyle.height/192;Root.style.translate=new Translate(Mathf.Sin(gait)*.3f*strength*unit,-bob*unit,0);Root.style.rotate=new Rotate(new Angle(lean,AngleUnit.Degree));
   LeftAngle=-Mathf.Sin(gait)*5.5f*strength;RightAngle=-LeftAngle;
   for(int side=0;side<2;side++){
    Vector2 shoulder=view==0?(side==0?new Vector2(52,60):new Vector2(84,61)):(side==0?new Vector2(53,62):new Vector2(83,62));
    Vector2 elbow=view==0?(side==0?new Vector2(50,87):new Vector2(88,88)):(side==0?new Vector2(51,86):new Vector2(86,88));
    Vector2 wrist=view==0?(side==0?new Vector2(49,106):new Vector2(89,106)):(side==0?new Vector2(49,102):new Vector2(89,107));
    float angle=side==0?LeftAngle:RightAngle,lower=angle+Mathf.Sin(gait+(side==0?0:Mathf.PI)+.25f)*2.5f*strength;Vector2 movedElbow=shoulder+Rotate(elbow-shoulder,angle),hand=movedElbow+Rotate(wrist-elbow,lower);
    Bone(upper[side],textures[view,side==0?1:3],55,34,shoulder,movedElbow,shoulder,elbow);Bone(fore[side],textures[view,side==0?2:4],87,25,movedElbow,hand,elbow,wrist);shoulders[side]=shoulder;elbows[side]=elbow;
   }
   if(previousView!=view){int near=view==0?0:1,far=1-near;upper[far].BringToFront();fore[far].BringToFront();torso.BringToFront();upper[near].BringToFront();fore[near].BringToFront();head.BringToFront();previousView=view;}
  }
 }
}
