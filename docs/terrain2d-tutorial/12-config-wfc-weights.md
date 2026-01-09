# Runtime Config: WFC & Map Rules

This page shows how to tweak the Wave Function Collapse (WFC) selection heuristics and map generation rules at runtime, without changing code.

## Where to Configure

Edit the app settings file: [TerrainGeneration2D/appsettings.json](../../TerrainGeneration2D/appsettings.json). The engine reads these values at scene initialization.

```json
{
  "WfcWeights": {
    "Base": 1,
    "NeighborMatchBoost": 3
  },
  "TerrainRules": {
    "MountainRangeMin": 8,
    "MountainRangeMax": 48,
    "MountainWidthMax": 12,
    "MountainWidthMin": 3,
    "BeachOceanSizeMin": 12,
    "BeachOceanSizeMax": 180,
    "BeachPlainsSizeMin": 20,
    "BeachPlainsSizeMax": 400,
    "OceanHeightMax": 0.34,
    "BeachHeightMin": 0.33,
    "BeachHeightMax": 0.48,
    "PlainsHeightMin": 0.35,
    "PlainsHeightMax": 0.78,
    "ForestHeightMin": 0.42,
    "ForestHeightMax": 0.88,
    "SnowHeightMin": 0.82,
    "MountainHeightMin": 0.76,
    "MountainNoiseThreshold": 0.55
  },
  "HeightMap": {
    "ContinentScale": 0.0045,
    "MountainScale": 0.02,
    "DetailScale": 0.1,
    "ContinentWeight": 0.75,
    "MountainWeight": 0.35,
    "DetailWeight": 0.25
  }
}
```

## What the Values Do

- Base: Baseline multiplier applied to all candidate tile weights.
- NeighborMatchBoost: Extra multiplier applied to candidates that match adjacent tiles according to the rule set. Higher values bias selection toward local consistency.

These weights feed into `WfcProvider` during tile selection. They influence both non-backtracking weighted rolls and candidate ordering when backtracking is enabled.

### TerrainRules

- Beach/Mountain ranges and widths: Control contiguous band sizes and mountain stripe thickness.
- Height thresholds (Ocean/Beach/Plains/Forest/Snow/Mountain): Gate tiles by altitude bands.
- MountainNoiseThreshold: Requires a local noise spike to permit mountain tiles.

These values are consumed by tile types via `TileTypeRegistry` rule checks.

### HeightMap

- Scales (Continent/Mountain/Detail): Frequency of noise layers.
- Weights (Continent/Mountain/Detail): Contribution of each layer to final altitude.

These values are passed to `HeightMapGenerator` to shape the overall terrain.

## Applying Changes

- The config is read when `GameScene` constructs the tilemap. Restart the game to apply changes.
- To observe a full regeneration, delete the saves folder at TerrainGeneration2D.Core/Content/saves. Chunks will be re-generated with the new weights.

## Try It

- Make selection neutral to neighbors: set `NeighborMatchBoost` to `0` and restart.
- Prefer strong local coherence: set `NeighborMatchBoost` to `5` or `6`.
- Toggle the debug overlay (F12) and pan around to inspect chunk borders and viewport bounds while experimenting.

- Widen beaches: increase `BeachPlainsSizeMax` and `BeachOceanSizeMax`.
- Raise oceans: decrease `OceanHeightMax`.
- Make mountains rarer: increase `MountainNoiseThreshold` or reduce `MountainWeight`.

## Notes

- Backtracking: If enabled, the weights affect candidate ordering but not the rollback mechanics.
- Determinism in tests: Unit tests may use deterministic random providers; weight changes won’t necessarily alter their outputs.
- Diagnostics: See [TerrainGeneration2D.Core/Diagnostics/README.md](../../TerrainGeneration2D.Core/Diagnostics/README.md) for performance events you can use to measure effects.
