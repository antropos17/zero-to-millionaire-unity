using System;

namespace ZeroToMillionaire
{
    public static class NavigationChecks
    {
        public static string Run()
        {
            int samples = 0, arrivals = 0; float distance = 0;
            var navigation = new RoomNavigator();
            if (RoomGeometry.SegmentFree(new FloorPoint(1.406f,1.036f),new FloorPoint(1.414f,1.042f)))
                throw new Exception("Sub-centimetre corner crossing must be blocked");
            var route = navigation.Path(new FloorPoint(1.35f, 0.45f), new FloorPoint(2.4f, 2.4f));
            if (route == null) throw new Exception("Floor passage is disconnected");
            for (int seed = 0; seed < 12; seed++)
            {
                var walker = new RoomWanderer(new FloorPoint(1.85f, 1.45f), new SeededRandom(seed));
                for (int frame = 0; frame < 36000; frame++)
                {
                    FloorPoint before = walker.Position;
                    float dt = frame % 1739 == 0 ? 0.25f : 1f / 60;
                    walker.Step(dt); samples++;
                    float step = FloorPoint.Distance(before, walker.Position);
                    if (!RoomGeometry.IsFree(walker.Position)) throw new Exception("Walk crossed wall, furniture or door swing");
                    if (step > RoomWanderer.MaximumSpeed * Math.Min(dt, 0.25f) + 0.0001f) throw new Exception("Walk teleported");
                    if (dt < 0.02f && !RoomGeometry.SegmentFree(before, walker.Position)) throw new Exception("Walk cut obstacle corner");
                }
                if (walker.Arrivals < 30 || walker.Travelled < 100) throw new Exception("Walk activity seed="+seed+" arrivals="+walker.Arrivals+" metres="+walker.Travelled);
                arrivals += walker.Arrivals; distance += walker.Travelled;
            }
            return "PASS walking: " + samples + " random motion samples, " + arrivals + " destinations, " + distance.ToString("F1") + "m; walls/furniture/door clear; stall integration and no teleport";
        }
    }
}
