# Tile Configuration Guide

## How rules work

- Each terrain or resource type is configured via a `GroupRuleConfiguration` (see `TerrainGeneration2D.Core/Mapping/TileTypes/TileTypeRuleConfiguration.cs`).
- The configuration for all types is a collection of these, held in `TileTypeRuleConfiguration` (or `ResourceTypeRuleConfiguration`).
- Each `GroupRuleConfiguration` includes:
  - `Id`: the terrain or resource type id
  - `MinGroupSizeX`, `MinGroupSizeY`: minimum contiguous group size in X and Y
  - `MaxGroupSizeX`, `MaxGroupSizeY`: maximum contiguous group size in X and Y
  - `ElevationMin`, `ElevationMax`: allowed tile elevation range
  - `NoiseProvider`: (optional) name of the noise function to use
  - `NoiseThreshold`: (optional) threshold for noise-based placement
- Every tile’s `EvaluateRules(TileRuleContext)` can access `TileRuleContext.GetCandidateGroupMetrics` or `GetNeighborGroupMetrics` (which call `TerrainGeneration2D.Core/Mapping/MappingInformationService.cs`) to inspect contiguous regions before accepting a candidate.

## Applying a configuration

- Pass a customized `TileTypeRuleConfiguration` (or `ResourceTypeRuleConfiguration`) when you construct `TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs` in `GameScene`:

```csharp
var config = new TileTypeRuleConfiguration
{
    Rules = new List<GroupRuleConfiguration>
    {
        new GroupRuleConfiguration { Id = TerrainTileIds.Mountain, MinGroupSizeX = 8, MaxGroupSizeX = 48, ElevationMin = 0.76f, NoiseProvider = "mountain", NoiseThreshold = 0.55f },
        new GroupRuleConfiguration { Id = TerrainTileIds.Beach, MinGroupSizeX = 12, MaxGroupSizeX = 180, ElevationMin = 0.33f, ElevationMax = 0.48f },
        // ...other types...
    }
};
_chunkedTilemap = new ChunkedTilemap(tileset, MapSizeInTiles, MasterSeed, saveDir, terrainRuleConfiguration: config);
```

Every change to a rule immediately alters the Wave Function Collapse constraints that produce your map.

## Heightmap configuration

- FastNoiseLite drives the heightmap implementation in `TerrainGeneration2D.Core/Mapping/HeightMap/HeightMapGenerator.cs`. Pass a `HeightMapConfiguration` alongside your rule configuration when constructing `ChunkedTilemap` to tune continent scale, mountain frequency, and contribution weights:

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

- Each tile now receives a `HeightSample` (altitude + noise derivatives). Rule checks include `TileRuleContext.CandidateHeight.Altitude` so oceans, beaches, plains, forests, snow, and mountains only validate when the altitude band matches the thresholds in the relevant `GroupRuleConfiguration`, and mountains also require a mountain-specific noise spike if configured.
- `GenericTileType` exists solely as a placeholder for any sprite index beyond the named biome IDs; it always returns `false`, so Wave Function Collapse never places it unless you subclass the type with your own logic.
- `NullTileType` (tile ID 0) remains registered as a sentinel but is filtered out of the initial WFC possibilities list, so void tiles no longer appear in the generated world.

## Scenario recipes

1. **Large continents (non-ocean landmasses)**
   - Increase `MaxGroupSizeX`/`MaxGroupSizeY` for mountains so mountain seeds do not splice continents apart, and raise beach group sizes so beaches only spawn when oceans become very large.
   - Result: plains/forest chunks can grow until they reach ocean edges, which must itself become huge before a beach is allowed, keeping more long, uninterrupted land.

2. **Large ocean groups**
   - Drop mountain group minimums and maximums so mountains stay clustered, while letting beach group sizes cover a wide band.
   - Oceans now overwhelm most of the map; beaches still appear but only on coastlines whose ocean neighbors already tick the large-group thresholds.

3. **Large plains groups**
   - Encourage large plains by setting beach group minimums very high so beaches only form when plains span hundreds of tiles.
   - Combine this with fewer ocean/beach seeds (via `MasterSeed`) so WFC runs out of valid beach placements before breaking plains apart.

4. **Narrow beach/coast groups between plains and oceans**
   - Set both beach group maximums to small numbers so the rules only allow beaches when the neighboring ocean or plain is tiny; once that neighbor grows beyond the cap, the beach rule fails and the coast stops expanding.
   - Keep the `Beach` tile IDs near `TerrainTileIds.Ocean`/`Plains` in the WFC order so that the beach tries to satisfy both neighbors simultaneously.

5. **Medium-sized, round forests**
   - Extend or shadow `ForestTileType` with your own `EvaluateRules` that inspects `context.GetCandidateGroupMetrics()` and caps `MaxGroupSizeX`/`Y` to around 16 while still permitting neighbors of type `Plains`/`Snow`/`Mountain`.
   - Alternatively, register a new `GenericTileType` subclass before building the registry and use `TileRuleContext` to reject candidate expansions whose group metrics exceed your desired circular radius, keeping forests from sprawling into long corridors.

Adjust these recipes incrementally and iterate with `dotnet run --project TerrainGeneration2D/TerrainGeneration2D.csproj`; delete `Content/saves` between experiments so the new rules are exercised through chunk regeneration rather than cached saves.

## Runtime configuration

- You can tweak `TileTypeRuleConfiguration`, `HeightMapConfiguration`, and WFC heuristic weights at runtime via appsettings. See the tutorial: [12-config-wfc-weights.md](./terrain2d-tutorial/12-config-wfc-weights.md).
