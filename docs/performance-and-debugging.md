# Performance, Profiling, and Debugging Plans

## Profiling options

**EventSource + `dotnet-trace` / PerfView**: `TerrainPerformanceEventSource` now emits begin/end events for `ChunkedTilemap.UpdateActiveChunks`, `LoadChunk`, `SaveChunk`, and `WaveFunctionCollapse` (see the `UpdateActiveChunksBegin/End`, `ChunkLoadBegin/End`, `ChunkSaveBegin/End`, and `WaveFunctionCollapseBegin/End` payloads). `dotnet-trace` can capture those events and timeline views; pair with `dotnet-counters` to monitor the process-level CPU/GC/ThreadPool counters while scrolling manually. Because `EventSource` produces structured events, filter by the `EventName` and see durations in the TraceViewer, making it easy to spot chunk-save spikes.

**.NET Counters**: The same `TerrainPerformanceEventSource` exposes `EventCounter` metrics `active-chunk-count` and `chunks-saved-per-second` in addition to the built-in CPU/memory counters. `dotnet-counters monitor --process-id <pid>` can display those alongside CPU/memory, letting us correlate UI hitches with chunk load/save bursts, and the counters stream into `dotnet-trace` for later flame graph analysis.

**Tracy integration**: For frame-level profiling inside the MonoGame render loop, wrap `ChunkedTilemap` entry points with Tracy `Zone` macros (requires adding the Tracy C# bindings or using a thin native agent). Tracy captures per-frame timelines, allows zooming on slow frames, and can be toggled on/off via command-line or config. Document this integration so new builds can enable Tracy only when needed, keeping regular builds lean.

- ### Sample commands

- `dotnet-counters ps` to enumerate running `.NET` processes and grab the PID for TerrainGeneration2D. The `dotnet-counters list` subcommand no longer reports per-process counters—it just prints the documentation link shown above—so rely on `ps` + `monitor` instead.

- `dotnet-counters monitor --process-id <pid> --refresh-interval 1 --counters "JohnLudlow.TerrainGeneration2D.Performance-active-chunk-count" "JohnLudlow.TerrainGeneration2D.Performance-chunks-saved-per-second" "System.Runtime-cpu-usage" "System.Runtime-working-set"` will sample those counters each second; the CLI shows the pause/resume/quit instructions right away and then prints updated values on each refresh tick, so keep it running to see the custom counters appear once `ChunkedTilemap.UpdateActiveChunks` runs and `ReportActiveChunkCount` fires.
- `dotnet-trace collect --process-id <pid> --providers "JohnLudlow.TerrainGeneration2D.Performance::Informational" --format nettrace --output terrain-events.nettrace` captures all events emitted by `TerrainPerformanceEventSource`; open the resulting trace in PerfView (or `dotnet-trace convert terrain-events.nettrace --format speedscope`) to inspect `ChunkLoadBegin/End`, `ChunkSaveBegin/End`, and `UpdateActiveChunks` durations.
- After the trace completes, run `dotnet-trace ps` (or the task manager) to verify the process ID and reopen the trace if needed; you can also replay the timeline with `perfview /trace:terrain-events.nettrace` or `speedscope terrain-events.speedscope.json` if you convert the file.

## Benchmark extension

- The benchmark suite in [TerrainGeneration2D.Benchmarks/Program.cs](../TerrainGeneration2D.Benchmarks/Program.cs) now exposes parameterized scenarios:
  - Map sizes: 512, 1024, 2048 tiles per side
  - Entropy strategy: Domain, Shannon, Combined (Domain+Shannon with tie-break)
  - WFC time budget per chunk: 20ms, 50ms, 100ms
  - Toggle WFC on/off to compare against random fallback
 These run as a Cartesian matrix for each scenario to compare throughput, allocations, and IO behavior.

- Scenarios:
  - `GenerateChunkedTerrain`: Generates the full map’s active region then saves, useful for raw generation throughput.
  - `GenerateAndScrollChunks`: Seeds a 2×2-chunk viewport, then performs eight one-chunk scrolls before saving, approximating camera panning and exercising the active-chunk buffer logic.

- Run benchmarks (Release) locally:
  - Full suite

  ```bash
  dotnet run -c Release --project TerrainGeneration2D.Benchmarks -- --job short
  ```

  - Filter by scenario

  ```bash
  dotnet run -c Release --project TerrainGeneration2D.Benchmarks -- --filter *GenerateAndScrollChunks*
  ```

  - Filter by parameter (e.g., only Shannon entropy)

  ```bash
  dotnet run -c Release --project TerrainGeneration2D.Benchmarks -- --filter "*Shannon*"
  ```

  Tip: Parameter values appear in the benchmark display names (e.g., `MapSizeInTiles=1024, Strategy=Shannon, TimeBudgetMs=50`), so you can filter by those substrings.

- Counter capture in benchmarks:
  - Benchmarks attach an EventPipe profiler provider for `JohnLudlow.TerrainGeneration2D.Performance` so custom EventSource counters/events (e.g., `active-chunk-count`, `chunks-saved-per-second`) are recorded during runs.
  - Artifacts are stored alongside benchmark results; open the trace with PerfView or `speedscope` to correlate generation steps with counter changes.

- Result tips:
  - Use the allocated bytes/op and Gen0/Gen1 counts to compare Domain vs Shannon when the time budget is tight (20ms) vs more generous (100ms).
  - Compare `GenerateAndScrollChunks` across map sizes to understand the impact of chunk buffer churn and save frequency.

## Debugging overlay plan

- Build a Gum/MonoGame overlay that draws current chunk bounds using `ChunkedTilemap.TileToChunkCoordinates` + `Chunk.ChunkSize`. White lines can highlight the 3×3 buffered area and the currently rendered viewport. Maintain a boolean `ShowDebugBounds` in `GameScene` that toggles when F12 is pressed (listen in `Update()` once per frame). When enabled, draw the overlay after chunk rendering (e.g., using `SpriteBatch.DrawRectangle` helpers or a simple pixel texture). Add tooltips showing chunk coordinates and whether they are loaded or pending save.
- Tie the overlay toggle to `GameController` (new `IsDebugOverlayActive()` method) so input handling stays centralized. Persist the state in `GameSceneUI` if you want to display the status in the Gum tree (e.g., a corner label that says "Debug Bounds: ON/OFF"). This UI helps confirm which chunks are active and if chunk saves are blocking scrolls.

## Terrain parameter tuning UI plan

- Extend `GameSceneUI` (or create a dedicated Gum panel) with sliders/dropdowns for `TerrainRuleConfiguration` and `HeightMapConfiguration` values: ocean/plains/mountain height thresholds, `MountainNoiseThreshold`, and the `HeightMapConfiguration` scales/weights. Each slider change writes back to a live `TerrainRuleConfiguration` instance that the current `ChunkedTilemap` references (or triggers a scene reload). Because the tilemap regenerates from saved chunk files, offer a "Regenerate" button that deletes `Content/saves` and rebuilds the visible chunks at the current camera position.
- For instant feedback during play, allow the UI to create a new `ChunkedTilemap` with the updated config upon button press, and swap it into `GameScene` (dispose the old tilemap, `SaveAll`, then instantiate a fresh one). Keep the UI out of the critical render path by executing the rebuild on a background task or frame queue, showing a busy indicator until the new chunks are ready.
- Document the UI controls (slider ranges, units) near the debugging overlay so testers know how to dial up ocean vs. mountain coverage without touching code.

## Benchmark CLI Overrides

- Use flags to override parameter sources during runs:
  - `--size`: map size in tiles per side (e.g., 512, 1024, 2048)
  - `--strategy`: entropy selection (`domain`, `shannon`, `combined`)
  - `--budget`: WFC time budget per chunk in ms (e.g., 25, 50, 100)
  - `--wfc`: enable WFC (`true`/`false`)
  - `--influenceSingle`: enable single-tile influence (`true`/`false`)
  - `--centerBias`: enable center bias (`true`/`false`)
  - `--uniform`: fraction [0.0–1.0] for uniform vs weighted selection
  - `--bias`: MostConstrainingBias weight (e.g., 0.0, 0.5)

- Examples:
  - Fast, targeted Shannon run
    - `dotnet run -c Release --project TerrainGeneration2D.Benchmarks -- --fast --size 1024 --strategy shannon --budget 25 --wfc true --centerBias true --uniform 0.25 --bias 0.5`
  - Full-size domain run
    - `dotnet run -c Release --project TerrainGeneration2D.Benchmarks -- --size 2048 --strategy domain --budget 50 --wfc true`
