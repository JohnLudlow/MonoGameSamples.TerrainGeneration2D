# MonoGame TerrainGeneration2D — AI Agent Guide

Use this guide to be productive immediately in this repo. It captures the architecture, workflows, and project-specific conventions AI agents should follow.

## Big Picture
- Single fullscreen game launches via [TerrainGeneration2D/TerrainGenerationGame.cs](TerrainGeneration2D/TerrainGenerationGame.cs) and immediately pushes `GameScene`; only one scene runs at a time.
- Core lifecycle lives in [TerrainGeneration2D.Core/Core.cs](TerrainGeneration2D.Core/Core.cs): wires graphics/content/input/audio and enforces `ChangeScene` disposal + `GC.Collect` before initializing the next scene.
- Key components and relationships: `Camera2D`, `ChunkedTilemap`, WFC, and `TileTypeRegistry`. See the diagram in [docs/architecture-class-diagram.md](docs/architecture-class-diagram.md).

## Map + Saves
- Deterministic 2048x2048 terrain, chunked into 64x64 tiles. Buffer updates a 3x3 area based on `Camera2D.ViewportWorldBounds`.
- Chunks persist as gzipped data under [TerrainGeneration2D.Core/Content/saves](TerrainGeneration2D.Core/Content/saves) using `map_{chunkX}_{chunkY}.dat`.
- Delete the saves folder to force regeneration and exercise load vs WFC paths. `GameScene.UnloadContent` invokes `ChunkedTilemap.SaveAll`.

## Input + Camera
- Centralized input via [TerrainGeneration2D/GameController.cs](TerrainGeneration2D/GameController.cs). Scenes should only call:
	- `GetCameraMovement()`, `GetZoomDelta()`, `IsCameraPanActive()`, `GetMousePosition()`
	- Toggles: F11 fullscreen, F12 debug overlay
- Movement: WASD/arrows + left-thumbstick; zoom: mouse wheel + shoulder triggers; right-click drag pans (scaled by current `Camera2D.Zoom`).

## UI Flow
- UI uses Gum: `GameScene.Initialize()` clears and repopulates `GumService.Default.Root`; re-add all nodes each scene.
- See [TerrainGeneration2D/UI/GameSceneUI.cs](TerrainGeneration2D/UI/GameSceneUI.cs) and [TerrainGeneration2D/UI/TooltipManager.cs](TerrainGeneration2D/UI/TooltipManager.cs).
- Tooltip format: `Tile:[x,y] Type:id Chunk:[cx,cy]` and updates only when tile coordinates change.

## Debug Overlay
- F12 paints dirty chunk borders and viewport bounds using `ChunkedTilemap.ActiveChunkInfo` snapshots; helps validate culling and buffer logic.

## Content Pipeline + Assets
- Build refreshes Content and runs the builder in [TerrainGeneration2D.Content/Builder](TerrainGeneration2D.Content/Builder).
- Load assets by relative names: `images/terrain-atlas`, `audio/theme`, `audio/ui`, `fonts/04b_30.fnt`.
- Always re-run a full build after editing assets.

## Diagnostics
- `EnablePerformanceDiagnostics` is on in DEBUG (see `TerrainGenerationGame`).
- Use `ConsoleEventListener` or external tools (dotnet-counters, dotnet-trace, PerfView) per [TerrainGeneration2D.Core/Diagnostics/README.md](TerrainGeneration2D.Core/Diagnostics/README.md).
- Keep custom Event IDs ≥ 10; existing `TerrainPerformanceEventSource` uses 10–17 to avoid conflicts.

## Developer Workflows
- Build: `dotnet build TerrainGeneration2D.slnx`
- Run: `dotnet run --project TerrainGeneration2D/TerrainGeneration2D.csproj`
- Test: `dotnet test TerrainGeneration2D.Tests/TerrainGeneration2D.Tests.csproj`

## Tutorial Content
- Purpose: The tutorial documents are designed so someone can implement the same functionality independently, without further reference to this repository. Treat them as standalone, step-by-step guides.

## Coding Standards
- Keep coding standards consistent across tutorial snippets and code.
- Respect local formatting and analyzer settings in [.editorconfig](../.editorconfig) and any repo-specific conventions.
- Prefer `var` for local variable declarations where the type is evident from the right-hand side.
- MonoGame lifecycle methods should appear in pipeline order where present: `Initialize`, `LoadContent`, `Update`, `Draw`, then unload/cleanup.
- Class member order: private fields, properties, constructors, public methods, private methods.
- Field naming: class fields (const or otherwise) use `_camelCase` with a leading underscore.
- Naming: classes and public members use `PascalCase`.
- Never use public settable member fields.
- Prefer get-only properties where possible; use settable properties only when required.
- Where possible, include meaningful XML documentation comments for public APIs and non-trivial members.

## Code Snippets
- Always lead with a short explanation plus the project and filename for context.
- When adding to the middle of a long class/method, note that earlier code is omitted for brevity so readers understand it’s a partial listing.

## Project Conventions
- Scenes must not touch `InputManager` directly; use `GameController` helpers.
- Use `Camera2D.ScreenToWorld` + `ChunkedTilemap.TileToChunkCoordinates` for tooltips and tile picking; cache coordinates to avoid flicker.
- `GameScene` loads `images/terrain-atlas`, creates `ChunkedTilemap` with master seed 12345, centers the camera, registers tooltips, manages debug overlay.

## Examples (do this)
- Pan/zoom handling: read movement/zoom from `GameController` and apply to `Camera2D`; scale pan by `Camera2D.Zoom`.
- Chunk persistence: call `ChunkedTilemap.SaveAll` when chunks leave the buffer or during scene unload.
- Overlay validation: toggle F12 and inspect borders + bounds to confirm culling.

For deeper learning and staged code snapshots, see the tutorial index at [docs/terrain2d-tutorial/README.md](docs/terrain2d-tutorial/README.md).
