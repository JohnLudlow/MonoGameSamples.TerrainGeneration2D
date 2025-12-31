# MonoGame Samples - Copilot Instructions

## Overview
- This workspace reworks the legacy Dungeon Slime tutorial into a TerrainGeneration2D prototype with a 2048×2048 deterministic map, full-screen camera, and Gum-driven UI.
- Three projects share responsibility: TerrainGeneration2D (game), TerrainGeneration2D.Core (framework/services), and TerrainGeneration2D.Content (custom content builder that runs automatically after every build).

## Architecture & Services
- TerrainGeneration2D/TerrainGenerationGame.cs extends TerrainGeneration2D.Core/Core.cs, wires up Gum, audio, input, and kicks off TerrainGeneration2D.Scenes.GameScene. Inspect Core for the singleton GraphicsDevice, SpriteBatch, InputManager, AudioController, and the GC.Collect that runs after each scene swap.
- Scenes derive from TerrainGeneration2D.Core/Scenes/Scene.cs, each owning its own ContentManager; you must Dispose() scenes explicitly and call UnloadContent to release assets inside the derived class.
- Directory.Build.props enforces <Nullable>enable</Nullable>, adds analyzers, and sets the global root namespace (JohnLudlow.MonoGameSamples.TerrainGeneration2D), which every project inherits.

## Gameplay + UI wiring
- GameScene (TerrainGeneration2D/Scenes/GameScene.cs) builds the tileset from images/terrain-atlas (20×20 tiles), constructs ChunkedTilemap with master seed 12345, creates Camera2D centered on the map, registers TooltipManager, and saves all chunks when unloading.
- GameSceneUI (TerrainGeneration2D/UI/GameSceneUI.cs) drives the Gum layout: score text, pause/game-over panels, and buttons that route through AnimatedButton. It assumes audio/ui and images/atlas-definition.xml are already built into Content and uses TextureAtlas.FromFile.
- TooltipManager (TerrainGeneration2D/UI/TooltipManager.cs) keeps a root Panel/TextRuntime, computes tile/chunk info via Camera2D.ScreenToWorld, caches the last coordinates, and only shows the tooltip when the cursor is over a valid tile, offset by +10,+10 screen pixels.
- GameController (TerrainGeneration2D/GameController.cs) translates Core.Input (keyboard, mouse, gamepad) into actions: GetCameraMovement (WASD/arrows/stick), GetZoomDelta (mouse wheel + triggers), ToggleFullscreen (F11), right-click pan detection, and exposes mouse position for tooltip updates.

## Chunked map & camera
- ChunkedTilemap (TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs) manages 64×64 chunks (see TerrainGeneration2D.Core/Graphics/Chunk.cs), deterministic generation (seed = masterSeed + chunkX * 73856093 + chunkY * 19349663), optional WaveFunctionCollapse/TileTypeRegistry constraints, gzip serialization to Content/saves/map_{chunkX}_{chunkY}.dat, and UpdateActiveChunks keeps a 3×3 buffer while unloading and saving distant chunks.
- Camera2D (TerrainGeneration2D.Core/Graphics/Camera2D.cs) stores Position, clamps Zoom between 0.25 and 4.0 with increment 0.1, exposes ViewportWorldBounds for chunk culling, and offers ScreenToWorld/WorldToScreen to drive tooltips and chunk range calculations.
- Tile rules (TerrainGeneration2D.Core/Mapping/TileTypes/TileTypes.cs) define TerrainRuleConfiguration and TileTypeRegistry used by WaveFunctionCollapse; the registry is sized to Tileset.Count so new tile IDs default to GenericTileType.

## Content pipeline
- TerrainGeneration2D.Content/Builder/TerrainGeneration2DContentBuilder.cs enumerates assets (textures, fonts, audio) and runs via the BuildContent target in the main csproj; the builder copies output into bin/Debug/net10.0/Content/ (no MGCB Editor involved).
- When assets change, rerun `dotnet build TerrainGeneration2D.slnx` so the builder refreshes the Content folder; the running game expects TerrainGeneration2D/Scenes/GameScene.cs to load "terrain-atlas" and the Gum atlas files.

## Workflows
- `dotnet build TerrainGeneration2D.slnx` builds everything and automatically triggers the content pipeline.
- `dotnet run --project TerrainGeneration2D/TerrainGeneration2D.csproj` launches the fullscreen map with camera, Gum UI, chunked tilemap, and tooltip overlays.
- ChunkedTilemap.SaveAll is invoked during GameScene.UnloadContent to flush dirty chunks; the saved files persist under Content/saves so repeating navigations hit the same data.

## Testing
- `dotnet test TerrainGeneration2D.Tests/TerrainGeneration2D.Tests.csproj` already covers Camera2D, Chunk behavior, ChunkedTilemap persistence, serialization, and mapping rules. Mirror the examples in TerrainGeneration2D.Tests/UNIT_TEST_RECOMMENDATIONS.md and reuse TestHelpers.cs for shared fixtures.
- Benchmarks for chunk generation live in TerrainGeneration2D.Benchmarks/, but the CI gate relies on the tests above.

Flag any missing details or questions so this doc can stay accurate and useful for the next AI pass.
