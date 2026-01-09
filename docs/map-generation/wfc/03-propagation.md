# Propagation

Purpose: detail candidate selection, neighbor pruning, and BFS-style propagation until stability or contradiction.

## Selection and Collapse
Pick the next cell by minimum entropy (fewest candidates). Collapse by selecting one tile, optionally biased by simple heuristics (e.g., favor tiles that match placed neighbors).

```csharp
/// <summary>
/// Chooses a tile for (x,y) and collapses its domain to a single value.
/// Returns false if the domain is empty (contradiction).
/// </summary>
private bool CollapseCell(int x, int y)
{
	// Compute weighted options, roll once, set _output[x,y], and mark domain as collapsed (null)
	// Return false if no candidates were available
	return true;
}
```

## BFS Propagation
After collapsing a cell, propagate constraints to its neighbors by intersecting their domains with the set of tiles allowed next to the chosen tile.

```csharp
/// <summary>
/// Propagate constraints outward from (startX,startY) until no domains change
/// or a contradiction is detected. Returns false if any domain becomes empty.
/// </summary>
private bool Propagate(int startX, int startY)
{
	var q = new Queue<(int x,int y)>();
	q.Enqueue((startX, startY));
	while (q.Count > 0)
	{
		var (x,y) = q.Dequeue();
		if (_output[x,y] == -1) continue; // not collapsed yet
		// For each neighbor, intersect its domain; enqueue neighbors whose domain changed
		// If a neighbor’s domain becomes empty, return false
	}
	return true;
}
```

### Applying neighbor constraints
The constraint for a neighbor combines tile-rule permissions and contextual inputs. The method below (signature only) documents intent and parameters.

```csharp
/// <summary>
/// Intersect the neighbor’s domain at (x,y) with the set of tiles allowed
/// next to <paramref name="neighborTileId"/> in the given direction.
/// Returns false if the resulting domain is empty.
/// </summary>
/// <param name="x">Neighbor cell x-coordinate.</param>
/// <param name="y">Neighbor cell y-coordinate.</param>
/// <param name="dirToNeighbor">Direction from the current cell to the neighbor.</param>
/// <param name="neighborTileId">Tile placed in the current cell.</param>
/// <param name="neighborPosition">Current cell’s position as a TilePoint.</param>
private bool ConstrainNeighbor(int x, int y, Direction dirToNeighbor, int neighborTileId, TilePoint neighborPosition)
{
	// Build TileRuleContext with height samples and config
	// Intersect the neighbor’s domain; collapse if only one remains
	// Return false if intersection is empty
	return true;
}
```

Code references:
- Selection + loop: [TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/WfcProvider.cs](../../../TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/WfcProvider.cs)
- Neighbor constraints: [TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/WfcProvider.cs](../../../TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/WfcProvider.cs)

Navigation
- Up: [WFC README](README.md)
- Previous: [02 — Domains](02-domains.md)
- Next: [04 — Backtracking](04-backtracking.md)
