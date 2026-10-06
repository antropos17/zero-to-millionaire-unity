using UnityEngine;
using UnityEngine.UIElements;
namespace ZeroToMillionaire
{
    // Cut the two complete shoes along their actual diagonal separation in
    // the neutral source. A vertical half-image retained the other shoe's heel.
    // This draws the original pixels; no generated art is painted or replaced.
    public sealed class MaskedShin : VisualElement
    {
        Texture2D texture;bool back,right;
        const int SourceTop=137,Rows=43;
        public MaskedShin(){pickingMode=PickingMode.Ignore;generateVisualContent+=Paint;}
        public void SetSource(Texture2D image,bool rear,bool rightLeg)
        {if(texture==image&&back==rear&&right==rightLeg)return;texture=image;back=rear;right=rightLeg;MarkDirtyRepaint();}
        static float Split(float y,bool rear)
        {return Mathf.Round(rear?66-Mathf.Max(0,y-158)*.25f:66+Mathf.Max(0,y-160)*.35f);}
        void Paint(MeshGenerationContext context)
        {
            if(texture==null||contentRect.width<=0||contentRect.height<=0)return;
            var mesh=context.Allocate(Rows*4,Rows*6,texture);Rect uv=mesh.uvRegion;
            float w=contentRect.width,h=contentRect.height;
            for(int row=0;row<Rows;row++){
                float y=SourceTop+row,boundary=Split(y+.5f,back),x0=right?boundary:0,x1=right?128:boundary;
                float u0=uv.xMin+x0/128*uv.width,u1=uv.xMin+x1/128*uv.width;
                float v0=uv.yMin+(1-y/192)*uv.height,v1=uv.yMin+(1-(y+1)/192)*uv.height;
                float top=row/(float)Rows*h,bottom=(row+1)/(float)Rows*h;
                mesh.SetNextVertex(new Vertex{position=new Vector3(x0/128*w,top,Vertex.nearZ),tint=Color.white,uv=new Vector2(u0,v0)});
                mesh.SetNextVertex(new Vertex{position=new Vector3(x1/128*w,top,Vertex.nearZ),tint=Color.white,uv=new Vector2(u1,v0)});
                mesh.SetNextVertex(new Vertex{position=new Vector3(x1/128*w,bottom,Vertex.nearZ),tint=Color.white,uv=new Vector2(u1,v1)});
                mesh.SetNextVertex(new Vertex{position=new Vector3(x0/128*w,bottom,Vertex.nearZ),tint=Color.white,uv=new Vector2(u0,v1)});
                ushort n=(ushort)(row*4);mesh.SetNextIndex(n);mesh.SetNextIndex((ushort)(n+1));mesh.SetNextIndex((ushort)(n+2));mesh.SetNextIndex(n);mesh.SetNextIndex((ushort)(n+2));mesh.SetNextIndex((ushort)(n+3));
            }
        }
    }
}
