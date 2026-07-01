using DataModel;
using Constraint = DataModel.Constraint;

ParallelLinesDemo();
CircleDemo();
ArcDemo();
RectangleDemo();

// -----------------------------------------------------------------------
// Demo 1: Parallel Lines
// Two non-parallel lines. The Parallel constraint moves pt4 so both
// lines share the same slope.
// -----------------------------------------------------------------------
static void ParallelLinesDemo()
{
    Console.WriteLine("=== Demo 1: Parallel Lines ===");
    var sketch = new Sketch();

    var pt1 = new Entity { H = new HEntity(1), Type = EntityType.PointIn2D, ActPoint = new Vector3d(0,  0, 0), ActVisible = true };
    var pt2 = new Entity { H = new HEntity(2), Type = EntityType.PointIn2D, ActPoint = new Vector3d(10, 0, 0), ActVisible = true };
    var pt3 = new Entity { H = new HEntity(3), Type = EntityType.PointIn2D, ActPoint = new Vector3d(0,  5, 0), ActVisible = true };
    var pt4 = new Entity { H = new HEntity(4), Type = EntityType.PointIn2D, ActPoint = new Vector3d(15, 9, 0), ActVisible = true };
    sketch.Entities.AddRange(new[] { pt1, pt2, pt3, pt4 });

    var line1 = new Entity { H = new HEntity(10), Type = EntityType.LineSegment, ActVisible = true };
    line1.Point[0] = pt1.H; line1.Point[1] = pt2.H;

    var line2 = new Entity { H = new HEntity(11), Type = EntityType.LineSegment, ActVisible = true };
    line2.Point[0] = pt3.H; line2.Point[1] = pt4.H;
    sketch.Entities.AddRange(new[] { line1, line2 });

    sketch.Constraints.Add(new Constraint { H = new HConstraint(100), Type = ConstraintType.Parallel,     EntityA = line1.H, EntityB = line2.H });
    sketch.Constraints.Add(new Constraint { H = new HConstraint(101), Type = ConstraintType.WhereDragged, PtA = pt1.H });
    sketch.Constraints.Add(new Constraint { H = new HConstraint(102), Type = ConstraintType.WhereDragged, PtA = pt2.H });
    sketch.Constraints.Add(new Constraint { H = new HConstraint(103), Type = ConstraintType.WhereDragged, PtA = pt3.H });

    PrintPoints("Before", sketch);
    double slopeBefore = (pt4.ActPoint.Y - pt3.ActPoint.Y) / (pt4.ActPoint.X - pt3.ActPoint.X);
    Console.WriteLine($"  Line2 slope before: {slopeBefore:F4}  (expected ~0.2667)");

    int r = SolverEngine.Solve(sketch);
    Console.WriteLine($"  Solver: {ResultLabel(r)}");
    PrintPoints("After", sketch);

    var a = sketch.FindEntity(line1.Point[0])!.ActPoint;
    var b = sketch.FindEntity(line1.Point[1])!.ActPoint;
    var c = sketch.FindEntity(line2.Point[0])!.ActPoint;
    var d = sketch.FindEntity(line2.Point[1])!.ActPoint;
    bool parallel = Math.Abs((b.Y - a.Y) / (b.X - a.X) - (d.Y - c.Y) / (d.X - c.X)) < 1e-6;
    Console.WriteLine($"  Parallel: {parallel}  (pt4 expected (15.000, 5.000))\n");

    sketch.SaveToJson("sketch_parallel.json");
}

// -----------------------------------------------------------------------
// Demo 2: Circle with Diameter Constraint
// Circle starts with radius 3. A Diameter constraint of 10 drives
// the solver to resize it to radius 5.
// -----------------------------------------------------------------------
static void CircleDemo()
{
    Console.WriteLine("=== Demo 2: Circle with Diameter Constraint ===");
    var sketch = new Sketch();

    var center = new Entity { H = new HEntity(1), Type = EntityType.PointIn2D, ActPoint = new Vector3d(5, 5, 0), ActVisible = true };
    sketch.Entities.Add(center);

    var circle = new Entity { H = new HEntity(2), Type = EntityType.Circle, ActVisible = true, NumDistance = 3.0, ActDistance = 3.0 };
    circle.Point[0] = center.H;
    sketch.Entities.Add(circle);

    sketch.Constraints.Add(new Constraint { H = new HConstraint(100), Type = ConstraintType.WhereDragged, PtA = center.H });
    sketch.Constraints.Add(new Constraint { H = new HConstraint(101), Type = ConstraintType.Diameter, EntityA = circle.H, ValA = 10.0 });

    Console.WriteLine($"  Radius before: {circle.NumDistance:F3}");
    int r = SolverEngine.Solve(sketch);
    Console.WriteLine($"  Solver: {ResultLabel(r)}");
    Console.WriteLine($"  Radius after:  {circle.NumDistance:F3}  (expected 5.000)\n");
}

