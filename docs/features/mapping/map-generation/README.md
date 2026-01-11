# Map Generation

This wiki covers the terrain generation system: chunked maps, persistence, and algorithms used to synthesize tiles at scale. It complements architecture and tutorial docs with a code-anchored reference focused on the runtime pipeline.

Scope:

- Chunk lifecycle, culling, and drawing
- Save/load of chunk data
- Algorithm modules (e.g., WFC) and their integration

Key references:

- Chunked Tilemap: [../chunked-tilemap.md](../chunked-tilemap.md)
- WFC overview: [wfc/README.md](wfc/README.md)
- Diagnostics overview: [../../../TerrainGeneration2D.Core/Diagnostics/README.md](../../../../TerrainGeneration2D.Core/Diagnostics/README.md)

Sub-wikis:

- Wave Function Collapse: [wfc/README.md](wfc/README.md)

See also:

- Architecture: [../../../architecture-class-diagram.md](../../../architecture-class-diagram.md)
- Performance guide: [../../../performance-and-debugging.md](../../../performance-and-debugging.md)
- Mapping area index: [../README.md](../README.md)
- Chunked Tilemap component: [../chunked-tilemap.md](../chunked-tilemap.md)

## Navigation

- Up: [Mapping Area](../README.md)
- Up: [Features Hub](../../README.md)
