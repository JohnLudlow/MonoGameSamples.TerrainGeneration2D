# 08 — WFC Deep Dive

## Table of contents

- [Core concepts](#core-concepts)
- [Data structures & algorithm outline](#data-structures--algorithm-outline)
- [Heuristics and tie-breaking](#heuristics-and-tie-breaking)
- [Backtracking details](#backtracking-details)
- [Time budgets and trade-offs](#time-budgets-and-trade-offs)
- [Fallback logic](#fallback-logic)
- [Diagnostics and logging](#diagnostics-and-logging)
- [Example: expanded C# generate loop](#example-expanded-c-generate-loop)
- [Concrete tuning examples](#concrete-tuning-examples)
- [Manual validation checklist](#manual-validation-checklist)
- [Extending WFC safely](#extending-wfc-safely)
- [References](#references)

## Definition of terms

| Term | Meaning |
| ---- | ------- |
| Domain | Set of candidate tile IDs for a cell |
| Backtracking | Undoing previous choices to try alternatives |
| Time budget | Upper bound on CPU time allowed for WFC per chunk |

This deep-dive explains the internal WFC mechanics used by this project, how backtracking and time budgets are applied, the data structures and heuristics that matter for tuning, and concrete examples to help reproduce or extend behavior.

Audience: engine developers who will tune WFC parameters, add new tile rules, or diagnose generation failures.

## Core concepts

- Domain: the set of candidate tile IDs a cell may take. Represented in the implementation as a compact bitset or boolean array.
- Entropy: a measure of uncertainty for a domain; lower entropy means fewer candidates. In practice this implementation uses a weighted score combining candidate count and heuristic biases (see Heuristics).
- Propagation: the process of applying adjacency rules after collapsing a cell to prune neighbors' domains. Implemented with a queue of affected cells processed until stable.
- Collapse: reducing a domain to a single tile ID (the chosen value). Performed by sampling from the domain according to weighted probabilities.
- Backtracking: undoing one or more earlier collapses to resolve contradictions. The provider records collapse history and domain snapshots (or diffs) to revert state.
- Time budget: an upper bound (TimeSpan) passed to the WFC Generate call that causes the provider to abort and report failure when exceeded; callers typically use this to avoid frame stalls.

## Data structures & algorithm outline

- Grid of cells: width × height cells, each with a Domain and an "isCollapsed" flag.
- Adjacency rules: for each tile ID, a list or bitmask of allowed neighbor IDs per direction (N/E/S/W). These are consulted during propagation to filter neighbor domains.
- Propagation queue: a FIFO queue of cells whose domain changes require neighbor updates.
- Collapse history: a stack of changes (cell index, previous domain) used for backtracking.

High-level pseudo-flow (mirrors production code):

1) Initialize domains to full set of tile IDs. 2) While uncollapsed cells remain:
   a) Find cell with lowest entropy (tie-break by heuristics + PRNG).
   b) Collapse cell to a single tile (weighted sample).
   c) Push domain snapshot to history if backtracking enabled.
   d) Enqueue neighbors and propagate constraints until queue empty.
   e) If any domain becomes empty → contradiction:
       - If backtracking enabled: pop history up to a prior safe point and retry (respecting maxBacktrackSteps/maxDepth).
       - Otherwise: abort and report failure.
3) Return success when all cells collapsed or failure if contradicted without recoverable backtracking.

## Heuristics and tie-breaking

- Primary selection: fewest-candidate rule (minimum remaining values) is used to choose the next cell.
- Tie-breakers: deterministic options include preferring cells with higher adjacency influence, proximity to seeded features from the heightmap, or simple PRNG-based selection for variety.
- Weighting formula (example used in docs/features/mapping/chunked-tilemap.md):

  H = w_c * C + w_m * M + w_d * D

  - C: normalized candidate count (lower is preferred)
  - M: match to heightmap or biome influence
  - D: distance or other domain-specific bias
  - w_*: configurable weights via WfcWeights/Heuristics

- Sampling within a cell when collapsing: sample among remaining candidates using per-tile weights (WfcWeights) to bias selection toward preferred tiles.

## Backtracking details

- Backtracking is optional but enabled by default in chunk generation for robustness (see ChunkedTilemap.GenerateChunk).
- The provider records a bounded history of prior collapses; limits are configured via MaxBacktrackSteps and MaxDepth to avoid unbounded compute or memory.
- Practical behavior: backtracking often resolves local contradictions arising from tight constraints, but if the configuration is inconsistent globally backtracking is unlikely to find a solution.

## Time budgets and trade-offs

- Time budgets (WfcTimeBudgetMs) are intended to cap per-chunk work. If a time budget is exceeded the provider returns failure for the chunk and callers fall back to the deterministic random generator.
- Trade-offs:
  - Lower time budget → faster frames, more fallbacks and less coherent patches.
  - Higher time budget → better success/consistency, risk of frame stalls if executed synchronously.
- Recommendation: start with the project default (50 ms) and adjust by profiling; use RegenerateChunksInView to re-run generation during tuning.

## Fallback logic

- On failure (contradiction or time budget exceeded) the chunk generator calls a simpler deterministic generator that uses the heightmap and tile rules (see ChunkedTilemap.GenerateRandomChunk). The fallback produces plausible terrain quickly and marks the chunk dirty so it will be saved.

## Diagnostics and logging

- Event hooks emitted by the implementation (examples):
  - TerrainPerformanceEventSource.Log.WaveFunctionCollapseBegin(cx, cy)
  - TerrainPerformanceEventSource.Log.WaveFunctionCollapseEnd(cx, cy, success)
  - GameLoggerMessages.MapGenerateBegin / MapGenerateEnd
  - Chunk load/save events (ChunkLoadBegin/End, ChunkSaveBegin/End)
- Use the F12 overlay to visually inspect chunk boundaries and dirty state; correlate log events to chunk coordinates to diagnose failures.

## Example: expanded C# generate loop

(with backtracking-timebudget semantics)

```csharp
bool Generate(TimeSpan timeBudget)
{
    var stopwatch = Stopwatch.StartNew();
    var history = new HistoryStack();

    while (!AllCollapsed())
    {
        if (stopwatch.Elapsed > timeBudget) return false; // signal timeout

        var (x,y) = FindLowestEntropy();
        if (x == -1) return true;

        var snapshot = CaptureDomainSnapshot(x,y);
        history.Push(snapshot);

        if (!CollapseCell(x,y))
        {
            if (!Backtrack(history)) return false; // unresolvable
            continue;
        }

        if (!Propagate(x,y))
        {
            if (!Backtrack(history)) return false;
        }
    }

    return true;
}
```

## Concrete tuning examples

- Conservative: WfcTimeBudgetMs = 20, EnableBacktracking = false — fast, more fallbacks.
- Balanced (default): WfcTimeBudgetMs = 50, EnableBacktracking = true, MaxBacktrackSteps = 2048 — generally good.
- Quality: WfcTimeBudgetMs = 200+, EnableBacktracking = true, MaxBacktrackSteps = 8192 — slower but more consistent patches.

## Manual validation checklist

- Toggle the F10 panel and change WfcTimeBudgetMs; run Regenerate Visible Chunks and watch logs for WaveFunctionCollapseBegin/End to confirm behavior.
- If many fallbacks appear, raise the time budget or enable backtracking.
- Use F12 overlay to check dirty/clean chunk states after regeneration.
- To force a global re-generation, ClearAllSavedChunks and restart the scene.

## Extending WFC safely

- When adding new tiles or adjacency rules: update TileTypeRegistry and add unit tests for adjacency constraints to avoid introducing contradictions.
- When changing weights/heuristics: prefer iterative small adjustments and validate via RegenerateChunksInView rather than sweeping global changes.

## References

- Implementation anchors: TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/WfcProvider.cs and TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs
- See also: docs/features/mapping/wfc-config-examples.md for recommended parameter presets.
