# Map Generation

> **Onboarding:** New to map generation? Start with the [Mapping Onboarding Overview](../feature-overview.md) for a guided introduction to the mapping subsystem, key concepts, and how to get productive quickly.

This wiki covers the terrain generation system: chunked maps, persistence, and algorithms used to synthesize tiles at scale. It complements architecture and tutorial docs with a code-anchored reference focused on the runtime pipeline.

## Navigation

- **Quick Start:**
  - [Mapping Onboarding Overview](../feature-overview.md)
  - [Chunked Tilemap](../chunked-tilemap.md)
  - [Wave Function Collapse (WFC)](wfc/README.md)
  - [Full Terrain Generation Tutorial](../../../terrain2d-tutorial/README.md)
- **Advanced Topics:**
  - [WFC Heuristics and Tie-Breaking](wfc/05-heuristics.md)
  - [Chunk Save/Load Format](../chunked-tilemap.md#save-format)
  - [Performance Diagnostics](../../../performance-and-debugging.md)
- **Architecture & Index:**
  - [Architecture Class Diagram](../../../architecture-class-diagram.md)
  - [Mapping Area Index](../README.md)
  - [Features Hub](../../README.md)

## Overview

- Chunk lifecycle, culling, and drawing
- Save/load of chunk data
- Algorithm modules (e.g., WFC) and their integration

Key references:

- Chunked Tilemap: [../chunked-tilemap.md](../chunked-tilemap.md)
- WFC overview: [wfc/README.md](wfc/README.md)
- Diagnostics overview: [../../../TerrainGeneration2D.Core/Diagnostics/README.md](../../../../TerrainGeneration2D.Core/Diagnostics/README.md)

## Glossary

| Term | Meaning |
| ---- | ------- |
| Chunk | 64×64 tile region, loaded/unloaded as a unit |
| Tile | Smallest map unit, e.g. grass, water, mountain |
| WFC | Wave Function Collapse, constraint-based generation |
| Heightmap | 2D noise-based terrain elevation map |
| Heuristics | Rules for cell selection and tie-breaking in WFC |
| Entropy | Measure of uncertainty in tile selection |
| TileTypeRegistry | Registry of tile types and rules |

## Changelog & Validation

- 2024-06-01: Initial doc
- 2026-01-15: Added onboarding pointer, navigation, glossary, advanced topics, changelog/validation

## Contributing & Validation

- When updating map generation docs:
  - Update navigation and add links to new docs.
  - Add new terms to the glossary below.
  - Validate links with `scripts/check-doc-links.ps1`.
  - Ensure onboarding and advanced topics are discoverable.

---

...existing code...
