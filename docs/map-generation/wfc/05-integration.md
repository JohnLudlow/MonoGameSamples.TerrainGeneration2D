# Integration

Purpose: show how WFC results populate chunks, persist, and render.

## Chunk pipeline in practice
Each chunk invokes WFC to produce a 2D array of tile IDs. If WFC succeeds, copy the result into the chunk’s storage; otherwise, fall back to a deterministic random fill (or a cached save).

```csharp
/// <summary>
/// Generate a single chunk: attempt WFC first, copy the result on success,
/// otherwise use the fallback path (e.g., random or previously saved data).
/// </summary>
private void GenerateChunk(Point chunkOrigin, Random random)
{
	// 1) Construct WFC with tile registry, config, height provider, and origin
	// var wfc = new WfcProvider(ChunkSize, ChunkSize, _tileTypeRegistry, random, _terrainRuleConfig, _heightProvider, chunkOrigin);

	// 2) Run with or without backtracking depending on settings
	// bool ok = wfc.Generate(enableBacktracking: true, maxIterations: 10000);

	// 3) On success, copy output into the chunk’s tiles
	// if (ok) { var output = wfc.GetOutput(); CopyToChunk(output, chunkOrigin); }
	// else { FillChunkFallback(random, chunkOrigin); }
}
```

### Seams and boundary constraints
To maintain continuity across chunk borders, seed edge domains based on neighbor edges (or import the neighbor’s placed outputs) before solving. Persist chunks so reloading preserves seams across runs.

### Saves and reproducibility
Chunk data is compressed to disk after generation; deleting the saves folder forces regeneration. Stable seeds ensure deterministic results for the same world settings.

Code references:
- Chunk orchestration: [TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs](../../../TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs)
- WFC output handoff: [TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/WfcProvider.cs](../../../TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/WfcProvider.cs)

Navigation
- Up: [WFC README](README.md)
- Previous: [04 — Backtracking](04-backtracking.md)
- Next: [06 — Performance](06-performance.md)