// -----------------------------------------------------------------------
// Demo 3: 3-Point Arc
// Arc: center=(0,0), start=(5,0), end=(4,4). The end point is NOT on
// the radius-5 circle. The arc constraint forces end onto the circle.
// -----------------------------------------------------------------------
static void ArcDemo()
{
    Console.WriteLine("=== Demo 3: 3-Point Arc ===");
    var sketch = new Sketch();

    var center = new Entity { H = new HEntity(1), Type = EntityType.PointIn2D, ActPoint = new Vector3d(0, 0, 0), ActVisible = true };
    var start  = new Entity { H = new HEntity(2), Type = EntityType.PointIn2D, ActPoint = new Vector3d(5, 0, 0), ActVisible = true };
    var end    = new Entity { H = new HEntity(3), Type = EntityType.PointIn2D, ActPoint = new Vector3d(4, 4, 0), ActVisible = true };
    sketch.Entities.AddRange(new[] { center, start, end });

    var arc = new Entity { H = new HEntity(10), Type = EntityType.ArcOfCircle, ActVisible = true };
    arc.Point[0] = center.H;
    arc.Point[1] = start.H;
    arc.Point[2] = end.H;
    sketch.Entities.Add(arc);

    sketch.Constraints.Add(new Constraint { H = new HConstraint(100), Type = ConstraintType.WhereDragged, PtA = center.H });
    sketch.Constraints.Add(new Constraint { H = new HConstraint(101), Type = ConstraintType.WhereDragged, PtA = start.H });

    double distBefore = Math.Sqrt(end.ActPoint.X * end.ActPoint.X + end.ActPoint.Y * end.ActPoint.Y);
    Console.WriteLine($"  End before: ({end.ActPoint.X:F3}, {end.ActPoint.Y:F3})  dist={distBefore:F3}  (not on r=5 circle)");

    int r = SolverEngine.Solve(sketch);
    Console.WriteLine($"  Solver: {ResultLabel(r)}");

    double distAfter = Math.Sqrt(end.ActPoint.X * end.ActPoint.X + end.ActPoint.Y * end.ActPoint.Y);
    Console.WriteLine($"  End after:  ({end.ActPoint.X:F3}, {end.ActPoint.Y:F3})  dist={distAfter:F3}  (expected ~5.000)\n");
}

// -----------------------------------------------------------------------
// Demo 4: Rectangle
// SketchFactory builds 4 lines with Horizontal/Vertical constraints.
// pt4 is corrupted to (1,9). The solver pulls it to (0,7).
// -----------------------------------------------------------------------
static void RectangleDemo()
{
    Console.WriteLine("=== Demo 4: Rectangle ===");
    var sketch = new Sketch();

    var (pts, _, nextH) = SketchFactory.AddCornerRectangle(sketch, 1, 0, 0, 10, 7);

    // Corrupt the top-left corner so the solver has real work to do.
    pts[3].ActPoint = new Vector3d(1, 9, 0);

    sketch.Constraints.Add(new Constraint { H = new HConstraint(nextH++), Type = ConstraintType.WhereDragged, PtA = pts[0].H });
    sketch.Constraints.Add(new Constraint { H = new HConstraint(nextH++), Type = ConstraintType.WhereDragged, PtA = pts[1].H });
    sketch.Constraints.Add(new Constraint { H = new HConstraint(nextH),   Type = ConstraintType.WhereDragged, PtA = pts[2].H });

    Console.WriteLine($"  pt4 before: ({pts[3].ActPoint.X:F3}, {pts[3].ActPoint.Y:F3})");
    int r = SolverEngine.Solve(sketch);
    Console.WriteLine($"  Solver: {ResultLabel(r)}");
    Console.WriteLine($"  pt4 after:  ({pts[3].ActPoint.X:F3}, {pts[3].ActPoint.Y:F3})  (expected (0.000, 7.000))\n");
}

// -----------------------------------------------------------------------
// Helpers
// -----------------------------------------------------------------------
static void PrintPoints(string label, Sketch sketch)
{
    Console.WriteLine($"  [{label}]");
    foreach (var e in sketch.Entities.Where(e => e.Type == EntityType.PointIn2D))
        Console.WriteLine($"    {e.H}  ({e.ActPoint.X:F3}, {e.ActPoint.Y:F3})");
}

static string ResultLabel(int r) => r switch
{
    0 => "OKAY",
    1 => "INCONSISTENT",
    2 => "DIDN'T CONVERGE",
    3 => "TOO MANY UNKNOWNS",
    _ => $"code {r}"
};
