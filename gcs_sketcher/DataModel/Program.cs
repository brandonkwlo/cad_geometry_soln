// Program.cs
// Proof of concept: two parallel horizontal lines,
// built using the translated SolveSpace data structures.

using DataModel;
using System.Data;
using System.Diagnostics;
using Constraint = DataModel.Constraint;

// -------------------------------------------------------------------
// 1. Create the sketch
// -------------------------------------------------------------------
var sketch = new Sketch();

// -------------------------------------------------------------------
// 2. Add the point entities (PointIn2D)
//    In SolveSpace a LineSegment request generates:
//      entity(0) = the line itself
//      entity(1) = start point
//      entity(2) = end point
//    We replicate that structure here manually for the PoC.
// -------------------------------------------------------------------

// Line 1 points: (0,0) → (10,0)
var pt1 = new Entity
{
    H = new HEntity(1),
    Type = EntityType.PointIn2D,
    ActPoint = new Vector3d(0, 0, 0),
    ActVisible = true,
};
var pt2 = new Entity
{
    H = new HEntity(2),
    Type = EntityType.PointIn2D,
    ActPoint = new Vector3d(10, 0, 0),
    ActVisible = true,
};

// Line 2 points: (0,5) → (10,5)
var pt3 = new Entity
{
    H = new HEntity(3),
    Type = EntityType.PointIn2D,
    ActPoint = new Vector3d(0, 5, 0),
    ActVisible = true,
};
var pt4 = new Entity
{
    H = new HEntity(4),
    Type = EntityType.PointIn2D,
    ActPoint = new Vector3d(10, 5, 0),
    ActVisible = true,
};

sketch.Entities.AddRange(new[] { pt1, pt2, pt3, pt4 });

// -------------------------------------------------------------------
// 3. Add the line segment entities
//    point[0] = start, point[1] = end  (matches SolveSpace convention)
// -------------------------------------------------------------------
var line1 = new Entity
{
    H = new HEntity(10),
    Type = EntityType.LineSegment,
    ActVisible = true,
};
line1.Point[0] = pt1.H;
line1.Point[1] = pt2.H;

var line2 = new Entity
{
    H = new HEntity(11),
    Type = EntityType.LineSegment,
    ActVisible = true,
};
line2.Point[0] = pt3.H;
line2.Point[1] = pt4.H;

sketch.Entities.AddRange(new[] { line1, line2 });

// -------------------------------------------------------------------
// 4. Add the Parallel constraint
//    entityA = line1, entityB = line2  (matches ConstraintBase fields)
// -------------------------------------------------------------------
var parallel = new Constraint
{
    H = new HConstraint(100),
    Type = ConstraintType.Parallel,
    EntityA = line1.H,
    EntityB = line2.H,
};
sketch.Constraints.Add(parallel);

// -------------------------------------------------------------------
// 5. Print what we built
// -------------------------------------------------------------------
Console.WriteLine("=== Sketch Contents ===");
Console.WriteLine($"Entities    : {sketch.Entities.Count}");
Console.WriteLine($"Constraints : {sketch.Constraints.Count}");

foreach (var e in sketch.Entities)
    Console.WriteLine($"  {e.H}  {e.DescriptionString()}");

foreach (var c in sketch.Constraints)
    Console.WriteLine($"  {c.H}  {c.DescriptionString()}");

// -------------------------------------------------------------------
// 6. Verify parallel (slopes equal)
// -------------------------------------------------------------------
var s1 = sketch.FindEntity(line1.Point[0])!.ActPoint;
var e1 = sketch.FindEntity(line1.Point[1])!.ActPoint;
var s2 = sketch.FindEntity(line2.Point[0])!.ActPoint;
var e2 = sketch.FindEntity(line2.Point[1])!.ActPoint;

double slope1 = (e1.Y - s1.Y) / (e1.X - s1.X);
double slope2 = (e2.Y - s2.Y) / (e2.X - s2.X);
bool isParallel = Math.Abs(slope1 - slope2) < 1e-9;

Console.WriteLine($"\n=== Parallel Check ===");
Console.WriteLine($"Line1 slope : {slope1}");
Console.WriteLine($"Line2 slope : {slope2}");
Console.WriteLine($"Parallel    : {isParallel}");

// -------------------------------------------------------------------
// 7. Save and reload (JSON round-trip)
// -------------------------------------------------------------------
sketch.SaveToJson("sketch.json");
Console.WriteLine("\nSaved to sketch.json");

var loaded = Sketch.LoadFromJson("sketch.json");
Console.WriteLine($"Reloaded    : {loaded.Entities.Count} entities, " +
                  $"{loaded.Constraints.Count} constraints");