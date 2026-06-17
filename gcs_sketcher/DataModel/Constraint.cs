// Constraint.cs
// Mirrors the Constraint class from sketch.h
// Inherits ConstraintBase (all the data) and adds the display struct
// that controls how the constraint label is drawn on-screen.
// Draw() methods are left out — those belong in the UI layer.

namespace DataModel
{
    public class Constraint : ConstraintBase
    {
        /// <summary>
        /// Mirrors the anonymous disp struct in sketch.h.
        /// Controls where the constraint label is drawn and its style.
        /// </summary>
        public class DisplayInfo
        {
            // Offset of the label from its default position
            public Vector3d Offset { get; set; }
            public HStyle   Style  { get; set; }
        }

        public DisplayInfo Disp { get; set; } = new DisplayInfo();

        public bool IsVisible()  => !Reference || Type == ConstraintType.Comment;
        public bool IsStylable() => Type != ConstraintType.Comment;

        public string Label()
        {
            if (Type == ConstraintType.Comment)
                return Comment;

            // For dimensional constraints, format the numeric value
            return HasLabel() ? $"{ValA:F3}" : "";
        }

        public string DescriptionString()
        {
            return Type switch
            {
                ConstraintType.PointsCoincident  => "coincident",
                ConstraintType.PtPtDistance      => $"distance {ValA:F3}",
                ConstraintType.Parallel          => "parallel",
                ConstraintType.Perpendicular     => "perpendicular",
                ConstraintType.Horizontal        => "horizontal",
                ConstraintType.Vertical          => "vertical",
                ConstraintType.EqualLengthLines  => "equal length",
                ConstraintType.EqualRadius       => "equal radius",
                ConstraintType.Angle             => $"angle {ValA:F3}°",
                ConstraintType.Diameter          => $"diameter {ValA:F3}",
                ConstraintType.PtOnLine          => "point on line",
                ConstraintType.PtOnCircle        => "point on circle",
                ConstraintType.Symmetric         => "symmetric",
                ConstraintType.Comment           => $"comment: {Comment}",
                _                                => $"constraint [{Type}]"
            };
        }
    }
}
