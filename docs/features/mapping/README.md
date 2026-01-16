# Mapping Features

> **Onboarding:** New to mapping? Start with the [Onboarding Overview](feature-overview.md) for a guided introduction to the mapping subsystem, key concepts, and how to get productive quickly.

This directory contains documentation for the mapping subsystem, including chunked tilemap, WFC, heightmaps, and related algorithms.

## Navigation

- **Quick Start:**
  - [Onboarding Overview](feature-overview.md)
  - [Full Terrain Generation Tutorial](../../terrain2d-tutorial/README.md)
- **Key Components:**
  - [Chunked Tilemap](chunked-tilemap.md): Large map, chunked for performance and streaming
  - [Wave Function Collapse (WFC)](map-generation/wfc/README.md): Constraint-based procedural generation
  - [Heightmap](map-generation/README.md): Noise-based terrain shaping
  - [TileTypeRegistry](map-generation/README.md): Tile rules and types
- **Related UI Features:**
  - [Tooltip Manager](../ui/tooltip-manager.md)
  - [Runtime Settings Panel](../ui/runtime-settings-panel.md)
  - [Debug Overlay](../ui/debug-overlay.md)
- **Architecture & Performance:**
  - [Architecture Class Diagram](../../architecture-class-diagram.md)
  - [Performance & Debugging](../../performance-and-debugging.md)

## Glossary

| Term | Meaning |
| ---- | ------- |
| Chunk | 64×64 tile region, loaded/unloaded as a unit |
| Tile | Smallest map unit, e.g. grass, water, mountain |
| WFC | Wave Function Collapse, constraint-based generation |
| Heightmap | 2D noise-based terrain elevation map |
| Heuristics | Rules for cell selection and tie-breaking in WFC |
| TileTypeRegistry | Registry of tile types and rules |

## Advanced Topics

- [WFC Heuristics and Tie-Breaking](map-generation/wfc/05-heuristics.md)
- [Chunk Save/Load Format](chunked-tilemap.md#save-format)
- [Performance Diagnostics](../../performance-and-debugging.md)

## Changelog & Validation

- 2024-06-01: Initial onboarding doc added
- 2024-06-02: Linked WFC and heightmap docs
- 2024-06-03: Added glossary and contributing notes
- 2026-01-15: Improved onboarding pointer, navigation, glossary, and added advanced topics/validation

## Contributing & Validation

- When updating mapping features:
  - Update this index and add links to new docs.
  - Add new terms to the glossary below.
  - Validate links with `scripts/check-doc-links.ps1`.
  - Ensure onboarding and advanced topics are discoverable.
