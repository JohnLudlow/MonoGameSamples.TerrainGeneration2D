# Mapping API Reference

## Table of contents

- [ChunkedTilemap](#chunkedtilemap)
- [WfcProvider (core WFC implementation)](#wfcprovider-core-wfc-implementation)
- [TileTypeRegistry](#tiletyperegistry)
- [HeightMapGenerator / IHeightProvider](#heightmapgenerator--iheightprovider)

## Definition of terms

| Term | Meaning |
| ---- | ------- |
| Domain | set of candidate tiles for a cell in WFC |
| Entropy | measure of uncertainty used to select next cell |

This page documents the key runtime types used by the mapping subsystem with the minimal information developers need to use them.

## ChunkedTilemap

- Purpose: Manages the world as 64×64 chunks, handles generation, save/load, and drawing.
- Important constructors / props:
  - ChunkedTilemap(Tileset tileset, int mapSizeInTiles, int masterSeed, string saveDirectory, bool useWaveFunctionCollapse = true, ...) — create with tileset, world size, seed and save directory.
  - int WfcTimeBudgetMs { get; set; } — per-chunk WFC time budget (ms), defaults to 50.
  - void UpdateActiveChunks(Rectangle viewportWorldBounds) — loads/generates chunks around the viewport and unloads distant chunks.
  - void RegenerateChunksInView(Rectangle viewportWorldBounds, bool overwriteSaves = true) — regenerates chunks in view; when overwriteSaves is true saved files are overwritten.
  - void ClearAllSavedChunks() — deletes saved chunk files matching `map_*_*.dat` in the save directory.
  - void SaveAll() — saves all dirty chunks currently active.
  - int GetTile(int tileX, int tileY) / void SetTile(int tileX, int tileY, int tileId)

## WfcProvider (core WFC implementation)

- Purpose: Performs constraint-based generation for a grid with adjacency rules, propagation and optional backtracking.
- Typical usage pattern:
  1) Construct with width/height, tile registry, random provider, rule config, height provider, chunk origin, weight/heuristics configs.
  2) Call Generate(enableBacktracking: bool, maxIterations: int, maxBacktrackSteps: int, maxDepth: int, timeBudget: TimeSpan).
  3) On success call GetOutput() to obtain the collapsed tile IDs.
- Notes: Backtracking improves robustness at the cost of time/complexity; callers typically bound work with a TimeSpan timeBudget.

## TileTypeRegistry

- Purpose: Registry of available tile types and adjacency/placement rules used by WFC and fallback generation.
- Typical interactions: TileTypeRegistry.CreateDefault(tileCount, config) to build from configuration; provider code queries rules to evaluate neighbor constraints.

## HeightMapGenerator / IHeightProvider

- Purpose: Provides deterministic elevation/biome samples for a world coordinate given a master seed and heightmap configuration.
- API: GetSample(int worldX, int worldY) → HeightSample (Altitude, DetailNoise, MountainNoise, etc.).

## Notes & guidance

- Prefer using the provided public methods on ChunkedTilemap rather than re-implementing chunk load/save logic.
- When tuning WFC parameters, change WfcTimeBudgetMs conservatively and use RegenerateChunksInView to preview changes.
- All saved chunks use a gzipped binary format with filename pattern `map_{cx}_{cy}.dat` stored in the configured save directory (see appsettings docs).
