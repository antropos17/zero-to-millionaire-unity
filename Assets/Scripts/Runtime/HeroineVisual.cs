using UnityEngine;
using UnityEngine.UIElements;

namespace ZeroToMillionaire
{
    // Generated neutral art becomes a small two-leg rig. Opposing leg rotations
    // follow distance continuously, keeping the head, pelvis and facing stable.
    public sealed class HeroineVisual
    {
        public const float CanvasHeight=212f,CanvasWidth=CanvasHeight*2f/3f;
        public readonly VisualElement Root;
        public int Phase { get; private set; }
        readonly Image upper = new Image { scaleMode = ScaleMode.StretchToFill };
        readonly Image leftLeg = new Image { scaleMode = ScaleMode.StretchToFill };
        readonly Image rightLeg = new Image { scaleMode = ScaleMode.StretchToFill };
        readonly Texture2D[] bodies = new Texture2D[2], leftLegs = new Texture2D[2], rightLegs = new Texture2D[2];
        readonly Texture2D[] idle = new Texture2D[2];
        readonly Image standing = new Image { scaleMode = ScaleMode.StretchToFill };
        bool back, left;
        public HeroineVisual()
        {
            Root = new VisualElement { name = "walking-heroine", pickingMode = PickingMode.Ignore };
            Root.style.position = Position.Absolute;
            Root.style.width = Length.Percent(CanvasWidth / 512 * 100); Root.style.height = Length.Percent(CanvasHeight / 512 * 100);
            for (int view = 0; view < 2; view++)
            {
                string name = view == 0 ? "front" : "back";
                bodies[view] = Resources.Load<Texture2D>("Art/animation/" + name + "-body");
                leftLegs[view] = Resources.Load<Texture2D>("Art/animation/" + name + "-leg-left");
                rightLegs[view] = Resources.Load<Texture2D>("Art/animation/" + name + "-leg-right");
                idle[view] = Resources.Load<Texture2D>("Art/animation/" + name + "-idle");
            }
            SetPartLayout(upper, 0, 100);
            SetPartLayout(leftLeg, 102f / 192 * 100, 90f / 192 * 100);
            SetPartLayout(rightLeg, 102f / 192 * 100, 90f / 192 * 100);
            leftLeg.style.transformOrigin = new TransformOrigin(Length.Percent(56f / 128 * 100), Length.Percent(0), 0);
            rightLeg.style.transformOrigin = new TransformOrigin(Length.Percent(76f / 128 * 100), Length.Percent(0), 0);
            SetPartLayout(standing, 0, 100);
            Root.Add(leftLeg); Root.Add(rightLeg); Root.Add(upper); Root.Add(standing);
            upper.pickingMode = leftLeg.pickingMode = rightLeg.pickingMode = standing.pickingMode = PickingMode.Ignore;
        }
        static void SetPartLayout(VisualElement element, float top, float height)
        {
            element.style.position = Position.Absolute; element.style.left = 0;
            element.style.top = Length.Percent(top); element.style.width = Length.Percent(100); element.style.height = Length.Percent(height);
        }
        public void SetPose(FloorPoint feet, FloorPoint delta, bool walking, float distance)
        {
            float dx = (delta.U - delta.V) * 68, dy = (delta.U + delta.V) * 100f / 3;
            if (walking && Mathf.Abs(dx) + Mathf.Abs(dy) > 0.0001f) { back = dy < 0; left = dx < 0; }
            Root.style.left = Length.Percent((RoomGeometry.PixelX(feet) - CanvasWidth/2) / 512 * 100);
            Root.style.top = Length.Percent((RoomGeometry.PixelY(feet) - 178f / 192 * CanvasHeight) / 512 * 100);
            Root.style.scale = new Scale(new Vector3(left ? -1 : 1, 1, 1));
            int view = back ? 1 : 0;
            Phase = walking ? Mathf.FloorToInt(distance / 0.72f * 4) % 4 : -1;
            bool ready = bodies[view] != null && leftLegs[view] != null && rightLegs[view] != null;
            upper.style.display = leftLeg.style.display = rightLeg.style.display = walking && ready ? DisplayStyle.Flex : DisplayStyle.None;
            standing.style.display = walking && ready ? DisplayStyle.None : DisplayStyle.Flex;
            upper.image = bodies[view]; leftLeg.image = leftLegs[view]; rightLeg.image = rightLegs[view];
            float gait = Mathf.Sin(distance / 0.72f * Mathf.PI * 2);
            leftLeg.style.rotate = new Rotate(new Angle(gait * 8, AngleUnit.Degree));
            rightLeg.style.rotate = new Rotate(new Angle(-gait * 8, AngleUnit.Degree));
            leftLeg.style.top = Length.Percent((102 - Mathf.Max(0, gait) * 2) / 192 * 100);
            rightLeg.style.top = Length.Percent((102 - Mathf.Max(0, -gait) * 2) / 192 * 100);
            standing.image = idle[view] != null ? idle[view] : Resources.Load<Texture2D>("Art/objects/heroine");
        }
    }
}
