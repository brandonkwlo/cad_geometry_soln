// SlvsInterop.cs
// Translates the C structs and functions from slvs.h
// into C# structs and P/Invoke signatures.

using System.Runtime.InteropServices;

namespace DataModel
{
    // Mirrors Slvs_Entity from slvs.h (56 bytes, fully blittable).
    // The C struct has point[4] and param[4] arrays; expanded here as
    // individual fields so the struct is blittable and passes by value
    // cleanly across the P/Invoke boundary.
    [StructLayout(LayoutKind.Sequential)]
    internal struct SlvsEntity
    {
        public uint H;
        public uint Group;
        public int  Type;
        public uint Wrkpl;
        public uint Point0, Point1, Point2, Point3;   // point[4]
        public uint Normal;
        public uint Distance;
        public uint Param0, Param1, Param2, Param3;   // param[4]
    }

    // Mirrors Slvs_Constraint from slvs.h (56 bytes).
    [StructLayout(LayoutKind.Sequential)]
    internal struct SlvsConstraint
    {
        public uint   H;
        public uint   Group;
        public int    Type;
        public uint   Wrkpl;
        public double ValA;     // at offset 16, satisfies 8-byte alignment
        public uint   PtA;
        public uint   PtB;
        public uint   EntityA;
        public uint   EntityB;
        public uint   EntityC;
        public uint   EntityD;
        public int    Other;
        public int    Other2;
    }

    // Mirrors Slvs_SolveResult from slvs.h (12 bytes).
    [StructLayout(LayoutKind.Sequential)]
    internal struct SlvsSolveResult
    {
        public int Result;
        public int Dof;
        public int Nbad;
    }

    // P/Invoke declarations for the high-level slvs.dll API.
    // Uses EntryPoint to map clean C# names to the Slvs_-prefixed exports.
    internal static class Slvs
    {
        public const int ResultOkay            = 0;
        public const int ResultInconsistent    = 1;
        public const int ResultDidntConverge   = 2;
        public const int ResultTooManyUnknowns = 3;

        private const string Dll = "slvs";

        // Resets the global sketch state inside the DLL.
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Slvs_ClearSketch")]
        public static extern void ClearSketch();

        // Creates an XY workplane at the origin and returns it.
        // Must be called first; the returned entity is passed to every
        // AddPoint2D / AddLine2D / Parallel / Dragged call.
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Slvs_AddBase2D")]
        public static extern SlvsEntity AddBase2D(uint grouph);

        // Registers a 2D point. Returns the entity whose Param0/Param1
        // are the handles for the x and y solver parameters.
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Slvs_AddPoint2D")]
        public static extern SlvsEntity AddPoint2D(uint grouph, double u, double v, SlvsEntity workplane);

        // Registers a line segment between two 2D points.
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Slvs_AddLine2D")]
        public static extern SlvsEntity AddLine2D(uint grouph, SlvsEntity ptA, SlvsEntity ptB, SlvsEntity workplane);

        // Adds a parallel constraint between two 2D line segments.
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Slvs_Parallel")]
        public static extern SlvsConstraint Parallel(uint grouph, SlvsEntity entityA, SlvsEntity entityB, SlvsEntity workplane);

        // Pins a point at its current position (WHERE_DRAGGED constraint).
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Slvs_Dragged")]
        public static extern SlvsConstraint Dragged(uint grouph, SlvsEntity ptA, SlvsEntity workplane);

        // Runs the solver. Pass IntPtr.Zero for the bad-constraint output
        // when you don't need to diagnose failures.
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Slvs_SolveSketch")]
        public static extern SlvsSolveResult SolveSketch(uint hg, IntPtr bad);

        // Reads the solved value of a parameter by its handle.
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Slvs_GetParamValue")]
        public static extern double GetParamValue(uint ph);
    }
}
