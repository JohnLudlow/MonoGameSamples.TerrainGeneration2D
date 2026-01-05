# MonoGame Samples - Copilot Instructions

## Overview
- This collection rebuilds Dungeon Slime as TerrainGeneration2D with a deterministic 2048×2048 map, Gum-powered UI, and a full-screen camera that centers on the world before user input.
- Three projects split responsibility: TerrainGeneration2D (game entry/UI wiring), TerrainGeneration2D.Core (graphics/input/audio services plus scene management), and TerrainGeneration2D.Content (custom builder that copies textures/fonts/audio into `bin/Debug/net10.0/Content`).

## Architecture & services
- `TerrainGeneration2D/GameController.cs` maps `Core.Input` (keyboard, mouse, gamepad) into the game-level actions that scenes consume: camera motion, zoom, fullscreen toggle, right-click pan detection, and mouse coordinates for tooltips.
- `TerrainGeneration2D.Core/Core.cs` exposes the singleton `GraphicsDeviceManager`, `SpriteBatch`, `InputManager`, and `AudioController`, controls scene transitions, and forces `GC.Collect()` after every swap; scenes should `Dispose()` their children, `UnloadContent()`, and then rely on `Initialize()` to recreate their UI and assets.
- `TerrainGeneration2D/TerrainGenerationGame.cs` derives from that core, wires in Gum, audio, and input, and always begins by pushing `TerrainGeneration2D.Scenes.GameScene` so the running game has a single scene focus.

## Performance Diagnostics
- `TerrainGeneration2D.Core/Diagnostics/TerrainPerformanceEventSource.cs` provides .NET EventSource instrumentation for chunk operations and WFC generation
- Events are only emitted when an `EventListener` is attached - by default they have zero overhead
- `ConsoleEventListener` automatically enabled in DEBUG builds writes events to console
- See `TerrainGeneration2D.Core/Diagnostics/README.md` for production monitoring with dotnet-counters, dotnet-trace, or PerfView
- Event source already instruments `ChunkedTilemap` (load/save/update) and `WaveFunctionCollapse` (generation tracking)

## Scene & UI patterns
- Scenes own their own `ContentManager` (see `TerrainGeneration2D.Core/Scenes/Scene.cs`), so avoid sharing content across scenes unless you have a static cache and explicitly dispose it when the scene unloads.
- `TerrainGeneration2D/Scenes/GameScene.cs` loads `images/terrain-atlas` (assumed to be 20×20 tiles in `Content`), builds the `ChunkedTilemap` with master seed `12345`, centers `Camera2D` on the map middle, registers `TooltipManager`, and wires `GameSceneUI` into Gum.
- `GameSceneUI` keeps the Gum tree rooted at `GumService.Default.Root`, owns score text, pause/game-over panels, and routes button clicks through `AnimatedButton` for consistent feedback, assuming all Gum assets live under the Content folder already built by the content builder.
- `TooltipManager` converts screen positions via `Camera2D.ScreenToWorld`, caches the last tile so it only updates when coordinates change, and shows a semi-transparent panel offset by +10,+10 pixels whenever the cursor is above a valid tile.

## Chunked map & camera
- `TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs` manages 64×64 chunks, keeps a 3×3 buffer around the camera by expanding the visible chunk range before and after drawing, and persists each chunk to `Content/saves/map_{chunkX}_{chunkY}.dat` with a gzip header that starts with `CHNK` and version `1`.
- Chunk generation is deterministic: seed = `masterSeed + chunkX * 73856093 + chunkY * 19349663`. The tile data is either the output of Wave Function Collapse (using `TerrainGeneration2D.Core/Mapping/TileTypes/TileTypes.cs`) or a random fallback, and each generated chunk flips `IsDirty` so it gets saved on unload.
- `ChunkedTilemap.SaveAll()` runs inside `GameScene.UnloadContent()` (and during `UpdateActiveChunks` when chunks drift out of the 3×3 buffer) to flush dirty chunks. Deleting `Content/saves` forces regeneration and ensures both `LoadChunk` and `GenerateChunk` paths are exercised.
- `Camera2D` clamps zoom between 0.25 and 4.0 (increments of 0.1), exposes `ViewportWorldBounds` for chunk culling, and supplies `ScreenToWorld`/`WorldToScreen` helpers that drive tooltip placement, chunk loading, and view transforms.

