# CAD Geometric Solver

Project for Professor Babak Boloury. Developing a Geometric Solver that operates in tandem with existing CAD softwares (e.g. SolidWorks).

Leverages works from SolveSpace and Sketchflat. Specifically source code that calculates the geometry constraints and relationships.

## Summary

This project integrates SolveSpace's constraint solver (`libslvs`) into a standalone C# application to demonstrate constraint-based 2D sketching — the same technology used under the hood in professional CAD tools like SolidWorks and Fusion 360.

The current deliverable is a console app that:
- Defines two non-parallel lines as a sketch
- Applies a **Parallel constraint** between them
- Runs the real SolveSpace solver to satisfy the constraint
- Prints before/after coordinates showing the solver moved the free point
- Saves the solved sketch to `sketch.json` and reloads it

The solver is called via P/Invoke (C# interop with a native DLL). The `slvs.dll` is compiled from the SolveSpace open-source project and exposes a high-level API for building and solving constraint sketches.

## Future Tasks

| Priority | Task | Description |
|----------|------|-------------|
| 1 | More entity/shape types | Add support for arcs, circles, and cubic splines — the core shapes used in SolveSpace and SolidWorks sketches beyond simple line segments |
| 2 | More constraint types | Add Horizontal, Vertical, Equal Length, and Point-on-Line constraints to cover common sketching scenarios |
| 3 | Visual sketch viewer | A basic 2D canvas (WinForms or WPF) that draws lines and animates the solver's effect in real time |
| 4 | Multi-constraint solving | Verify the solver handles multiple simultaneous constraints correctly |
| 5 | SolidWorks API research | Investigate how SolidWorks exposes sketch geometry via its API to plan the integration path |

## How to Use

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022 with the **Desktop development with C++** and **.NET desktop development** workloads
- CMake 3.x (CMake 4.x is not supported by the SolveSpace build)

### Building `slvs.dll`

`slvs.dll` must be compiled from the SolveSpace source before running the app.

1. Configure the CMake build (disable GUI, CLI, and tests — only the solver library is needed):
   ```powershell
   cmake -S "path\to\solvespace" -B "C:\slvs_build" -A x64 -DENABLE_GUI=OFF -DENABLE_CLI=OFF -DENABLE_TESTS=OFF
   ```

2. Build the `slvs` target:
   ```powershell
   cmake --build "C:\slvs_build" --target slvs --config Debug
   ```

3. Copy the output DLL into the C# project folder:
   ```powershell
   $dest = "path\to\gcs_sketcher\gcs_sketcher\DataModel\slvs.dll"
   Copy-Item "C:\slvs_build\bin\Debug\slvs.dll" $dest
   ```

### Running the App

**Option 1 — Terminal:**
```powershell
cd "path\to\gcs_sketcher\gcs_sketcher\DataModel"
dotnet run
```

**Option 2 — Visual Studio:**
1. Open `gcs_sketcher\gcs_sketcher\gcs_sketcher.slnx`
2. Right-click the `DataModel` project in Solution Explorer → **Set as Startup Project**
3. Press **Ctrl+F5** to run

### Expected Output

```
=== Before Solve ===
  H(1)  (0.000, 0.000)
  H(2)  (10.000, 0.000)
  H(3)  (0.000, 5.000)
  H(4)  (15.000, 9.000)
  Line1 slope: 0.000000
  Line2 slope: 0.266667  (expected ~0.267)

Solver result: 0 (OKAY)

=== After Solve ===
  H(1)  (0.000, 0.000)
  H(2)  (10.000, 0.000)
  H(3)  (0.000, 5.000)
  H(4)  (15.000, 5.000)

=== Parallel Check ===
  Line1 slope: 0.000000
  Line2 slope: 0.000000
  Parallel:    True

Saved to sketch.json
Reloaded: 6 entities, 4 constraints
```

Point 4 moves from `(15, 9)` to `(15, 5)` — the solver adjusted it to satisfy the parallel constraint.
