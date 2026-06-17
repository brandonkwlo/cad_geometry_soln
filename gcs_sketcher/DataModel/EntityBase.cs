// EntityBase.cs
// Mirrors EntityBase from sketch.h
// EntityBase is the core data-only class for all geometric entities.
// Entity (in Entity.cs) inherits from this and adds rendering/display fields.

namespace DataModel
{
    public enum EntityType : uint
    {
        // Points
        PointIn3D           = 2000,
        PointIn2D           = 2001,
        PointNTrans         = 2010,
        PointNRotTrans      = 2011,
        PointNCopy          = 2012,
        PointNRotAA         = 2013,
        PointNRotAxisTrans  = 2014,

        // Normals
        NormalIn3D          = 3000,
        NormalIn2D          = 3001,
        NormalNCopy         = 3010,
        NormalNRot          = 3011,
        NormalNRotAA        = 3012,

        // Distances
        Distance            = 4000,
        DistanceNCopy       = 4001,

        // Faces
        FaceNormalPt        = 5000,
        FaceXProd           = 5001,
        FaceNRotTrans       = 5002,
        FaceNTrans          = 5003,
        FaceNRotAA          = 5004,
        FaceRotNormalPt     = 5005,
        FaceNRotAxisTrans   = 5006,

        // Geometry
        Workplane           = 10000,
        LineSegment         = 11000,
        Cubic               = 12000,
        CubicPeriodic       = 12001,
        Circle              = 13000,
        ArcOfCircle         = 14000,
        TtfText             = 15000,
        Image               = 16000
    }

    public class EntityBase
    {
        // Max points any entity can reference (matches #define MAX_POINTS_IN_ENTITY 12)
        public const int MAX_POINTS_IN_ENTITY = 12;

        public int        Tag  { get; set; }
        public HEntity    H    { get; set; }

        public EntityType Type { get; set; }

        public HGroup  Group     { get; set; }
        public HEntity Workplane { get; set; } = HEntity.FREE_IN_3D;

        // The points that define this entity's geometry.
        // e.g. a LineSegment uses point[0]=start, point[1]=end
        // an ArcOfCircle uses point[0]=center, point[1]=start, point[2]=end
        public HEntity[] Point { get; set; } = new HEntity[MAX_POINTS_IN_ENTITY];
        public int       ExtraPoints { get; set; }

        // Reference to this entity's normal and distance sub-entities
        public HEntity Normal   { get; set; }
        public HEntity Distance { get; set; }

        // Solver parameters (points, normals, distances own their params)
        public HParam[] Param { get; set; } = new HParam[8];

        // Numerically evaluated values (used after solving)
        public Vector3d    NumPoint    { get; set; }
        public Quaternion  NumNormal   { get; set; }
        public double      NumDistance { get; set; }

        // For text entities
        public string  Str         { get; set; } = "";
        public string  Font        { get; set; } = "";
        public string  File        { get; set; } = "";
        public double  AspectRatio { get; set; }

        // For entities derived by transformation: how many times to apply it
        public int TimesApplied { get; set; }

        // --- Query helpers (mirror the methods in EntityBase) ---

        public bool IsPoint() =>
            Type >= EntityType.PointIn3D && Type <= EntityType.PointNRotAxisTrans;

        public bool IsNormal() =>
            Type >= EntityType.NormalIn3D && Type <= EntityType.NormalNRotAA;

        public bool IsDistance() =>
            Type == EntityType.Distance || Type == EntityType.DistanceNCopy;

        public bool IsWorkplane() => Type == EntityType.Workplane;

        public bool IsFace() =>
            Type >= EntityType.FaceNormalPt && Type <= EntityType.FaceNRotAxisTrans;

        public bool IsCircle() => Type == EntityType.Circle;

        public bool HasEndpoints() =>
            Type == EntityType.LineSegment ||
            Type == EntityType.ArcOfCircle ||
            Type == EntityType.Cubic       ||
            Type == EntityType.CubicPeriodic;

        public void Clear()
        {
            // Reset arrays to defaults
            Point = new HEntity[MAX_POINTS_IN_ENTITY];
            Param = new HParam[8];
        }
    }
}