## Input & UI wiring
- `GameController.GetCameraMovement()`, `GetZoomDelta()`, and `IsCameraPanActive()` are the single input sources for camera control; the scene simply reads these once per `Update` and does not touch raw `InputManager` state directly.
- Mouse-based pans move the camera by taking the difference between the last drag position and the current mouse position, dividing by the current zoom, and applying that translation to `Camera2D`.
- Gum UI is composable: every UI element must be added under `GumService.Default.Root` (the root is cleared when `GameScene.Initialize()` runs), and buttons should route through `AnimatedButton` so hover, press, and click behaviors stay consistent.

## Content pipeline & assets
- `TerrainGeneration2D.Content/Builder/TerrainGeneration2DContentBuilder.cs` enumerates art, fonts, and audio assets, builds Gum atlases, and copies everything into `TerrainGeneration2D.Content/bin/Debug/net10.0/Content/`. The builder runs automatically as part of `dotnet build TerrainGeneration2D.slnx` (no MGCB editor required).
- Game assets are referenced by relative names such as `images/terrain-atlas` or `audio/*.wav`; updating any image/audio/Gum definition requires rerunning the build command so the content folder contains the latest data.
- Chunk saves land in the runtime `Content/saves` directory by default; inspect those gzipped `.dat` files when you need to debug serialization issues.

## Workflows
- Run `dotnet build TerrainGeneration2D.slnx` to compile every project, refresh the content folder, and regenerate Gum assets.
- Use `dotnet run --project TerrainGeneration2D/TerrainGeneration2D.csproj` to launch the fullscreen map with camera, Gum UI, chunked tilemap, and tooltip overlays.
- After changing art, audio, or Gum definitions, rerun the build command above—there is no file watcher, so the builder only runs on explicit builds.
- Performance events automatically appear in console during DEBUG builds; use `dotnet-counters` or `dotnet-trace` for production monitoring.

## Testing
- `dotnet test TerrainGeneration2D.Tests/TerrainGeneration2D.Tests.csproj` exercises `Camera2D`, `Chunk`, `ChunkedTilemap` persistence, serialization, mapping rules, and EventSource diagnostics. Follow the patterns in `TerrainGeneration2D.Tests/TestHelpers.cs` and `UNIT_TEST_RECOMMENDATIONS.md` when adding new tests.
- Benchmarks live under `TerrainGeneration2D.Benchmarks/`, but CI gates rely solely on the test project.

## Tips for AI engineers
- Scene transitions invoke `Core.ChangeScene`, which disposes the old scene, runs `GC.Collect()`, and then initializes the next scene. Always assume `Initialize()` re-runs `LoadContent()` and remember to dispose any Gum nodes you add manually.
- Chunk data is deterministic but cached under `Content/saves`; deleting those files forces regeneration and lets you verify both `LoadChunk` and `GenerateChunk` as you change tile rules.
- Gum widgets should only be added to `GumService.Default.Root`; `GameSceneUI` clears the root each time the scene initializes to avoid stale controls, so add GIFs or panels there instead of storing them globally.
- Tooltip text mirrors the string format `Tile:[x,y] Type:id Chunk:[cx,cy]`; reuse that format when emitting debug logs to make it easier to cross-reference UI feedback.
- EventSource diagnostics require an active listener to emit - `ConsoleEventListener` is auto-enabled in DEBUG builds, but production requires dotnet-counters/dotnet-trace.

Please flag any missing context or unclear sections so we can iterate on this guidance.
