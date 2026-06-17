// MathTypes.cs
// Simple math structs needed by EntityBase and ConstraintBase.
// SolveSpace uses its own Vector and Quaternion types (in dsc.h / vec.h).
// We translate only what's needed for the data model.

namespace DataModel
{
    /// <summary>
    /// Mirrors SolveSpace's Vector struct (a 3D vector/point).
    /// </summary>
    public struct Vector3d
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Vector3d(double x, double y, double z)
        {
            X = x; Y = y; Z = z;
        }

        public static readonly Vector3d Zero = new Vector3d(0, 0, 0);

        public override string ToString() => $"({X}, {Y}, {Z})";
    }

    /// <summary>
    /// Mirrors SolveSpace's Quaternion struct.
    /// Represents a rotation as w + xi + yj + zk.
    /// </summary>
    public struct Quaternion
    {
        public double W  { get; set; }
        public double VX { get; set; }
        public double VY { get; set; }
        public double VZ { get; set; }

        public Quaternion(double w, double vx, double vy, double vz)
        {
            W = w; VX = vx; VY = vy; VZ = vz;
        }

        public static readonly Quaternion Identity = new Quaternion(1, 0, 0, 0);

        public override string ToString() => $"({W}, {VX}, {VY}, {VZ})";
    }
}
