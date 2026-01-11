# Performance

Purpose: discuss hotspots and diagnostics for WFC and chunk generation.

## Hot paths

- Entropy scan for the next cell (min remaining values).
- Propagation queue processing and neighbor constraint checks.
- Backtracking: undo application and re-propagation after rollbacks.
- Height sampling if constraints depend on elevation/biome.

## Practical tips

- Data layout: prefer arrays; keep structs small; reuse buffers to avoid per-iteration allocations.
- Precompute: adjacency tables and bitset-compatible masks to speed set intersections.
- Heuristics: random but deterministic tie-breakers; bias towards cells near solved regions.
- Limits: cap `maxIterations` and `maxBacktracks`; bail early on repeated contradictions.
- Cache: memoize height/biome samples within a chunk solve.

## Diagnostics with EventSource

Emit timings and counters around key phases to correlate spikes with inputs and settings.

```csharp
// TerrainPerformanceEventSource example emission points (IDs ≥ 10)
// _perf.WfcIterationStart(); _perf.WfcIterationStop();
// _perf.WfcPropagationStart(); _perf.WfcPropagationStop();
// _perf.WfcBacktrack(); _perf.WfcContradiction();
```

Monitor with `dotnet-counters` or `dotnet-trace` as described in the diagnostics README.

References:

- Diagnostics overview: see [Mapping Area](../README.md) and [Performance & Debugging](../../../../performance-and-debugging.md)
- Emission sites: see WFC and map-generation implementation notes

Navigation

- Up: [WFC README](README.md)
- Previous: [06 — Integration](06-integration.md)
