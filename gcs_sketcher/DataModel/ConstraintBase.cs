// ConstraintBase.cs
// Mirrors ConstraintBase from sketch.h
// ConstraintBase holds all the data for one geometric constraint.
// Constraint (in Constraint.cs) inherits this and adds display fields.

namespace DataModel
{
    public enum ConstraintType : uint
    {
        PointsCoincident     = 20,
        PtPtDistance         = 30,
        PtPlaneDistance      = 31,
        PtLineDistance       = 32,
        PtFaceDistance       = 33,
        ProjPtDistance       = 34,
        PtInPlane            = 41,
        PtOnLine             = 42,
        PtOnFace             = 43,
        EqualLengthLines     = 50,
        LengthRatio          = 51,
        EqLenPtLineD         = 52,
        EqPtLnDistances      = 53,
        EqualAngle           = 54,
        EqualLineArcLen      = 55,
        LengthDifference     = 56,
        Symmetric            = 60,
        SymmetricHoriz       = 61,
        SymmetricVert        = 62,
        SymmetricLine        = 63,
        AtMidpoint           = 70,
        Horizontal           = 80,
        Vertical             = 81,
        Diameter             = 90,
        PtOnCircle           = 100,
        SameOrientation      = 110,
        Angle                = 120,
        Parallel             = 121,
        Perpendicular        = 122,
        ArcLineTangent       = 123,
        CubicLineTangent     = 124,
        CurveCurveTangent    = 125,
        EqualRadius          = 130,
        WhereDragged         = 200,
        ArcArcLenRatio       = 210,
        ArcLineLenRatio      = 211,
        ArcArcDifference     = 212,
        ArcLineDifference    = 213,
        Comment              = 1000
    }

    public class ConstraintBase
    {
        public int           Tag { get; set; }
        public HConstraint   H   { get; set; }

        public static readonly HConstraint NO_CONSTRAINT = HConstraint.NO_CONSTRAINT;

        public ConstraintType Type { get; set; }

        public HGroup  Group     { get; set; }
        public HEntity Workplane { get; set; }

        // --- Parameters for the constraint ---

        // Numeric value (e.g. distance for PT_PT_DISTANCE, angle for ANGLE)
        public double ValA { get; set; }

        // Parametric value (references a solver parameter instead of a fixed number)
        public HParam ValP { get; set; }

        // The entities and points this constraint is applied to
        public HEntity PtA     { get; set; }
        public HEntity PtB     { get; set; }
        public HEntity EntityA { get; set; }
        public HEntity EntityB { get; set; }
        public HEntity EntityC { get; set; }
        public HEntity EntityD { get; set; }

        // Modifier flags (used to disambiguate e.g. which angle direction)
        public bool Other  { get; set; }
        public bool Other2 { get; set; }

        // If true, this is a reference dimension — it displays a value
        // but generates no solver equations (doesn't drive geometry)
        public bool Reference { get; set; }

        // Comments are stored as constraints of type Comment
        public string Comment { get; set; } = "";

        public bool Equals(ConstraintBase c)
        {
            return Type      == c.Type      &&
                   Group.V   == c.Group.V   &&
                   Workplane.V == c.Workplane.V &&
                   ValA      == c.ValA      &&
                   ValP.V    == c.ValP.V    &&
                   PtA.V     == c.PtA.V     &&
                   PtB.V     == c.PtB.V     &&
                   EntityA.V == c.EntityA.V &&
                   EntityB.V == c.EntityB.V &&
                   EntityC.V == c.EntityC.V &&
                   EntityD.V == c.EntityD.V &&
                   Other     == c.Other     &&
                   Other2    == c.Other2    &&
                   Reference == c.Reference &&
                   Comment   == c.Comment;
        }

        /// <summary>
        /// Returns true if this constraint type shows a numeric label on screen.
        /// </summary>
        public bool HasLabel()
        {
            return Type == ConstraintType.PtPtDistance     ||
                   Type == ConstraintType.PtPlaneDistance  ||
                   Type == ConstraintType.PtLineDistance   ||
                   Type == ConstraintType.Diameter         ||
                   Type == ConstraintType.LengthRatio      ||
                   Type == ConstraintType.LengthDifference ||
                   Type == ConstraintType.Angle            ||
                   Type == ConstraintType.Comment;
        }

        public void Clear() { }
    }
}
