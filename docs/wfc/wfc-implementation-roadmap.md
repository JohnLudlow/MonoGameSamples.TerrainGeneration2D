# WFC Implementation Roadmap

## Current Implementation & Features
- Core game lifecycle: single fullscreen game launches and pushes a `GameScene`; scene switching managed in Core with proper disposal and `GC.Collect`.
- Rendering: `Camera2D` for transforms, `ChunkedTilemap` for generation + culling, `Tileset` + `TextureRegion` for atlas slicing.
- Terrain: Deterministic 2048x2048 terrain chunked into 64x64 tiles; 3x3 active buffer updated from `Camera2D.ViewportWorldBounds`.
- Persistence: Chunks saved/loaded as gzipped files under saves folder; `ChunkedTilemap.SaveAll()` on scene unload.
- Input: Centralized in `GameController` (movement, zoom, pan; F11 fullscreen; F12 debug overlay).
- UI: Gum-based UI rebuilt per scene; tooltip shows tile and chunk coordinates.
- Debug: Overlay draws active chunk borders and viewport bounds to validate culling.
- Content Pipeline: Assets built via Content project; load by relative names.
- Diagnostics: EventSource counters (`TerrainPerformanceEventSource`) and logging (`GenLog` source-generated); performance diagnostics enabled in DEBUG.
- Tests: Comprehensive unit tests pass; tutorials aligned with APIs.

Key files:
- [TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs](../../TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs)
- [TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/WfcProvider.cs](../../TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/WfcProvider.cs)
- [TerrainGeneration2D.Core/Diagnostics/TerrainPerformanceEventSource.cs](../../TerrainGeneration2D.Core/Diagnostics/TerrainPerformanceEventSource.cs)
- [TerrainGeneration2D/GameController.cs](../../TerrainGeneration2D/GameController.cs)

## WFC Evaluation (Current Algorithm)
- Domains: Each cell tracks a `HashSet<int>` of candidate tiles initialized from `TileTypeRegistry.ValidTileIds`.
- Selection: Chooses the next cell by lowest entropy (fewest candidates); breaks ties randomly.
- Collapse: Picks a tile with a simple weight heuristic favoring neighbors of the same type.
- Propagation: Breadth-first constraint propagation to neighbors; prunes candidates by `TileType.EvaluateRules(context)` with heightmap-aware context.
- Termination:
  - Success when all cells collapse.
  - Failure (contradiction) when a neighbor’s allowed set becomes empty or a cell has no candidates.
- Integration: `ChunkedTilemap` calls WFC per chunk and falls back to random fill on failure; emits WFC begin/end events.

Conclusion: This is a partial WFC implementation (observation + propagation) but lacks backtracking and global consistency across chunk seams. It is functional and performant for many cases, but not a full, robust WFC.

## Gaps vs. "Proper" WFC
- No backtracking: Contradictions abort rather than rewinding decisions.
- Entropy metric: Uses candidate count; not Shannon entropy $H = -\sum p_i \log p_i$ or enriched heuristics (e.g., tie-breaking by noise, neighborhood priors).
- Rule representation: Rules evaluated per `TileType` at runtime; adjacency relations could be precomputed lookup tables for speed and clarity.
- Queue discipline: Propagation queue is basic; AC-3 style arc-consistency with explicit arcs can be clearer and avoid redundant work.
- Chunk seam constraints: No explicit seam constraints to ensure continuity between neighboring chunks.
- Snapshot/reversion: No domain snapshots or undo log to support efficient backtracking.
- Performance: Height samples fetched per check; caching at chunk scope could reduce overhead. Rule evaluation could be hotspot.
- Instrumentation depth: Counters/logs exist, but finer-grained metrics (propagation steps, backtracks, contradictions count) would aid tuning.

## Work Required (Roadmap)
1. Backtracking & Solve Loop
- Add decision stack with `(x,y, candidates, index)`.
- Snapshot domains or maintain an undo log to revert changes cheaply.
- On contradiction, pop and try next candidate; terminate when all collapsed or no candidates remain.
- File: update [WfcProvider.cs](../../TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/WfcProvider.cs).

2. Entropy & Selection Heuristics
- Entropy metric:
  - Current: simple domain size (candidate count). The solver picks the cell with the fewest remaining candidates; ties are resolved via the injected randomness provider.
  - Option: Shannon entropy $H = -\sum p_i \log p_i$ using tile priors/weights to prefer cells with higher information gain.
  - Hybrid: Most Constrained (lowest entropy) + Most Constraining (highest impact on neighbors) for improved stability.
