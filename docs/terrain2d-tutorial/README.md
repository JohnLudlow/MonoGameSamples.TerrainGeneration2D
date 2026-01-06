# Procedural 2D Terrain Tutorial (from scratch)

This multi-part tutorial walks you from an empty folder to a working 2D strategy-style terrain with:
- Tile rendering using MonoGame (DesktopGL)
- Deterministic map generation
- Adjacency rules and (optional) Wave Function Collapse
- Noise-based heightmaps
- Diagnostics using .NET EventSource counters
- Basic structured logging

We encourage test-driven development (TDD): each phase includes a minimal test you can write first, then implement the feature to pass the test.

## Purpose
This tutorial is designed so that you can implement the same functionality in your own, separate repository without further reference to this codebase. Each phase is standalone, with explanations and file paths, and can be followed independently.

## Conventions
- Keep coding standards consistent across snippets.
- Respect local settings such as `.editorconfig` when you create your own repo.
- Prefer `var` for locals where the type is evident.
- MonoGame method order where present: `Initialize`, `LoadContent`, `Update`, `Draw`, then unload/cleanup.
- Class member order: private fields, properties, constructors, public methods, private methods.
- Field naming: `_camelCase` with a leading underscore (including `const`).
- Naming: classes and public members use `PascalCase`.
- Never use public settable member fields; prefer get-only properties where possible.
- Where useful, include meaningful XML documentation comments.
- Snippets lead with an explanation and the project/filename; when pasting into the middle of a longer file, we note that earlier code is omitted for brevity.

## Prerequisites
- .NET 10 SDK installed
- Git (optional)
- Windows/Mac/Linux with OpenGL support

Verify:
```bash
 dotnet --version
```
Should print 10.x.y.

## What you will build

High-level architecture (plain text):

- Game Host (MonoGame)
  - Creates a window and runs the update/draw loop
  - Calls into the Tilemap Renderer to draw the world
- Tilemap Renderer
  - Draws visible tiles based on camera/viewport
- Map Data
  - 2D array (and later chunks) storing tile IDs
- Generators (choose or combine)
  - Random Fill (deterministic with a seed)
  - Adjacency Rules (greedy neighbor constraints)
  - Wave Function Collapse (optional, constraint-based)
  - Heightmap (noise-driven biomes)
- Diagnostics
  - EventSource Counters (active tiles/chunks, saves/sec)
  - Logging (console, structured messages)

Simple view of data flow:
```
[Game Host] --> [Tilemap Renderer] --> (reads) --> [Map Data]
                                     ^
                                     |
                         [Generators & Rules]

[Game Host] --> [Diagnostics]
               (EventSource counters, logging)
```

## Phases
1. 01-Setup: Project bootstrap and blank powder-blue screen (MonoGame)
2. 02-SingleTile: Fill the screen with a single tile (basic drawing)
3. 03-Logging: Add structured logging early so later steps are observable
4. 04-Performance: Add EventSource counters early (monitor with dotnet-counters)
5. 05-RandomTiles: Fill the map from a tileset at random (deterministic seed) [TDD]
6. 06-AdjacencyRules: Apply adjacency / nearness rules [TDD]
7. 07-Heightmap: Apply noise-based heightmap rules [TDD]
8. 08-WFC-Domains: Initialize per-cell candidate domains and entropy heuristics [TDD]
9. 09-WFC-Propagation: Implement constraint propagation (arc-consistency) [TDD]
10. 10-WFC-Backtracking: Handle contradictions with decision stack and backtracking [TDD]
11. 11-WFC-Integration: Integrate WFC into chunk generation and performance instrumentation [TDD]
12. 12-Config-WFC-Weights: Runtime configuration for WFC heuristic weights (appsettings)

Each phase is self-contained and includes complete code snapshots and at least one unit test. You can start from any phase by applying that phase’s snapshot.

Navigation:
- Start here: [01-setup.md](./01-setup.md)
- Full index: this page
- Cross-links: Each phase ends with “See also” links to previous/next phases and this index.

## Getting Started
Open the first phase:
- [01-setup.md](./01-setup.md)

Then enable observability:
- [03-logging.md](./03-logging.md)
- [04-performance.md](./04-performance.md)

Continue building the map (with tests):
- [02-single-tile.md](./02-single-tile.md)
- [05-random-tiles.md](./05-random-tiles.md)
- [06-adjacency-rules.md](./06-adjacency-rules.md)
- [07-heightmap.md](./07-heightmap.md)

Expand to WFC stages:
- [08-wfc-domains.md](./08-wfc-domains.md)
- [09-wfc-propagation.md](./09-wfc-propagation.md)
- [10-wfc-backtracking.md](./10-wfc-backtracking.md)
- [11-wfc-integration.md](./11-wfc-integration.md)
- [12-config-wfc-weights.md](./12-config-wfc-weights.md)

## Heuristics
- Cell selection: default is lowest domain size. See Shannon entropy option and trade-offs in [map-generation/wfc/05-heuristics.md](../map-generation/wfc/05-heuristics.md).
- Tile choice: neighbor-match weighting is configurable via `WfcWeights` in [TerrainGeneration2D/appsettings.json](../../TerrainGeneration2D/appsettings.json).
- Runtime config: quick tuning guide in [12-config-wfc-weights.md](./12-config-wfc-weights.md).
- Determinism: inject a test `IRandomProvider` for reproducible tie-breaks; see examples in [WfcProvider.cs](../../TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/WfcProvider.cs) and tests.

Refer to the tests appendix for project setup and commands:
- [tests-appendix.md](./tests-appendix.md)

If you get stuck, each phase ends with a Troubleshooting section.
