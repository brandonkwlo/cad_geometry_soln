// Sketch.cs
// The top-level container for a sketch.
// In SolveSpace the equivalent data lives in SolveSpaceUI (the global SS object)
// as lists: SS.entity, SS.constraint, SS.request, SS.param, SS.group.
// We collect them here in one Sketch class for cleanliness.

using System.Text.Json;

namespace DataModel
{
    public class Sketch
    {
        // Grid settings — used by a future visual layer for snapping
        public bool   SnapToGrid  { get; set; } = false;
        public double GridSpacing { get; set; } = 1.0;

        // All geometric entities in this sketch (points, lines, circles, arcs…)
        public List<Entity>     Entities    { get; set; } = new();

        // All constraints (parallel, coincident, distance, angle…)
        public List<Constraint> Constraints { get; set; } = new();

        // All requests (the user operations that produced the entities)
        public List<Request>    Requests    { get; set; } = new();

        // --- Save / Load (JSON) ---

        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            WriteIndented        = true,
            IncludeFields        = false,
        };

        public void SaveToJson(string path)
        {
            string json = JsonSerializer.Serialize(this, _jsonOpts);
            File.WriteAllText(path, json);
        }

        public static Sketch LoadFromJson(string path)
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Sketch>(json, _jsonOpts)
                   ?? throw new InvalidDataException("Failed to deserialize sketch.");
        }

        // --- Lookup helpers ---

        public Entity?     FindEntity    (HEntity     h) => Entities   .FirstOrDefault(e => e.H.V == h.V);
        public Constraint? FindConstraint(HConstraint h) => Constraints.FirstOrDefault(c => c.H.V == h.V);
        public Request?    FindRequest   (HRequest    h) => Requests   .FirstOrDefault(r => r.H.V == h.V);

        // All entities that are points
        public IEnumerable<Entity> Points =>
            Entities.Where(e => e.IsPoint());

        // All entities that are line segments
        public IEnumerable<Entity> Lines =>
            Entities.Where(e => e.Type == EntityType.LineSegment);
    }
}
