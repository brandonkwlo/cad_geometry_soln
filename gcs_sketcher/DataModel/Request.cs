// Request.cs
// Mirrors the Request class from sketch.h.
// A Request is a user operation (draw a line, draw a circle, etc.)
// that generates one or more Entity objects.
// Every entity in the sketch is owned by a request.

namespace DataModel
{
    public enum RequestType : uint
    {
        Workplane        = 100,
        DatumPoint       = 101,
        LineSegment      = 200,
        Cubic            = 300,
        CubicPeriodic    = 301,
        Circle           = 400,
        ArcOfCircle      = 500,
        TtfText          = 600,
        Image            = 700,
        Ellipse          = 800,
        PartialEllipse   = 900
    }

    public class Request
    {
        // Predefined requests present in every sketch (the reference axes)
        public static readonly HRequest HREQUEST_REFERENCE_XY = new HRequest(1);
        public static readonly HRequest HREQUEST_REFERENCE_YZ = new HRequest(2);
        public static readonly HRequest HREQUEST_REFERENCE_ZX = new HRequest(3);

        public int         Tag  { get; set; }
        public HRequest    H    { get; set; }

        public RequestType Type        { get; set; }
        public int         ExtraPoints { get; set; }

        // Which workplane this request lives in (or FREE_IN_3D for 3D)
        public HEntity Workplane { get; set; } = HEntity.FREE_IN_3D;
        public HGroup  Group     { get; set; }
        public HStyle  Style     { get; set; }

        // Construction lines are helpers — they constrain but aren't part of the profile
        public bool Construction { get; set; }

        // For text requests
        public string Str         { get; set; } = "";
        public string Font        { get; set; } = "";
        public string File        { get; set; } = "";
        public double AspectRatio { get; set; }

        // Index of this request within its group
        public int GroupRequestIndex { get; set; }

        public string DescriptionString()
        {
            return Type switch
            {
                RequestType.LineSegment  => "line segment",
                RequestType.Circle       => "circle",
                RequestType.ArcOfCircle  => "arc of circle",
                RequestType.Cubic        => "cubic bezier",
                RequestType.Workplane    => "workplane",
                RequestType.DatumPoint   => "datum point",
                RequestType.TtfText      => $"text \"{Str}\"",
                _                        => $"request [{Type}]"
            };
        }

        public void Clear() { }
    }
}
