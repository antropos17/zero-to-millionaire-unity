using System;
using System.Collections.Generic;

namespace ZeroToMillionaire
{
    public struct FloorPoint
    {
        public readonly float U, V;
        public FloorPoint(float u, float v) { U = u; V = v; }
        public static float Distance(FloorPoint a, FloorPoint b)
        { float u = a.U - b.U, v = a.V - b.V; return (float)Math.Sqrt(u * u + v * v); }
        public static FloorPoint Towards(FloorPoint from, FloorPoint to, float distance)
        {
            float length = Distance(from, to);
            if (length <= distance || length < 0.000001f) return to;
            return new FloorPoint(from.U + (to.U - from.U) * distance / length,
                                  from.V + (to.V - from.V) * distance / length);
        }
    }
    public struct FloorObstacle
    {
        public readonly string Name;
        public readonly float U0, V0, U1, V1;
        public FloorObstacle(string name, float u0, float v0, float u1, float v1)
        { Name = name; U0 = u0; V0 = v0; U1 = u1; V1 = v1; }
        public bool Contains(FloorPoint p, float padding)
        { return p.U >= U0 - padding && p.U <= U1 + padding && p.V >= V0 - padding && p.V <= V1 + padding; }
    }
    public static class RoomGeometry
    {
        public const float Metres = 3f, FootRadius = 0.34f;
        public const float MaximumLegacyRelocation=0.75f;
        public static readonly FloorPoint StoolAnchor=new FloorPoint(1.90f,2.30f);
        // Reserve placement zones before purchase too: buying furniture never
        // spawns it through an occupant or forces an abrupt relocation.
        public static readonly FloorObstacle[] Obstacles = {
            new FloorObstacle("Door swing", 1.75f, 0f, 2.90f, 0.70f),
            new FloorObstacle("Mattress", 0.08f, 0.95f, 0.85f, 2.68f),
            new FloorObstacle("Lamp base", 0.28f, 0.35f, 0.55f, 0.65f),
            new FloorObstacle("Stool", 1.75f, 2.15f, 2.05f, 2.45f)
        };
        // Schema1..3 used the original stool reserve. Validate that layout
        // before migrating coordinates; never discard otherwise valid progress.
        static readonly FloorObstacle[] LegacyObstacles={
            new FloorObstacle("Door swing",1.75f,0f,2.90f,.70f),new FloorObstacle("Mattress",.08f,.95f,.85f,2.68f),
            new FloorObstacle("Lamp base",.28f,.35f,.55f,.65f),new FloorObstacle("Stool",1.08f,2.30f,1.38f,2.60f)};
        public static bool IsFreeForSave(FloorPoint p,int schema){return IsFreeIn(p,schema<4?LegacyObstacles:Obstacles);}
        public static FloorPoint ProjectLegacyFeet(FloorPoint point){
            if(!IsFreeForSave(point,3))throw new ArgumentException("Invalid legacy feet");if(IsFree(point))return point;
            var xs=new List<float>{point.U,FootRadius,Metres-FootRadius};var ys=new List<float>{point.V,FootRadius,Metres-FootRadius};
            foreach(var obstacle in Obstacles){xs.Add(Math.Max(FootRadius,obstacle.U0-FootRadius-.001f));xs.Add(Math.Min(Metres-FootRadius,obstacle.U1+FootRadius+.001f));ys.Add(Math.Max(FootRadius,obstacle.V0-FootRadius-.001f));ys.Add(Math.Min(Metres-FootRadius,obstacle.V1+FootRadius+.001f));}
            FloorPoint best=point;float bestDistance=float.MaxValue;
            foreach(float x in xs)foreach(float y in ys){var candidate=new FloorPoint(x,y);if(!IsFree(candidate))continue;float distance=FloorPoint.Distance(point,candidate);if(distance<bestDistance-0.000001f||(Math.Abs(distance-bestDistance)<0.000001f&&(x<best.U||(x==best.U&&y<best.V)))){best=candidate;bestDistance=distance;}}
            if(bestDistance>MaximumLegacyRelocation)throw new InvalidOperationException("Legacy relocation exceeded bounded policy");return best;
        }
        public static float PixelX(FloorPoint p) { return 256 + (p.U - p.V) * 68; }
        public static float PixelY(FloorPoint p) { return 260 + (p.U + p.V) * (100f / 3); }
        public static bool IsFree(FloorPoint p){return IsFreeIn(p,Obstacles);}
        static bool IsFreeIn(FloorPoint p,FloorObstacle[] obstacles)
        {
            if (float.IsNaN(p.U) || float.IsNaN(p.V) || float.IsInfinity(p.U) || float.IsInfinity(p.V) ||
                p.U < FootRadius || p.V < FootRadius || p.U > Metres - FootRadius || p.V > Metres - FootRadius) return false;
            foreach (var obstacle in obstacles) if (obstacle.Contains(p, FootRadius)) return false;
            return true;
        }
        public static bool SegmentFree(FloorPoint from, FloorPoint to)
        {
            if (!IsFree(from) || !IsFree(to)) return false;
            foreach (var obstacle in Obstacles)
            {
                float first = 0, last = 1;
                if (Slab(from.U, to.U - from.U, obstacle.U0 - FootRadius, obstacle.U1 + FootRadius, ref first, ref last) &&
                    Slab(from.V, to.V - from.V, obstacle.V0 - FootRadius, obstacle.V1 + FootRadius, ref first, ref last)) return false;
            }
            return true;
        }
        static bool Slab(float origin, float delta, float min, float max, ref float first, ref float last)
        {
            if (Math.Abs(delta) < 0.0000001f) return origin >= min && origin <= max;
            float near = (min - origin) / delta, far = (max - origin) / delta;
            if (near > far) { float swap = near; near = far; far = swap; }
            first = Math.Max(first, near); last = Math.Min(last, far);
            return first <= last;
        }
    }
    public sealed class RoomNavigator
    {
        const int Side = 28;
        readonly bool[] free = new bool[Side * Side];
        public RoomNavigator()
        { for (int i = 0; i < free.Length; i++) free[i] = RoomGeometry.IsFree(Point(i)); }
        static FloorPoint Point(int id) { return new FloorPoint(0.15f + id % Side * 0.1f, 0.15f + id / Side * 0.1f); }
        int Closest(FloorPoint point)
        {
            int best = -1; float distance = float.MaxValue;
            for (int i = 0; i < free.Length; i++) if (free[i])
            {
                float d = FloorPoint.Distance(point, Point(i));
                if (d < distance && RoomGeometry.SegmentFree(point, Point(i))) { distance = d; best = i; }
            }
            return best;
        }
        public List<FloorPoint> Path(FloorPoint start, FloorPoint target)
        {
            if (!RoomGeometry.IsFree(start) || !RoomGeometry.IsFree(target)) return null;
            if (RoomGeometry.SegmentFree(start, target)) return new List<FloorPoint> { target };
            int first = Closest(start), last = Closest(target);
            if (first < 0 || last < 0) return null;
            var parents = new int[free.Length];
            for (int i = 0; i < parents.Length; i++) parents[i] = -1;
            var queue = new Queue<int>(); queue.Enqueue(first); parents[first] = first;
            while (queue.Count > 0 && parents[last] < 0)
            {
                int node = queue.Dequeue(), x = node % Side, y = node / Side;
                for (int dy = -1; dy <= 1; dy++) for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = x + dx, ny = y + dy;
                    if (nx < 0 || ny < 0 || nx >= Side || ny >= Side) continue;
                    int next = ny * Side + nx;
                    if (!free[next] || parents[next] >= 0 || !RoomGeometry.SegmentFree(Point(node), Point(next))) continue;
                    parents[next] = node; queue.Enqueue(next);
                }
            }
            if (parents[last] < 0) return null;
            var route = new List<FloorPoint> { target };
            for (int n = last; n != first; n = parents[n]) route.Add(Point(n));
            route.Add(Point(first)); route.Reverse();
            var smooth = new List<FloorPoint>(); FloorPoint current = start; int index = 0;
            while (index < route.Count)
            {
                int far = index;
                for (int i = route.Count - 1; i >= index; i--)
                    if (RoomGeometry.SegmentFree(current, route[i])) { far = i; break; }
                current = route[far]; smooth.Add(current); index = far + 1;
            }
            return smooth;
        }
        public List<FloorPoint> RandomRoute(FloorPoint start, IRandomSource random)
        {
            for (int attempt = 0; attempt < 64; attempt++)
            {
                var target = new FloorPoint(0.15f + random.Next(2701) / 1000f, 0.15f + random.Next(2701) / 1000f);
                if (FloorPoint.Distance(start, target) < 0.65f) continue;
                var route = Path(start, target); if (route != null) return route;
            }
            return null;
        }
    }
    public sealed class RoomWanderer
    {
        public const float MaximumSpeed = 0.52f, Acceleration = 0.9f;
        readonly RoomNavigator navigator = new RoomNavigator();
        readonly IRandomSource random;
        List<FloorPoint> route;
        int waypoint;
        float pause = 0.8f, speed;
        public FloorPoint Position { get; private set; }
        public FloorPoint Delta { get; private set; }
        public float Travelled { get; private set; }
        public int Arrivals { get; private set; }
        public bool Walking { get { return speed > 0.025f && FloorPoint.Distance(new FloorPoint(0, 0), Delta) > 0.00001f; } }
        public RoomWanderer(FloorPoint feet, IRandomSource source)
        {
            if (!RoomGeometry.IsFree(feet)) throw new ArgumentException("Feet must be on clear floor");
            Position = feet; random = source;
        }
        public void HoldForTurn(){Delta=new FloorPoint(0,0);speed=0;}
        public void Step(float seconds)
        {
            if (float.IsNaN(seconds) || seconds <= 0) return;
            // Bound integration steps so stalls cannot jump across obstacles.
            float remaining = Math.Min(seconds, 0.25f);
            FloorPoint before = Position;
            while (remaining > 0)
            { float dt = Math.Min(remaining, 1f / 60); Integrate(dt); remaining -= dt; }
            Delta = new FloorPoint(Position.U - before.U, Position.V - before.V);
            Travelled += FloorPoint.Distance(before, Position);
        }
        void Integrate(float dt)
        {
            if (route == null)
            {
                speed = 0; pause -= dt;
                if (pause > 0) return;
                route = navigator.RandomRoute(Position, random); waypoint = 0;
                if (route == null) { pause = 0.5f; return; }
            }
            FloorPoint target = route[waypoint];
            float distance = FloorPoint.Distance(Position, target);
            float desired = Math.Min(MaximumSpeed, (float)Math.Sqrt(2 * Acceleration * distance));
            speed = speed < desired ? Math.Min(desired, speed + Acceleration * dt) : Math.Max(desired, speed - Acceleration * dt);
            FloorPoint next = FloorPoint.Towards(Position, target, Math.Max(0.003f, speed) * dt);
            if (!RoomGeometry.SegmentFree(Position, next)) { route = null; pause = 0.4f; speed = 0; return; }
            Position = next;
            if (FloorPoint.Distance(Position, target) <= 0.000001f)
            {
                // Still integrate to the endpoint; never snap to a random destination.
                if (++waypoint == route.Count) { route = null; pause = 0.8f + random.Next(1801) / 1000f; Arrivals++; }
                speed = 0;
            }
        }
    }
}
