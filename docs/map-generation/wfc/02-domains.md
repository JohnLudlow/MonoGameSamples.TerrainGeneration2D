# Domains

Purpose: describe how domains and outputs are represented and initialized.

## Representation
Each cell has a domain of possible tile IDs. We use a 2D array of `HashSet<int>?` where `null` means collapsed, and a separate `int[,]` to store the chosen tile IDs.

```csharp
/// <summary>
/// For each cell (x,y), a null value means the domain is collapsed.
/// Otherwise, the set contains all currently valid tile IDs.
/// </summary>
private readonly HashSet<int>?[,] _possibilities;

/// <summary>
/// Output grid of chosen tile IDs. A value of -1 indicates unset.
/// </summary>
private readonly int[,] _output;
```

## Initialization
On construction, we seed every domain with all valid tile IDs and mark outputs as unset.

```csharp
/// <summary>
/// Initialize domains from TileTypeRegistry.ValidTileIds and clear outputs.
/// </summary>
private void InitializeDomains(TileTypeRegistry tileRegistry, int width, int height)
{
	var all = tileRegistry.ValidTileIds;
	for (int y = 0; y < height; y++)
	for (int x = 0; x < width; x++)
	{
		_possibilities[x, y] = new HashSet<int>(all);
		_output[x, y] = -1;
	}
}
```

## Context for Rule Evaluation
Rule checks compute whether a candidate tile is compatible with a neighbor, given:
- Tile rule definitions (`TileTypeRegistry` and concrete tile types)
- Biome/height context (`IHeightProvider` samples)
- Configuration (`TerrainRuleConfiguration`)
- Read-only access to placed outputs (`MappingInformationService`)

These inputs are packaged into a `TileRuleContext` passed to rule evaluators during propagation.

Code references:
- Data + setup: [TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse.cs](../../../TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse.cs)
- Rule evaluation: [TerrainGeneration2D.Core/Mapping/TileAdjacencyRules.cs](../../../TerrainGeneration2D.Core/Mapping/TileAdjacencyRules.cs)

Navigation
- Up: [WFC README](README.md)
- Previous: [01 — Overview](01-overview.md)
- Next: [03 — Propagation](03-propagation.md)
