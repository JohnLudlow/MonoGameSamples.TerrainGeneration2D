# Chunked Tilemap

## Table of contents

- [Overview](#overview)
- [Intent & Use-Cases](#intent--use-cases)
- [Architecture & Data Flow](#architecture--data-flow)
- [Domain Terms](#domain-terms)
- [Configuration](#configuration)
- [Algorithms & Math](#algorithms--math)
- [Examples](#examples)
- [Performance Notes](#performance-notes)
- [Follow-ups / Decisions](#follow-ups--decisions)
- [Changelog](#changelog)

## Definition of terms

| Term | Meaning |
| ---- | ------- |
| Chunk | fixed-size tile block (64×64) with a world tile origin |
| Active buffer | expanded set of chunks kept in memory around the viewport |
| Dirty chunk | chunk with unsaved changes |

## Overview

- Manages a large 2D map by dividing it into fixed-size chunks to balance memory
  usage and generation time.
- Uses deterministic generation per chunk and persists results to disk for fast
  reloads.

## Intent & Use-Cases

- Efficiently render and update visible terrain while culling distant areas.
- Persist generated chunks to avoid re-running generation on subsequent sessions.
- Regenerate visible chunks after tuning heuristics/runtime settings to preview
  effects.

## Architecture & Data Flow

- Scene lifecycle integrates via GameScene: camera drives viewport → tilemap
  loads/unloads chunks around the view.
- Core services: graphics/content/input/audio initialized via Core; `ChangeScene`
  enforces disposal + GC before init.
- Key files (paths): TerrainGeneration2D.Core/Core.cs,
  TerrainGeneration2D/Scenes/GameScene.cs,
  TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs,
  TerrainGeneration2D.Core/Graphics/Camera2D.cs.
- Flow: UpdateActiveChunks(viewport) → load/generate chunks → mark dirty → save/unload
  on buffer exit → draw visible tiles.

## Domain Terms

- Chunk: fixed-size tile block (64×64) with local indices and a world tile origin.
- Active buffer: expanded set of chunks around the viewport kept in memory.
- Dirty chunk: chunk with unsaved changes; eligible for persistence.

## Configuration

- Driven by TerrainGeneration2D/appsettings.json sections: WfcWeights,
  TerrainRules, HeightMap, Heuristics, WfcRuntime.
- Runtime toggles: F10 settings panel for heuristics/time budget; clear saves;
  regenerate visible chunks.

## Algorithms & Math

- Deterministic seed per chunk: `seed = masterSeed + cx*73856093 + cy*19349663`.
- WFC entropy/influence heuristics and weights determine collapse order and tile
  selection.
- Example entropy weighting:

$$
H = w_c C + w_m M + w_d D
$$

## Examples

- Minimal usage sketch with XML docs for API intent:

```csharp
/// <summary>
/// Updates active chunk set based on the camera viewport and persists/unloads
/// distant chunks.
/// </summary>
/// <param name="viewportWorldBounds">Current camera-aligned world bounds.</param>
public void UpdateActiveChunks(Rectangle viewportWorldBounds)
{
  // See TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs
}

/// <summary>
/// Regenerates chunks in view using current heuristics, optionally overwriting
/// saves.
/// </summary>
/// <param name="viewportWorldBounds">World bounds to expand and refresh.</param>
/// <param name="overwriteSaves">Overwrite existing saves when true.</param>
public void RegenerateChunksInView(Rectangle viewportWorldBounds, bool overwriteSaves = true)
{
  // See TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs
}
```

## Performance Notes

- Avoid LINQ/boxing in per-frame loops (Update/Draw); minimize allocations.
- Bound WFC work with `WfcTimeBudgetMs` to maintain frame time.
- Use diagnostics (ConsoleEventListener, dotnet-counters/dotnet-trace) when changing chunking/WFC.

## Follow-ups / Decisions

- Trade-offs: larger buffer reduces thrash but increases memory; tune by viewport and device.
- Decide save overwrite on regeneration based on need for reproducibility vs iteration speed.

## Changelog

- 2026-01-09: Initial feature doc added.
- 2026-01-30: Clarified runtime behavior: WFC backtracking, fallback generation, time budget, and save/regeneration behavior.

---

Implementation notes

- The runtime implementation (ChunkedTilemap) enables WFC backtracking during chunk generation and will fall back to randomized generation if WFC fails for a chunk; generated chunks are marked dirty and saved via SaveChunk.
- WFC work is bounded by the WfcTimeBudgetMs property (default 50 ms) to avoid long frame stalls.
- RegenerateChunksInView(viewportWorldBounds, overwriteSaves = true) regenerates the expanded viewport region and will overwrite existing saved chunk files when overwriteSaves is true.
- ClearAllSavedChunks removes saved files matching the pattern `map_*_*.dat` in the configured save directory to force regeneration on next load.

Validation

- Link this doc from the index and validate:
  - Update docs/features/mapping/README.md
  - Run:

```powershell
scripts/check-doc-links.ps1
```
