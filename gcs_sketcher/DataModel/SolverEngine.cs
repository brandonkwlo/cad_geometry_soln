// SolverEngine.cs
// Translates the SolveSketch function from slvs.h into C#.

namespace DataModel
{
    public static class SolverEngine
    {
        private const uint Group = 1;

        // Solves all constraints in the sketch using libslvs.
        // On success (return 0), writes solved coordinates back into
        // each PointIn2D entity's ActPoint field.
        public static int Solve(Sketch sketch)
        {
            Slvs.ClearSketch();
            var wp = Slvs.AddBase2D(Group);

            // Register every 2D point with the solver.
            // Key = HEntity.V, Value = the SlvsEntity whose Param0/Param1
            // are the x and y parameter handles.
            var pointMap = new Dictionary<uint, SlvsEntity>();
            foreach (var e in sketch.Entities)
            {
                if (e.Type != EntityType.PointIn2D) continue;
                var sp = Slvs.AddPoint2D(Group, e.ActPoint.X, e.ActPoint.Y, wp);
                pointMap[e.H.V] = sp;
            }

            // Register every line segment.
            var lineMap = new Dictionary<uint, SlvsEntity>();
            foreach (var e in sketch.Entities)
            {
                if (e.Type != EntityType.LineSegment) continue;
                if (!pointMap.TryGetValue(e.Point[0].V, out var ptA)) continue;
                if (!pointMap.TryGetValue(e.Point[1].V, out var ptB)) continue;
                var sl = Slvs.AddLine2D(Group, ptA, ptB, wp);
                lineMap[e.H.V] = sl;
            }

            // Translate each DataModel constraint into a libslvs call.
            foreach (var c in sketch.Constraints)
            {
                switch (c.Type)
                {
                    case ConstraintType.Parallel:
                        if (lineMap.TryGetValue(c.EntityA.V, out var la) &&
                            lineMap.TryGetValue(c.EntityB.V, out var lb))
                            Slvs.Parallel(Group, la, lb, wp);
                        break;

                    // WHERE_DRAGGED pins the point at its current position,
                    // giving the solver a fixed anchor to solve against.
                    case ConstraintType.WhereDragged:
                        if (pointMap.TryGetValue(c.PtA.V, out var dp))
                            Slvs.Dragged(Group, dp, wp);
                        break;
                }
            }

            var result = Slvs.SolveSketch(Group, IntPtr.Zero);

            if (result.Result == Slvs.ResultOkay)
            {
                foreach (var e in sketch.Entities)
                {
                    if (e.Type != EntityType.PointIn2D) continue;
                    if (!pointMap.TryGetValue(e.H.V, out var sp)) continue;
                    double x = Slvs.GetParamValue(sp.Param0);
                    double y = Slvs.GetParamValue(sp.Param1);
                    e.ActPoint = new Vector3d(x, y, 0);
                }
            }

            return result.Result;
        }
    }
}
