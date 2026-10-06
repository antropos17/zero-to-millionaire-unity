using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
namespace ZeroToMillionaire
{
    // Presentation only: the caller supplies a disk-committed grid. Never rolls or pays.
    public sealed class ReelMotion
    {
        readonly Image[] originals; readonly Texture2D[] textures;
        readonly VisualElement[] strips = new VisualElement[15], highlights = new VisualElement[15];
        readonly Image[] above = new Image[15], below = new Image[15];
        readonly int[] initialSymbols=new int[15];readonly float[] steps = new float[15]; readonly bool[] landed = new bool[5];
        int[] finalGrid; bool skip;
        public string Phase { get; private set; } = "idle";
        public bool Active { get; private set; }
        public ReelMotion(Image[] images, Texture2D[] symbols) { originals=images; textures=symbols; }
        public void Skip() { if(Active)skip=true; }
        public IEnumerator Play(SpinResult result, bool reduced, Action finalBoard, Action<float> rewardProgress)
        {
            Clear();skip=false;Active=true;finalGrid=result.Grid;return Animate(result,reduced,finalBoard,rewardProgress);
        }
        IEnumerator Animate(SpinResult result,bool reduced,Action finalBoard,Action<float> rewardProgress)
        {
            Phase="travel";
            Array.Clear(landed,0,landed.Length);
            if(!reduced)Build();
            float start=Time.unscaledTime;
            if(!reduced)while(!skip&&Time.unscaledTime-start<1.06f) {
                float elapsed=Time.unscaledTime-start;
                for(int column=0;column<5;column++) {
                    float local=elapsed-column*.08f;
                    for(int row=0;row<3;row++){int cell=row*5+column;
                        if(local<0)continue;
                        if(local<.60f) {
                            float t=Mathf.Clamp01(local/.60f),cycles=6*t*t*(3-2*t);int cycle=Mathf.Min(5,Mathf.FloorToInt(cycles));float offset=(cycles-cycle)*steps[cell];
                            int oldSymbol=cycle==0?initialSymbols[cell]:result.Pool[(cell+cycle)%result.Pool.Length];int nextSymbol=cycle==5?result.Grid[cell]:result.Pool[(cell+cycle+1)%result.Pool.Length];
                            Set(above[cell],oldSymbol,offset);Set(below[cell],nextSymbol,offset-steps[cell]);
                        } else {
                            float settle=Mathf.Clamp01((local-.60f)/.14f);float offset=Mathf.Round(Mathf.Sin(settle*Mathf.PI)*2);
                            Set(above[cell],result.Grid[cell],offset);below[cell].style.visibility=Visibility.Hidden;
                        }
                    }
                    if(local>=.74f&&!landed[column]){landed[column]=true;for(int row=0;row<3;row++)Land(row*5+column);}
                }
                yield return null;
            }
            for(int i=0;i<15;i++)Land(i);Phase="match";finalBoard();
            if(!skip&&!reduced){Emphasize(result);float matchStart=Time.unscaledTime;while(!skip&&Time.unscaledTime-matchStart<.30f)yield return null;}
            RemoveHighlights();Phase="reward";
            float rewardStart=Time.unscaledTime,duration=reduced?.16f:.60f;
            while(!skip&&Time.unscaledTime-rewardStart<duration){float t=Mathf.Clamp01((Time.unscaledTime-rewardStart)/duration);rewardProgress(1-Mathf.Pow(1-t,3));yield return null;}
            rewardProgress(1);Clear();Active=false;Phase="idle";
        }
        void Build(){for(int i=0;i<15;i++){
            initialSymbols[i]=Mathf.Max(0,Array.IndexOf(textures,originals[i].image as Texture2D));var parent=originals[i].parent;var layer=new VisualElement{name="reel-strip-"+i,pickingMode=PickingMode.Ignore};layer.style.position=Position.Absolute;layer.style.left=parent.resolvedStyle.paddingLeft;layer.style.top=parent.resolvedStyle.paddingTop;layer.style.width=parent.contentRect.width;layer.style.height=parent.contentRect.height;layer.style.overflow=Overflow.Hidden;
            float w=Mathf.Min(44,parent.contentRect.width),h=Mathf.Min(44,parent.contentRect.height);steps[i]=Mathf.Max(20,parent.contentRect.height);
            above[i]=Moving(w,h,parent.contentRect);below[i]=Moving(w,h,parent.contentRect);layer.Add(above[i]);layer.Add(below[i]);parent.Add(layer);strips[i]=layer;originals[i].style.visibility=Visibility.Hidden;
            SymbolVisual.SetTexture(above[i],originals[i].image as Texture2D);above[i].style.translate=new Translate(0,0,0);below[i].style.visibility=Visibility.Hidden;
        }}
        static Image Moving(float w,float h,Rect space){var image=new Image{scaleMode=ScaleMode.ScaleToFit,pickingMode=PickingMode.Ignore};image.style.position=Position.Absolute;image.style.left=(space.width-w)/2;image.style.top=(space.height-h)/2;image.style.width=w;image.style.height=h;return image;}
        void Set(Image image,int symbol,float offset){if(image==null)return;SymbolVisual.SetTexture(image,textures[symbol]);image.style.visibility=Visibility.Visible;image.style.translate=new Translate(0,Mathf.Round(offset),0);}
        void Land(int i){if(finalGrid!=null)SymbolVisual.SetTexture(originals[i],textures[finalGrid[i]]);originals[i].style.visibility=Visibility.Visible;strips[i]?.RemoveFromHierarchy();strips[i]=null;}
        void Emphasize(SpinResult result){for(int i=0;i<15;i++){
            int column=i%5;bool triple=result.Grid[column]==result.Grid[column+5]&&result.Grid[column]==result.Grid[column+10];bool fragment=result.Grid[i]>0&&result.Fragments[result.Grid[i]-1]>0;
            if(!triple&&!fragment)continue;var glow=new VisualElement{name="reel-match-"+i,pickingMode=PickingMode.Ignore};glow.style.position=Position.Absolute;glow.style.left=glow.style.right=3;glow.style.top=glow.style.bottom=3;glow.style.borderLeftWidth=glow.style.borderRightWidth=glow.style.borderTopWidth=glow.style.borderBottomWidth=2;var color=triple?PixelUi.Hex(0xffb758):PixelUi.Hex(0xd7e6ba);glow.style.borderLeftColor=glow.style.borderRightColor=glow.style.borderTopColor=glow.style.borderBottomColor=color;originals[i].parent.Add(glow);highlights[i]=glow;
        }}
        void RemoveHighlights(){for(int i=0;i<15;i++){highlights[i]?.RemoveFromHierarchy();highlights[i]=null;}}
        public void Clear(){RemoveHighlights();for(int i=0;i<15;i++){strips[i]?.RemoveFromHierarchy();strips[i]=null;originals[i].style.visibility=Visibility.Visible;}Active=false;Phase="idle";}
    }
}
