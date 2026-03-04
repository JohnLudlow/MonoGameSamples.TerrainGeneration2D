# Mapping System: Developer Onboarding Overview

## Purpose

This document provides a high-level, developer-focused introduction to the mapping subsystem in MonoGameSamples.TerrainGeneration2D. It complements the detailed component and API docs by explaining the big picture, extensibility points, and practical workflow for contributing to or debugging the terrain generation pipeline.

## Table of contents

- [What is the Mapping System?](#what-is-the-mapping-system)
- [Key Concepts](#key-concepts)
- [Extending or Debugging the Mapping System](#extending-or-debugging-the-mapping-system)
- [Key Files & Entry Points](#key-files--entry-points)
- [Best Practices](#best-practices)
- [Further Reading](#further-reading)

## Definition of terms

| Term | Meaning |
| ---- | ------- |
| Chunk | 64×64 tile region loaded/unloaded as a unit |
| WFC | Wave Function Collapse — constraint-based procedural generation |
| HeightMap | Noise-based elevation map used as context for rules |

## What is the Mapping System?

- The mapping system is responsible for generating, storing, and rendering large 2D worlds using chunked tilemaps, procedural algorithms (notably Wave Function Collapse), and runtime-tunable heuristics.
- It is designed for scalability (2048x2048+ tiles), deterministic generation, and efficient streaming/persistence.

## Key Concepts

- **ChunkedTilemap**: Divides the world into 64x64 tile chunks, manages loading/unloading, and persists dirty chunks to disk. See [chunked-tilemap.md](chunked-tilemap.md).
- **Wave Function Collapse (WFC)**: The main procedural algorithm for generating coherent terrain, with support for entropy heuristics, backtracking, and runtime configuration. See [map-generation/wfc/README.md](map-generation/wfc/README.md).
- **HeightMap**: Provides elevation/biome context for rules and tile selection.
- **Heuristics & Runtime Tuning**: All major generation parameters are runtime-configurable via the F10 panel, enabling rapid iteration.

## Extending or Debugging the Mapping System

- **To add a new generation algorithm**: Implement a new provider (see WfcProvider for reference), and integrate it into the chunk generation path in [ChunkedTilemap.cs](../../../TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs).
- **To tune generation**: Use the F10 runtime settings panel to adjust heuristics, weights, and time budgets. Regenerate visible chunks to preview changes.
- **To debug chunking or WFC**: Use the F12 debug overlay to visualize active/dirty chunks and the current viewport. Enable diagnostics (see [performance-and-debugging.md](../../performance-and-debugging.md)) for live counters and event tracing.
- **To persist or clear saves**: Chunks are auto-saved on unload; use the settings panel to clear all saves and force regeneration.

## Key Files & Entry Points

- [ChunkedTilemap.cs](../../../TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs): Main entry for chunk management and generation.
- [GameScene.cs](../../../TerrainGeneration2D/Scenes/GameScene.cs): Scene integration, camera, and UI wiring.
- [appsettings.json](../../../TerrainGeneration2D/appsettings.json): Default config for all generation parameters.
- [WfcProvider.cs](../../../TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/WfcProvider.cs): Core WFC implementation.
- Runtime settings UI: [migration notes](../ui/migration-notes.md) (legacy code-only UI removed)

## Best Practices

- Keep per-frame allocations low in hot paths (Update/Draw, WFC loops).
- Use deterministic seeds for reproducibility.
- Prefer extending via new providers/services rather than modifying core chunk/tilemap logic.
- Document new algorithms or heuristics in /docs/features/mapping.

## Further Reading

- [Chunked Tilemap](chunked-tilemap.md)
- [Map Generation](map-generation/README.md)
- [WFC Details](map-generation/wfc/README.md)
- [Performance & Debugging](../../performance-and-debugging.md)
- [Architecture Diagram](../../architecture-class-diagram.md)
