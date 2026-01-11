# Mapping Functional Area

## Overview

- Terrain generation, chunk management, and rule-based systems, organized for clarity and extensibility.
- Clear separation between generation algorithms (WFC, heightmap) and rendering/persistence (chunked tilemap).
- Deterministic per-chunk seeds and runtime-configurable heuristics enable iterative tuning.

## Components

- [Chunked Tilemap](chunked-tilemap.md): Chunked rendering, activation buffer, save/load.
- Map Generation Wiki: [features/mapping/map-generation/README.md](map-generation/README.md)
  - Child Component — WFC: [features/mapping/map-generation/wfc/README.md](map-generation/wfc/README.md)
- Tile Configuration: [docs/tile-configuration.md](../../tile-configuration.md)
- Architecture Diagram: [docs/architecture-class-diagram.md](../../architecture-class-diagram.md)

## Intent

- Efficient streaming and persistence for large maps.
- Deterministic generation per chunk; configurable heuristics and heightmaps.
- Clear separation of algorithms (WFC, heightmap) from rendering/storage.

## References

- Tilemap: [TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs](../../../TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs)
- Camera: [TerrainGeneration2D.Core/Graphics/Camera2D.cs](../../../TerrainGeneration2D.Core/Graphics/Camera2D.cs)
- Scene integration: [TerrainGeneration2D/Scenes/GameScene.cs](../../../TerrainGeneration2D/Scenes/GameScene.cs)
- Configuration: [TerrainGeneration2D/appsettings.json](../../../TerrainGeneration2D/appsettings.json)

## Navigation

- Up: [Features Hub](../README.md)