- Tie-breaking strategy:
  - Current: random among equal-entropy cells using `IRandomProvider` (see [WfcProvider.cs](../../TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/WfcProvider.cs)).
  - Deterministic mode: for testability, keep candidate ordering stable and inject a deterministic provider.
  - Alternatives: bias ties by screen/world position (e.g., top-left to bottom-right), Perlin noise, or distance to chunk center.
- Weight strategies (tile choice within a cell):
  - Current: neighbor-match boost, computed as `weight = 1 + 3 * (neighbor matches)`. Non-backtracking uses a weighted roll; backtracking orders candidates by weight desc then tile id asc and explores sequentially.
  - Tunable factors: adjust the base (`1`) and multiplier (`3`) to control locality vs. variation; consider diminishing returns (e.g., logarithmic boosts) to avoid streaking.
  - Context-aware weights: include heightmap class, biome, or global frequency penalties (e.g., soft caps to keep tile distribution balanced).
  - Probabilistic mixing: combine heuristics with `NextDouble()` (e.g., 80% heuristic, 20% uniform) to preserve diversity.
- Determinism & testing:
  - Use `IRandomProvider` with a deterministic implementation for unit tests; keep candidate ordering stable (sort by tile id when weights tie).
  - Avoid HashSet iteration nondeterminism by ordering before any selection; document tie-breakers explicitly.
- Practical tuning tips:
  - Start with small multipliers; increase only if contradictions persist or visuals look too noisy.
  - Monitor `wfc_contradictions`, `wfc_backtracks`, and `wfc_stats` via [TerrainPerformanceEventSource.cs](../../TerrainGeneration2D.Core/Diagnostics/TerrainPerformanceEventSource.cs); adjust weights to reduce rollback pressure.
  - Balance `maxBacktrackSteps` and `maxDepth` with heuristic aggressiveness; strong locality weights reduce branching but may raise contradiction hotspots.
  - Use the debug overlay to visually inspect collapse patterns; consider an entropy heatmap to identify problem areas.
 - See also: [05 — Heuristics](../map-generation/wfc/05-heuristics.md) for a focused deep-dive and "Try It" tips.

3. Rule Tables & AC-3 Propagation
- Precompute adjacency tables: `allowed[(tile, dir)] = {neighbors}` from `TileTypeRegistry`.
- Implement AC-3 with explicit arcs; enqueue arcs when a domain changes.
- Optimize to avoid repeated recomputation; intersect sets via bitsets for speed.

4. Chunk Seam Consistency
- Seed WFC domains at chunk edges from already-generated neighbor chunks.
- When generating chunk `(cx,cy)`, import boundary constraints from `(cx-1,cy)`, `(cx,cy-1)`, etc.
- Ensure persistence maintains seam constraints; handle regeneration coherently.
- Files: [ChunkedTilemap.cs](../../TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs) and [WfcProvider.cs](../../TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/WfcProvider.cs).

5. Heightmap Integration
- Cache `IHeightProvider` samples per chunk to avoid repeated calls.
- Consider rule stage separation: coarse biome pre-pass, then WFC with adjacency only.

6. Performance & Diagnostics
- Add counters: `wfc_observations`, `wfc_propagations`, `wfc_backtracks`, `wfc_contradictions`.
- Add timings per chunk for observation/propagation/backtracking phases.
- Files: [TerrainPerformanceEventSource.cs](../../TerrainGeneration2D.Core/Diagnostics/TerrainPerformanceEventSource.cs).

7. Tests (TDD)
- Domain initialization and entropy correctness.
- Propagation removes invalid neighbors; queue drains.
- Backtracking resolves constructed contradictions.
- Seam tests: adjacent chunks share consistent edges.
- Randomized fuzz tests under varied seeds/rule sets; assert no empty domains.
- Benchmarks: per-chunk solve time distribution.

8. Integration & UI
- Expose toggles to enable/disable WFC/backtracking; fall back controlled via config.
- Optional debug view: visualize domains/entropy heatmap during generation.

## Acceptance Criteria
- All chunks solve with WFC under default rules without falling back.
- No empty domains during solve; robust backtracking handles contradictions.
- Seams between chunks are consistent and reproducible across runs.
- Unit tests cover core behaviors; benchmarks show acceptable performance.
- Diagnostics surface meaningful counters and timings for tuning.
