// Entity.cs
// Mirrors the Entity class from sketch.h
// Entity inherits EntityBase and adds runtime display state
// (visibility, bounding box, cached edges, etc.)
// We skip the Canvas/Draw methods — those belong in the UI layer, not data model.

namespace DataModel
{
    public enum DrawAs
    {
        Default,
        Overlay,
        Hidden,
        Hovered,
        Selected
    }

    public class Entity : EntityBase
    {
        // A linked entity that was hidden in the source file is hidden here too
        public bool ForceHidden { get; set; }

        // Numerically evaluated current state (populated after solver runs)
        public Vector3d   ActPoint    { get; set; }
        public Quaternion ActNormal   { get; set; }
        public double     ActDistance { get; set; }

        // Whether this entity is currently visible (saved for import/export)
        public bool ActVisible { get; set; }

        public HStyle Style        { get; set; }
        public bool   Construction { get; set; }

        // Chord tolerance used when generating edge approximations
        public double EdgesChordTol { get; set; }

        // Whether the entity can be interactively dragged
        public bool CanBeDragged()
        {
            // Only free points and normals can be dragged in a 2D sketch
            return IsPoint() || IsNormal();
        }

        public bool IsVisible() => !ForceHidden && ActVisible;

        public bool IsStylable()
        {
            // Workplanes and reference geometry are not user-stylable
            return Type != EntityType.Workplane &&
                   Type != EntityType.NormalIn3D &&
                   Type != EntityType.NormalIn2D;
        }

        public string DescriptionString()
        {
            return Type switch
            {
                EntityType.PointIn2D    => $"point ({ActPoint.X:F3}, {ActPoint.Y:F3})",
                EntityType.LineSegment  => $"line-segment",
                EntityType.Circle       => $"circle r={ActDistance:F3}",
                EntityType.ArcOfCircle  => $"arc",
                EntityType.Workplane    => $"workplane",
                _                       => $"entity [{Type}]"
            };
        }

        public new void Clear()
        {
            base.Clear();
        }
    }
}
