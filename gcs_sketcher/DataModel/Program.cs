// Program.cs
// Proof of concept for the DataModel and SolverEngine.
// This is a simple console app that creates a sketch
// with two lines, applies a parallel constraint, and
// runs the solver to see the results.

using DataModel;
using Constraint = DataModel.Constraint;

// -----------------------------------------------------------------------
// 1. Define two NON-parallel lines.
//    Line1: (0,0) → (10,0)  slope = 0   (horizontal)
//    Line2: (0,5) → (15,9)  slope = 4/15 ≈ 0.267  ← NOT parallel
//
//    After solving, the Parallel constraint should pull pt4 from (15,9)
//    down to (15,5) so that Line2 matches Line1's slope of 0.
// -----------------------------------------------------------------------
var sketch = new Sketch();

var pt1 = new Entity { H = new HEntity(1), Type = EntityType.PointIn2D, ActPoint = new Vector3d(0,  0, 0), ActVisible = true };
var pt2 = new Entity { H = new HEntity(2), Type = EntityType.PointIn2D, ActPoint = new Vector3d(10, 0, 0), ActVisible = true };
var pt3 = new Entity { H = new HEntity(3), Type = EntityType.PointIn2D, ActPoint = new Vector3d(0,  5, 0), ActVisible = true };
var pt4 = new Entity { H = new HEntity(4), Type = EntityType.PointIn2D, ActPoint = new Vector3d(15, 9, 0), ActVisible = true };

sketch.Entities.AddRange(new[] { pt1, pt2, pt3, pt4 });

var line1 = new Entity { H = new HEntity(10), Type = EntityType.LineSegment, ActVisible = true };
line1.Point[0] = pt1.H;
line1.Point[1] = pt2.H;

var line2 = new Entity { H = new HEntity(11), Type = EntityType.LineSegment, ActVisible = true };
line2.Point[0] = pt3.H;
line2.Point[1] = pt4.H;

sketch.Entities.AddRange(new[] { line1, line2 });

// -----------------------------------------------------------------------
// 2. Constraints.
//    - PARALLEL between line1 and line2.
//    - WHERE_DRAGGED on pt1, pt2, pt3 to pin them in place.
//      The solver is then free to move only pt4 to satisfy parallel.
// -----------------------------------------------------------------------
sketch.Constraints.Add(new Constraint { H = new HConstraint(100), Type = ConstraintType.Parallel,      EntityA = line1.H, EntityB = line2.H });
sketch.Constraints.Add(new Constraint { H = new HConstraint(101), Type = ConstraintType.WhereDragged,  PtA = pt1.H });
sketch.Constraints.Add(new Constraint { H = new HConstraint(102), Type = ConstraintType.WhereDragged,  PtA = pt2.H });
sketch.Constraints.Add(new Constraint { H = new HConstraint(103), Type = ConstraintType.WhereDragged,  PtA = pt3.H });

// -----------------------------------------------------------------------
// 3. Print initial state.
// -----------------------------------------------------------------------
Console.WriteLine("=== Before Solve ===");
foreach (var e in sketch.Entities.Where(e => e.Type == EntityType.PointIn2D))
    Console.WriteLine($"  {e.H}  ({e.ActPoint.X:F3}, {e.ActPoint.Y:F3})");

double s1Before = (pt2.ActPoint.Y - pt1.ActPoint.Y) / (pt2.ActPoint.X - pt1.ActPoint.X);
double s2Before = (pt4.ActPoint.Y - pt3.ActPoint.Y) / (pt4.ActPoint.X - pt3.ActPoint.X);
Console.WriteLine($"  Line1 slope: {s1Before:F6}");
Console.WriteLine($"  Line2 slope: {s2Before:F6}  (expected ~0.267)");

// -----------------------------------------------------------------------
// 4. Run the solver.
// -----------------------------------------------------------------------
int result = SolverEngine.Solve(sketch);
Console.WriteLine($"\nSolver result: {result} ({(result == 0 ? "OKAY" : "FAILED")})");

// -----------------------------------------------------------------------
// 5. Print solved state.
// -----------------------------------------------------------------------
Console.WriteLine("\n=== After Solve ===");
foreach (var e in sketch.Entities.Where(e => e.Type == EntityType.PointIn2D))
    Console.WriteLine($"  {e.H}  ({e.ActPoint.X:F3}, {e.ActPoint.Y:F3})");

// -----------------------------------------------------------------------
// 6. Verify parallel.
// -----------------------------------------------------------------------
var a = sketch.FindEntity(line1.Point[0])!.ActPoint;
var b = sketch.FindEntity(line1.Point[1])!.ActPoint;
var c = sketch.FindEntity(line2.Point[0])!.ActPoint;
var d = sketch.FindEntity(line2.Point[1])!.ActPoint;

double slope1 = (b.Y - a.Y) / (b.X - a.X);
double slope2 = (d.Y - c.Y) / (d.X - c.X);
bool isParallel = Math.Abs(slope1 - slope2) < 1e-6;

Console.WriteLine($"\n=== Parallel Check ===");
Console.WriteLine($"  Line1 slope: {slope1:F6}");
Console.WriteLine($"  Line2 slope: {slope2:F6}");
Console.WriteLine($"  Parallel:    {isParallel}");

// -----------------------------------------------------------------------
// 7. Save and reload.
// -----------------------------------------------------------------------
sketch.SaveToJson("sketch.json");
Console.WriteLine("\nSaved to sketch.json");
var loaded = Sketch.LoadFromJson("sketch.json");
Console.WriteLine($"Reloaded: {loaded.Entities.Count} entities, {loaded.Constraints.Count} constraints");
