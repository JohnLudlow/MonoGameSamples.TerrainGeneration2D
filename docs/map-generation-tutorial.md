# Map Generation Tutorial

This guide reproduces the entire terrain map generation experience and explains how the chunks, height maps, and Wave Function Collapse stages fit together. Follow each numbered step to go from zero to a live, instrumented 2048×2048 tile map.

1. **Refresh the build/content pipeline.**
   - Run `dotnet build TerrainGeneration2D.slnx` to compile every project, re-run the Gum/asset builder, and populate `TerrainGeneration2D.Content/bin/Debug/net10.0/Content`. The command shown in [docs/architecture-class-diagram.md](docs/architecture-class-diagram.md#L1-L26) is the canonical starting point for any change that touches art, audio, or UI definitions.

2. **Understand the core loop that drives the map.**
   - Read the architecture diagram and the `Core`/scene plumbing so you know why only one scene lives at a time and why `GameScene.Initialize` clears `GumService.Default.Root` before recreating the UI.
   - `TerrainGenerationGame` (see `TerrainGeneration2D/TerrainGenerationGame.cs`) enables diagnostics in DEBUG, preloads the theme song, boots Gum, and immediately pushes `GameScene`, which is where the chunked map is created.

3. **Run the game and watch the chunks appear.**
   - Launch `dotnet run --project TerrainGeneration2D/TerrainGeneration2D.csproj`. `GameScene.LoadContent` loads `images/terrain-atlas`, creates the `ChunkedTilemap`, centers `Camera2D`, and wires up the tooltip/debug helpers.
   - As soon as the camera moves, `GameScene.Update` forwards input through `GameController` helpers and calls `ChunkedTilemap.UpdateActiveChunks`, which keeps a 3×3 buffered window of chunks around the viewport.

4. **Follow how each chunk is born, cached, and saved.**
   - Open [TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs](TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs) to see the deterministic seed (`masterSeed + chunkX * 73856093 + chunkY * 19349663`), the gzipped save format (`Content/saves/map_{chunkX}_{chunkY}.dat`), and the `GenerateChunk` path.
   - When `useWaveFunctionCollapse` is true (default), the chunk calls `WfcProvider.Generate`; when WFC succeeds it copies the tile grid into the chunk, otherwise it falls back to `GenerateRandomChunk`, which samples `HeightMapGenerator` and maps heights via `TerrainRuleConfiguration`.
   - Inspect [`TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/WfcProvider.cs`](TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/WfcProvider.cs) to see how candidate tiles are constrained based on nearby decisions, weighted by neighbor matches, and then propagated through the `TileTypeRegistry` rules.
   - The WFC constraints depend on the tile types defined in [`TerrainGeneration2D.Core/Mapping/TileTypes/TileTypes.cs`](TerrainGeneration2D.Core/Mapping/TileTypes/TileTypes.cs) and the threshold-driven materials described in [`TerrainGeneration2D.Core/Mapping/TileTypes/TerrainRuleConfiguration.cs`](TerrainGeneration2D.Core/Mapping/TileTypes/TerrainRuleConfiguration.cs).

5. **Use the debug overlay to visualize active and dirty chunks.**
   - Press F12 (handled by `GameController.ToggleDebugOverlay`) to toggle the overlay drawn after the chunk rendering stage. The overlay copies `ChunkedTilemap.GetActiveChunkInfos` snapshots and draws rectangles for the buffered 3×3 range plus the viewport (see `TerrainGeneration2D/Scenes/GameScene.cs` for the debug draw helpers and the white/green boundary logic).
   - While the overlay is showing, pay attention to the `IsDirty` flag color (orange-red versus lime-green) to verify that saves are triggered when chunks leave the buffer.

6. **Force a regeneration to exercise both load and generate code.**
   - Exit the game, delete everything under `Content/saves`, and then rerun the game with the same command from step 3. The absence of saved chunk files forces `LoadChunk` to return null and sends every chunk through `GenerateChunk` again.
   - Zoom out or pan to send the camera across multiple chunk boundaries so `UpdateActiveChunks` hits both the load-then-save cycle (see `ChunkedTilemap.UpdateActiveChunks`) and the instrumentation hooks (`TerrainPerformanceEventSource.Log.*`).

7. **Tune the terrain parameters powering both the random fallback and WFC decisions.**
   - Adjust `TerrainRuleConfiguration` (beach width, mountain thresholds, noise requirements) and `HeightMapConfiguration` (noise scales/weights) to sculpt the map. After editing those values, instantiate `ChunkedTilemap` with the new configs in `GameScene.LoadContent` so they flow through the `HeightMapGenerator` and `TileTypeRegistry`.
   - For rapid iterations, consider exposing a Gum control that rebuilds the `ChunkedTilemap`: dispose the old instance, call `SaveAll`, delete the cached saves if needed, and construct a fresh tilemap with the updated configs.

8. **Observe the instrumentation that proves chunk work is happening.**
   - The DEBUG build already enables `Core.EnablePerformanceDiagnostics`, so the console prints the `TerrainPerformanceEventSource` events defined under `TerrainGeneration2D.Core/Diagnostics`. You can follow the step-by-step capture instructions in [docs/performance-and-debugging.md](docs/performance-and-debugging.md#L1-L62) to monitor the `active-chunk-count`/`chunks-saved-per-second` counters via `dotnet-counters` and to record `dotnet-trace` captures.

9. **Stretch goal: add a new tile type or rule.**
   - Add a new `TileType` implementation under `TerrainGeneration2D.Core/Mapping/TileTypes`, register it via `TileTypeRegistry.CreateDefault`, and ensure it exposes the evaluation logic you want (neighbor checks, height-based gating, group metrics via `MappingInformationService`). Then run steps 3–7 again to confirm the new tile participates in WFC and chunk saves.

By following these steps you reproduce a full map generation run, gain confidence in the deterministic chunk caching, and understand the knobs you can turn to tweak terrain behavior. Let me know if you want me to expand on the rebuild UI or instrumentation capture commands.