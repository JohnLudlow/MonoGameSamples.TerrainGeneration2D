# TileTypeRegistry

Purpose

- Central registry of tile types and their rules used by WFC, fallback generation, and UI tooling.
- Provides adjacency tables, lookup by id or name, and helpers used by generation code to validate neighbor relationships.

Key behaviours

- Created via TileTypeRegistry.CreateDefault(tileCount, config) in code; the registry is populated from TileTypeRuleConfiguration / TerrainRules at startup.
- Exposes methods to query adjacency and per-type rule objects (example: GetRuleForType(typeId) used by ChunkedTilemap).
- Internally represents adjacency as bitmasks or lists per direction for fast propagation in WFC.

Why it matters

- Consistency: WFC, fallback generators, and any editor tooling must use the same registry to avoid contradictions at chunk borders.
- Performance: adjacency checks are hot-path operations during propagation; the registry is optimized for quick membership tests.

Practical usage (code sketch)

```csharp
// Build default registry from configuration and tileset size
var registry = TileTypeRegistry.CreateDefault(tileset.Count, terrainRuleConfig);

// Query a rule by type id
var oceanRule = registry.GetRuleForType(TerrainTileIds.Ocean);

// Check if tile B is allowed to be a neighbor of tile A to the North
bool allowed = registry.IsNeighborAllowed(tileAId, Direction.North, tileBId);
```

Extending

- To add a new tile type:
  1) Add the tile art to the tileset and a constant ID in TerrainTileIds.
  2) Add a rule entry in TerrainRules (see TerrainRules docs).
  3) Update any adjacency rules if necessary to avoid introducing unavoidable contradictions.
  4) Run the game and use RegenerateChunksInView to validate.

Testing

- Unit tests should assert adjacency symmetry where appropriate and ensure GetRuleForType returns expected thresholds for configured tiles.
- When changing adjacency rules add regression tests that generate small grids with WFC to detect contradictions early.

References

- Implementation anchor: TerrainGeneration2D.Core/Mapping/TileTypes/TileTypeRegistry.cs
- Related docs: terrain-rules.md, wfc/README.md
