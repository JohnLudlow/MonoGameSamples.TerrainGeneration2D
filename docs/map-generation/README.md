# Map Generation

This wiki covers the terrain generation system: chunked maps, persistence, and algorithms used to synthesize tiles at scale. It complements architecture and tutorial docs with a code-anchored reference focused on the runtime pipeline.

Scope:
- Chunk lifecycle, culling, and drawing
- Save/load of chunk data
- Algorithm modules (e.g., WFC) and their integration

Key code:
- [TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs](../../TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs)
- [TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/WfcProvider.cs](../../TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/WfcProvider.cs)
- [TerrainGeneration2D.Core/Diagnostics/TerrainPerformanceEventSource.cs](../../TerrainGeneration2D.Core/Diagnostics/TerrainPerformanceEventSource.cs)

Sub-wikis:
- Wave Function Collapse: [docs/map-generation/wfc/README.md](wfc/README.md)

See also:
- Architecture: [docs/architecture-class-diagram.md](../architecture-class-diagram.md)
- Performance guide: [docs/performance-and-debugging.md](../performance-and-debugging.md)
