# Full Terrain Generation Tutorial

This tutorial walks through building the TerrainGeneration2D sample from scratch: set up the **project solution**, wire the **core services**, implement the **chunked map and WFC generator**, connect the **Gum UI**, and finish with the **Debug/diagnostics flow** that mirrors the running sample. Follow each step, referencing the existing files for implementation details.

1. **Start in an empty folder**
   - Create a new directory and open it in VS Code.
   - Initialize a Git repo if desired.
   - Install required tools: [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) + [MonoGame.DesktopGL](https://dotnet.microsoft.com/en-us/apps/games/monogame) templates.

2. **Scaffold the solution**
   - Run `dotnet new sln -n TerrainGeneration2D`.
   - Create the core projects:

     ```bash
     dotnet new mgdesktopgl -o TerrainGeneration2D
     dotnet new classlib -o TerrainGeneration2D.Core
     dotnet new classlib -o TerrainGeneration2D.Content
     dotnet new xunit -o TerrainGeneration2D.Tests
     dotnet new console -o TerrainGeneration2D.Benchmarks
     ```

   - Add them to the solution (e.g., `dotnet sln add TerrainGeneration2D/TerrainGeneration2D.csproj ...`).
   - Copy the `dotnet-tools.json` from this repo or register the needed global tools (Gum registry/builder) in the content project.

3. **Wire the Core services**
   - In `TerrainGeneration2D.Core/Core.cs`, derive from `Microsoft.Xna.Framework.Game` and expose static singletons (`GraphicsDeviceManager`, `SpriteBatch`, `InputManager`, `AudioController`). Use `Scene` switching logic like the existing `ChangeScene`/`TransitionScene` so swapping scenes disposes the old one, forces GC, and calls `Initialize` on the new one.
   - Add `Diagnostics` support (see `TerrainGeneration2D.Core/Diagnostics/TerrainPerformanceEventSource.cs` and `Diagnostics/ConsoleEventListener`) so `EnablePerformanceDiagnostics` toggles console logging and `dotnet-trace` friendly output.

4. **Create the entry point game**
   - In `TerrainGeneration2D/TerrainGenerationGame.cs`, derive from `Core`. Set the window title/size, enable diagnostics in DEBUG, load the theme song via the shared `Content` manager, and initialize Gum (`GumService.Default.Initialize(...)`) before doing `ChangeScene(new GameScene())`.
   - Register Gum input (keyboard/gamepad) and content loader as shown in the current file, and set the canvas size to the graphics device.

5. **Implement camera/input helpers**
   - Build `GameController` to wrap `Core.Input` (keyboard, mouse, gamepad) into methods like `GetCameraMovement()`, `GetZoomDelta()`, `IsCameraPanActive()`, `ToggleFullscreen()`, and `ToggleDebugOverlay()` so scenes never touch `InputManager` directly.
   - In your scene update loop, call those helpers rather than reading raw input.

6. **Add chunked tilemap infrastructure**
   - Create `ChunkedTilemap` under `TerrainGeneration2D.Core/Graphics`: manage 64×64 chunks, buffer a 3×3 area around `Camera2D.ViewportWorldBounds`, and persist each chunk in `Content/saves/map_{chunkX}_{chunkY}.dat` (gzip, magic header `CHNK`).
   - Implement deterministic generation using `masterSeed + chunkX * 73856093 + chunkY * 19349663`.
   - Integrate `TileTypeRegistry`, `WaveFunctionCollapse`, and `HeightMapGenerator` (see files under `TerrainGeneration2D.Core/Mapping`). When WFC succeeds, copy its output; otherwise, fallback to `GenerateRandomChunk` that uses `HeightMapGenerator` + `TerrainRuleConfiguration`.
   - Emit `TerrainPerformanceEventSource.Log.ChunkLoadBegin/End`, `ChunkSaveBegin/End`, `UpdateActiveChunksBegin/End`, and `ReportActiveChunkCount` so diagnostics can monitor chunk activations.

7. **Build terrain generation helpers**
   - Port `WaveFunctionCollapse` (tile entropy tracking, neighbor constraint propagation, `TileRuleContext`, weighted tile choice) from `TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse.cs`. Include `TileTypeRegistry`, `MappingInformationService`, and the predefined tile types (`TerrainGeneration2D.Core/Mapping/TileTypes/TileTypes.cs`) to enforce adjacency rules.
   - Provide `TerrainRuleConfiguration` and `HeightMapConfiguration` classes to configure thresholds (ocean/beach/plains/mountain/snow ranges) and noise scales/weights. The `HeightMapGenerator` should sample OpenSimplex/Perlin/Value noise to compute altitude + noise metrics.
   - Use `ChunkedTilemap.PickTileByHeight` to translate a `HeightSample` into tile IDs when WFC fails.

8. **Create the main GameScene**
   - In `TerrainGeneration2D/Scenes/GameScene.cs`, load `images/terrain-atlas`, build the `Tileset`, and instantiate the `ChunkedTilemap` with the desired map size (2048) and the save folder path `Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "saves")`.
   - Create `Camera2D` centered on the map, manage mouse/keyboard zoom and pan via `GameController`, and call `_chunkedTilemap.UpdateActiveChunks(_camera.ViewportWorldBounds)` every frame. Handle F11/F12 toggles for fullscreen and debug overlay.
   - Draw the tilemap with `_camera.GetTransformMatrix()`, overlay debug rectangles for dirty chunk borders and viewport bounds (see helper methods), and draw Gum UI (via `GumService.Default.Draw()`).
   - In `UnloadContent`, call `_chunkedTilemap?.SaveAll()` to persist chunks before scene change.

9. **Add Gum UI controls and tooltips**
   - Build `GameSceneUI` in `TerrainGeneration2D/UI/GameSceneUI.cs` to add score text, pause/game-over panels, and buttons using `AnimatedButton`. Load Gum textures/sounds (`images/atlas-definition.xml`, `audio/ui`) via the shared content manager and reuse `_uiSoundEffect` for clicks.
   - Create `TooltipManager` that converts mouse screen position to world tiles (`Camera2D.ScreenToWorld`), caches the last tile, and displays `Tile:[x,y] Type:id Chunk:[cx,cy]` in a Gum panel added to `GumService.Default.Root`. Update this panel each frame.
   - Ensure `GameScene.Initialize` clears `GumService.Default.Root.Children` before instantiating these helpers so new nodes aren’t reused across scenes.

10. **Hook up diagnostics and counters**

- Use `dotnet-counters monitor --process-id <pid>` with the custom counters `JohnLudlow.TerrainGeneration2D.Performance-active-chunk-count` and `...-chunks-saved-per-second` (see docs/performance-and-debugging.md) to watch chunk load/save behavior.
- Capture `TerrainPerformanceEventSource` events with `dotnet-trace collect --process-id <pid> --providers "JohnLudlow.TerrainGeneration2D.Performance::Informational"` and view them in PerfView or Speedscope.
- Refer to `EVENT_ID_CONFLICT_FIX.md` for the necessary event ID ranges (start at 10) to avoid conflicts.

11. **Finalize assets & saves**

- Add textures (e.g., `images/terrain-atlas.png`), Gum definitions, fonts, and audio to `TerrainGeneration2D.Content/Assets`. Update the Gum builder in `TerrainGeneration2D.Content/Builder/TerrainGeneration2DContentBuilder.cs` so `dotnet build` copies them into the runtime Content folder.
- When running the game, the runtime will store gzipped chunk files under `Content/saves`. Deleting that folder triggers chunk regeneration and exercises both `LoadChunk` and `GenerateChunk` paths.

12. **Add tests and benchmarks**

- Write unit tests for `Camera2D`, `Chunk`, and chunk persistence (`ChunkedTilemap.SaveAll`, `LoadChunk`), modeling them after `TerrainGeneration2D.Tests/ChunkedTilemapTests.cs`.
- Add benchmark logic (see `TerrainGeneration2D.Benchmarks/Program.cs`) to simulate scrolling across multiple chunks and saving them so you can measure generation latency.

By following these steps—create the solution, implement the core/scene helpers, port the chunk and WFC machinery, wire Gum UI, and add diagnostics—you end up with the same TerrainGeneration2D sample currently in this repo. Let me know if you’d like a checklist version or a repository template to bootstrap faster.
