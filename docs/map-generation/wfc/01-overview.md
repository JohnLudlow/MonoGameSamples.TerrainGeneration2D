# WFC Overview

Purpose: explain tiles, adjacency rules, entropy-based selection, and the high-level solve loop used to synthesize terrain.

## Concepts
- Tiles and adjacency: Allowed neighbors are defined by tile rules; evaluation happens per neighbor pair with contextual inputs (height samples, configuration, map service).
- Entropy: Choose the next cell by the fewest remaining candidates; break ties deterministically or with PRNG for variety.
- Loop: Select → Collapse → Propagate → Repeat until all cells are collapsed or a contradiction is reached.
- Outcomes: Success when every domain is reduced to a single value; failure if a domain becomes empty (or if backtracking limits are exceeded when enabled).

## Solve Loop at a Glance
The snippet below mirrors the production loop but omits implementation details for clarity.

```csharp
/// <summary>
/// High-level WFC loop: pick lowest-entropy cell, collapse, then propagate
/// constraints until stable or a contradiction is found.
/// </summary>
private bool Generate(int maxIterations = 10000)
{
	// 1) Optional: emit diagnostics begin
	// TerrainPerformanceEventSource.Log.WaveFunctionCollapseBegin(cx, cy);

	int iterations = 0;
	while (iterations++ < maxIterations)
	{
		// 2) Select cell with the fewest candidates
		var (x, y) = FindLowestEntropy();
		if (x == -1) return true; // all collapsed

		// 3) Collapse the chosen cell to a single tile (heuristic + PRNG)
		if (!CollapseCell(x, y)) return false; // contradiction

		// 4) Propagate constraints to neighbors using adjacency rules
		if (!Propagate(x, y)) return false; // contradiction
	}

	// 5) Optional: emit diagnostics end
	// TerrainPerformanceEventSource.Log.WaveFunctionCollapseEnd(cx, cy, success:false);
	return false; // guard: max iterations exceeded
}
```

Code references:
- Algorithm: [TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/WfcProvider.cs](../../../TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/WfcProvider.cs)
- Integration: [TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs](../../../TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs)

Navigation
- Up: [WFC README](README.md)
- Next: [02 — Domains](02-domains.md)
