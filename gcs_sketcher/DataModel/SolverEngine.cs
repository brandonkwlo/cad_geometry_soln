namespace DataModel
{
    public static class SolverEngine
    {
        private const uint Group = 1;

        public static int Solve(Sketch sketch)
        {
            Slvs.ClearSketch();
            var wp     = Slvs.AddBase2D(Group);
            var norm2d = Slvs.AddNormal2D(Group, wp);

            var pointMap  = new Dictionary<uint, SlvsEntity>();
            var lineMap   = new Dictionary<uint, SlvsEntity>();
            var arcMap    = new Dictionary<uint, SlvsEntity>();
            var circleMap = new Dictionary<uint, SlvsEntity>();
            var distMap   = new Dictionary<uint, SlvsEntity>();

            foreach (var e in sketch.Entities)
            {
                if (e.Type != EntityType.PointIn2D) continue;
                pointMap[e.H.V] = Slvs.AddPoint2D(Group, e.ActPoint.X, e.ActPoint.Y, wp);
            }

            foreach (var e in sketch.Entities)
            {
                switch (e.Type)
                {
                    case EntityType.LineSegment:
                        if (!pointMap.TryGetValue(e.Point[0].V, out var la)) break;
                        if (!pointMap.TryGetValue(e.Point[1].V, out var lb)) break;
                        lineMap[e.H.V] = Slvs.AddLine2D(Group, la, lb, wp);
                        break;

                    case EntityType.ArcOfCircle:
                        if (!pointMap.TryGetValue(e.Point[0].V, out var ac)) break;
                        if (!pointMap.TryGetValue(e.Point[1].V, out var as_)) break;
                        if (!pointMap.TryGetValue(e.Point[2].V, out var ae)) break;
                        arcMap[e.H.V] = Slvs.AddArc(Group, norm2d, ac, as_, ae, wp);
                        break;

                    case EntityType.Circle:
                        if (!pointMap.TryGetValue(e.Point[0].V, out var cc)) break;
                        var dist = Slvs.AddDistance(Group, e.NumDistance, wp);
                        distMap[e.H.V]   = dist;
                        circleMap[e.H.V] = Slvs.AddCircle(Group, norm2d, cc, dist, wp);
                        break;
                }
            }

            foreach (var c in sketch.Constraints)
            {
                switch (c.Type)
                {
                    case ConstraintType.Parallel:
                        if (lineMap.TryGetValue(c.EntityA.V, out var pla) &&
                            lineMap.TryGetValue(c.EntityB.V, out var plb))
                            Slvs.Parallel(Group, pla, plb, wp);
                        break;

                    case ConstraintType.Perpendicular:
                        if (lineMap.TryGetValue(c.EntityA.V, out var prla) &&
                            lineMap.TryGetValue(c.EntityB.V, out var prlb))
                            Slvs.Perpendicular(Group, prla, prlb, wp, 0);
                        break;

                    case ConstraintType.Horizontal:
                        if (lineMap.TryGetValue(c.EntityA.V, out var hl))
                            Slvs.Horizontal(Group, hl, wp, default);
                        break;

                    case ConstraintType.Vertical:
                        if (lineMap.TryGetValue(c.EntityA.V, out var vl))
                            Slvs.Vertical(Group, vl, wp, default);
                        break;

                    case ConstraintType.PointsCoincident:
                        if (pointMap.TryGetValue(c.PtA.V, out var coa) &&
                            pointMap.TryGetValue(c.PtB.V, out var cob))
                            Slvs.Coincident(Group, coa, cob, wp);
                        break;

                    case ConstraintType.Diameter:
                        if (circleMap.TryGetValue(c.EntityA.V, out var dc))
                            Slvs.Diameter(Group, dc, c.ValA);
                        break;

                    case ConstraintType.EqualLengthLines:
                        if (lineMap.TryGetValue(c.EntityA.V, out var ela) &&
                            lineMap.TryGetValue(c.EntityB.V, out var elb))
                            Slvs.Equal(Group, ela, elb, wp);
                        break;

                    case ConstraintType.ArcLineTangent:
                        if (arcMap.TryGetValue(c.EntityA.V, out var ata) &&
                            lineMap.TryGetValue(c.EntityB.V, out var atl))
                            Slvs.Tangent(Group, ata, atl, wp);
                        break;

                    case ConstraintType.CurveCurveTangent:
                        if (arcMap.TryGetValue(c.EntityA.V, out var cca) &&
                            arcMap.TryGetValue(c.EntityB.V, out var ccb))
                            Slvs.Tangent(Group, cca, ccb, wp);
                        break;

                    case ConstraintType.AtMidpoint:
                        if (pointMap.TryGetValue(c.PtA.V, out var mpt) &&
                            lineMap.TryGetValue(c.EntityA.V, out var mln))
                            Slvs.Midpoint(Group, mpt, mln, wp);
                        break;

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
                    e.ActPoint = new Vector3d(
                        Slvs.GetParamValue(sp.Param0),
                        Slvs.GetParamValue(sp.Param1),
                        0);
                }

                foreach (var e in sketch.Entities)
                {
                    if (e.Type != EntityType.Circle) continue;
                    if (!distMap.TryGetValue(e.H.V, out var d)) continue;
                    e.NumDistance = Slvs.GetParamValue(d.Param0);
                    e.ActDistance = e.NumDistance;
                }
            }

            return result.Result;
        }
    }
}
