// Handles.cs
// Mirrors all the hWhatever handle classes from sketch.h
// In C++, these are lightweight 32-bit IDs used to reference data structures.
// In C#, we use readonly structs for the same zero-overhead purpose.

namespace DataModel
{
    public readonly struct HEntity
    {
        public readonly uint V;
        public HEntity(uint v) { V = v; }

        public bool IsFromRequest() => (V & 0x80000000) == 0;
        public HRequest Request()   => new HRequest(V >> 16);
        public HGroup   Group()     => new HGroup((V >> 16) & 0x3fff);
        public HEquation Equation(int i) => new HEquation(V | 0x40000000 | (uint)i);

        public static readonly HEntity FREE_IN_3D = new HEntity(uint.MaxValue);
        public static readonly HEntity NO_ENTITY  = new HEntity(0);

        public override string ToString() => $"HEntity({V})";
    }

    public readonly struct HGroup
    {
        public readonly uint V;
        public HGroup(uint v) { V = v; }

        public HEntity   Entity(int i)   => new HEntity(0x80000000 | (V << 16) | (uint)i);
        public HParam    Param(int i)    => new HParam(0x80000000  | (V << 16) | (uint)i);
        public HEquation Equation(int i) => new HEquation((V << 16) | 0x80000000 | (uint)i);

        public static readonly HGroup HGROUP_REFERENCES = new HGroup(1);

        public override string ToString() => $"HGroup({V})";
    }

    public readonly struct HRequest
    {
        public readonly uint V;
        public HRequest(uint v) { V = v; }

        public HEntity Entity(int i) => new HEntity((V << 16) | (uint)i);
        public HParam  Param(int i)  => new HParam((V << 16)  | (uint)i);

        public override string ToString() => $"HRequest({V})";
    }

    public readonly struct HConstraint
    {
        public readonly uint V;
        public HConstraint(uint v) { V = v; }

        public HEquation Equation(int i) => new HEquation((V << 16) | (uint)i);
        public HParam    Param(int i)    => new HParam(V | 0x40000000 | (uint)i);

        public static readonly HConstraint NO_CONSTRAINT = new HConstraint(0);

        public override string ToString() => $"HConstraint({V})";
    }

    public readonly struct HParam
    {
        public readonly uint V;
        public HParam(uint v) { V = v; }

        public HRequest Request() => new HRequest(V >> 16);

        public override string ToString() => $"HParam({V})";
    }

    public readonly struct HEquation
    {
        public readonly uint V;
        public HEquation(uint v) { V = v; }

        public bool        IsFromConstraint() => (V & 0xc0000000) == 0;
        public HConstraint Constraint()       => new HConstraint(V >> 16);

        public override string ToString() => $"HEquation({V})";
    }

    public readonly struct HStyle
    {
        public readonly uint V;
        public HStyle(uint v) { V = v; }

        public override string ToString() => $"HStyle({V})";
    }
}
