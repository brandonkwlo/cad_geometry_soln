namespace DataModel
{
    // Factory methods for composite shapes.
    // Each method adds entities/constraints to the sketch and returns
    // the created objects plus the next available handle value.
    public static class SketchFactory
    {
        // --- Primitives ---

        static Entity MakePoint(uint h, double x, double y) =>
            new Entity { H = new HEntity(h), Type = EntityType.PointIn2D,
                         ActPoint = new Vector3d(x, y, 0), ActVisible = true };

        static Entity MakeLine(uint h, HEntity a, HEntity b)
        {
            var e = new Entity { H = new HEntity(h), Type = EntityType.LineSegment, ActVisible = true };
            e.Point[0] = a;
            e.Point[1] = b;
            return e;
        }

        static Constraint MakeConstraint(uint h, ConstraintType type,
            HEntity ptA = default, HEntity ptB = default,
            HEntity entityA = default, HEntity entityB = default,
            double valA = 0) =>
            new Constraint { H = new HConstraint(h), Type = type,
                PtA = ptA, PtB = ptB, EntityA = entityA, EntityB = entityB, ValA = valA };

        // --- Rectangle (corner-based) ---
        // Creates 4 corner points, 4 lines, and Horizontal/Vertical constraints.
        // Returns the 4 corner points and 4 lines for use in caller's WhereDragged setup.
        public static (Entity[] Pts, Entity[] Lines, uint Next) AddCornerRectangle(
            Sketch sketch, uint h, double x, double y, double width, double height)
        {
            var pts = new[]
            {
                MakePoint(h++, x,         y),
                MakePoint(h++, x + width, y),
                MakePoint(h++, x + width, y + height),
                MakePoint(h++, x,         y + height)
            };
            sketch.Entities.AddRange(pts);

            var lines = new[]
            {
                MakeLine(h++, pts[0].H, pts[1].H),  // bottom
                MakeLine(h++, pts[1].H, pts[2].H),  // right
                MakeLine(h++, pts[2].H, pts[3].H),  // top
                MakeLine(h++, pts[3].H, pts[0].H)   // left
            };
            sketch.Entities.AddRange(lines);

            sketch.Constraints.Add(MakeConstraint(h++, ConstraintType.Horizontal, entityA: lines[0].H));
            sketch.Constraints.Add(MakeConstraint(h++, ConstraintType.Vertical,   entityA: lines[1].H));
            sketch.Constraints.Add(MakeConstraint(h++, ConstraintType.Horizontal, entityA: lines[2].H));
            sketch.Constraints.Add(MakeConstraint(h++, ConstraintType.Vertical,   entityA: lines[3].H));

            return (pts, lines, h);
        }

        // --- Rectangle (center-based) ---
        public static (Entity[] Pts, Entity[] Lines, uint Next) AddCenterRectangle(
            Sketch sketch, uint h, double cx, double cy, double width, double height) =>
            AddCornerRectangle(sketch, h, cx - width / 2, cy - height / 2, width, height);

        // --- Parallelogram ---
        // Base edge: pt1→pt2. Offset vector (dx, dy) defines the other two corners.
        // Adds 2 Parallel constraints (bottom||top, right||left).
        public static (Entity[] Pts, Entity[] Lines, uint Next) AddParallelogram(
            Sketch sketch, uint h,
            double x1, double y1, double x2, double y2, double dx, double dy)
        {
            var pts = new[]
            {
                MakePoint(h++, x1,      y1),
                MakePoint(h++, x2,      y2),
                MakePoint(h++, x2 + dx, y2 + dy),
                MakePoint(h++, x1 + dx, y1 + dy)
            };
            sketch.Entities.AddRange(pts);

            var lines = new[]
            {
                MakeLine(h++, pts[0].H, pts[1].H),  // bottom
                MakeLine(h++, pts[1].H, pts[2].H),  // right
                MakeLine(h++, pts[2].H, pts[3].H),  // top
                MakeLine(h++, pts[3].H, pts[0].H)   // left
            };
            sketch.Entities.AddRange(lines);

            sketch.Constraints.Add(MakeConstraint(h++, ConstraintType.Parallel,
                entityA: lines[0].H, entityB: lines[2].H));
            sketch.Constraints.Add(MakeConstraint(h++, ConstraintType.Parallel,
                entityA: lines[1].H, entityB: lines[3].H));

            return (pts, lines, h);
        }

        // --- Regular Polygon ---
        // N points on a circle of given radius, connected by N line segments.
        public static (Entity[] Pts, Entity[] Lines, uint Next) AddPolygon(
            Sketch sketch, uint h, double cx, double cy, int sides, double radius)
        {
            var pts = new Entity[sides];
            for (int i = 0; i < sides; i++)
            {
                double angle = 2 * Math.PI * i / sides - Math.PI / 2;
                pts[i] = MakePoint(h++, cx + radius * Math.Cos(angle), cy + radius * Math.Sin(angle));
            }
            sketch.Entities.AddRange(pts);

            var lines = new Entity[sides];
            for (int i = 0; i < sides; i++)
                lines[i] = MakeLine(h++, pts[i].H, pts[(i + 1) % sides].H);
            sketch.Entities.AddRange(lines);

            return (pts, lines, h);
        }

        // --- Perimeter Circle ---
        // Computes the circumscribed circle of 3 points and adds it to the sketch.
        public static (Entity CenterPt, Entity Circle, uint Next) AddPerimeterCircle(
            Sketch sketch, uint h,
            double x1, double y1, double x2, double y2, double x3, double y3)
        {
            var (cx, cy, r) = Circumcircle(x1, y1, x2, y2, x3, y3);
            var center = MakePoint(h++, cx, cy);
            sketch.Entities.Add(center);

            var circle = new Entity
            {
                H           = new HEntity(h++),
                Type        = EntityType.Circle,
                ActVisible  = true,
                NumDistance = r,
                ActDistance = r
            };
            circle.Point[0] = center.H;
            sketch.Entities.Add(circle);

            return (center, circle, h);
        }

        // Circumcenter + radius from 3 points.
        static (double cx, double cy, double r) Circumcircle(
            double x1, double y1, double x2, double y2, double x3, double y3)
        {
            double ax = x2 - x1, ay = y2 - y1;
            double bx = x3 - x1, by = y3 - y1;
            double d  = 2 * (ax * by - ay * bx);
            double ux = (by * (ax * ax + ay * ay) - ay * (bx * bx + by * by)) / d;
            double uy = (ax * (bx * bx + by * by) - bx * (ax * ax + ay * ay)) / d;
            return (x1 + ux, y1 + uy, Math.Sqrt(ux * ux + uy * uy));
        }
    }
}
