# Tile Configuration Guide

## How rules work
- `TerrainRuleConfiguration` fields are consumed by `TerrainGeneration2D.Core/Mapping/TileTypes/BeachTile.cs` and `MountainTile.cs`; other tile types only check adjacency via the `MatchesNeighbor` helper in `TerrainGeneration2D.Core/Mapping/TileTypes/TileTypes.cs`.
- Every tile’s `EvaluateRules(TileRuleContext)` can access `TileRuleContext.GetCandidateGroupMetrics` or `GetNeighborGroupMetrics` (which call `TerrainGeneration2D.Core/Mapping/MappingInformationService.cs`) to inspect contiguous regions before accepting a candidate.

## Applying a configuration
- Pass a customized `TerrainRuleConfiguration` when you construct `TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs` in `GameScene`:

```csharp
var config = new TerrainRuleConfiguration
{
    MountainRangeMin = 32,
    MountainRangeMax = 512,
    BeachOceanSizeMin = 200,
    BeachOceanSizeMax = 1200,
};
_chunkedTilemap = new ChunkedTilemap(tileset, MapSizeInTiles, MasterSeed, saveDir, terrainRuleConfiguration: config);
```

Every change to `config` immediately alters the Wave Function Collapse constraints that produce your map.

## Heightmap configuration
- FastNoiseLite drives the heightmap implementation in `TerrainGeneration2D.Core/Mapping/HeightMap/HeightMapGenerator.cs`. Pass a `HeightMapConfiguration` alongside `TerrainRuleConfiguration` when constructing `ChunkedTilemap` to tune continent scale, mountain frequency, and contribution weights:

```csharp
var heightConfig = new HeightMapConfiguration
{
   ContinentScale = 0.005f,
   MountainScale = 0.018f,
   DetailScale = 0.085f,
   ContinentWeight = 0.7f,
   MountainWeight = 0.4f,
   DetailWeight = 0.2f
};
_chunkedTilemap = new ChunkedTilemap(tileset, MapSizeInTiles, MasterSeed, saveDir,
   terrainRuleConfiguration: config,
   heightMapConfiguration: heightConfig);
```
- Each tile now receives a `HeightSample` (altitude + noise derivatives). Rule checks include `TileRuleContext.CandidateHeight.Altitude` so oceans, beaches, plains, forests, snow, and mountains only validate when the altitude band matches the thresholds in `TerrainRuleConfiguration`, and mountains also require a mountain-specific noise spike (`MountainNoiseThreshold`).
- `GenericTileType` exists solely as a placeholder for any sprite index beyond the named biome IDs; it always returns `false`, so Wave Function Collapse never places it unless you subclass the type with your own logic.
- `NullTileType` (tile ID 0) remains registered as a sentinel but is filtered out of the initial WFC possibilities list, so void tiles no longer appear in the generated world.

## Scenario recipes
1. **Large continents (non-ocean landmasses)**
   - Increase `MountainRangeMax`/`MountainWidthMax` (e.g., 512, 200) so mountain seeds do not splice continents apart, and raise `BeachOceanSizeMin`/`BeachOceanSizeMax` (e.g., 400–2000) so beaches only spawn when oceans become very large.
   - Result: plains/forest chunks can grow until they reach ocean edges, which must itself become huge before a beach is allowed, keeping more long, uninterrupted land.

2. **Large ocean groups**
   - Drop `MountainRangeMin` (e.g., 5) and `MountainRangeMax` (e.g., 128) so mountains stay clustered, while letting `BeachOceanSizeMin`/`BeachOceanSizeMax` cover a wide band (e.g., 500–2500).
   - Oceans now overwhelm most of the map; beaches still appear but only on coastlines whose ocean neighbors already tick the large-group thresholds.

3. **Large plains groups**
   - While plains do not use configuration values directly, you can indirectly encourage large plains by lowering `BeachPlainsSizeMin` to very high values (e.g., 400) so beaches only form when plains span hundreds of tiles.
   - Combine this with fewer ocean/beach seeds (via `MasterSeed`) so WFC runs out of valid beach placements before breaking plains apart.

4. **Narrow beach/coast groups between plains and oceans**
   - Set both `BeachOceanSizeMax` and `BeachPlainsSizeMax` to small numbers (e.g., 8–12) so the rules only allow beaches when the neighboring ocean or plain is tiny; once that neighbor grows beyond the cap, the beach rule fails and the coast stops expanding.
   - Keep the `Beach` tile IDs near `TerrainTileIds.Ocean`/`Plains` in the WFC order so that the beach tries to satisfy both neighbors simultaneously.

5. **Medium-sized, round forests**
   - Extend or shadow `ForestTileType` with your own `EvaluateRules` that inspects `context.GetCandidateGroupMetrics()` and caps `MaxDimension` to around 16 while still permitting neighbors of type `Plains`/`Snow`/`Mountain`.
   - Alternatively, register a new `GenericTileType` subclass before building the registry and use `TileRuleContext` to reject candidate expansions whose group metrics exceed your desired circular radius, keeping forests from sprawling into long corridors.

Adjust these recipes incrementally and iterate with `dotnet run --project TerrainGeneration2D/TerrainGeneration2D.csproj`; delete `Content/saves` between experiments so the new rules are exercised through chunk regeneration rather than cached saves.
