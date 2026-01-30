# appsettings.json — Mapping Configuration

## Table of contents

- [Common sections](#common-sections)
- [Practical guidance](#practical-guidance)
- [Where to edit](#where-to-edit)

## Definition of terms

| Term | Meaning |
| ---- | ------- |
| WfcWeights | Per-tile and heuristic weights used by WFC |
| WfcRuntime | Runtime budgets and options for WFC (time budgets, backtracking) |

This document describes the mapping-related configuration keys used by the runtime. These keys appear in TerrainGeneration2D/appsettings.json and control generation heuristics and runtime budgets.

## Common sections

- WfcWeights
  - Purpose: Per-tile and per-heuristic weights used by WFC when selecting between candidates.
  - Example:

```json
"WfcWeights": {
  "entropyWeight": 1.0,
  "mountainBias": 0.5
}
```

- TerrainRules (Tile type rules)
  - Purpose: Per-tile elevation/noise thresholds and placement groupings used by fallback generation and WFC constraints.
  - Example (simplified):

```json
"TerrainRules": {
  "Ocean": { "ElevationMax": 0.2 },
  "Beach": { "ElevationMin": 0.2, "ElevationMax": 0.28 },
  "Plains": { "ElevationMin": 0.28 }
}
```

- HeightMap
  - Purpose: Parameters for the height/noise generator (frequency, octaves, amplitude, seeds).
  - Example:

```json
"HeightMap": {
  "frequency": 0.005,
  "octaves": 4,
  "lacunarity": 2.0,
  "persistence": 0.5
}
```

- Heuristics
  - Purpose: Tie-breaking and cell selection heuristics used by WFC runtime.
  - Example:

```json
"Heuristics": {
  "preferLowEntropy": true,
  "randomTieBreak": true
}
```

- WfcRuntime
  - Purpose: Runtime budgets and limits for WFC operations executed per chunk.
  - Example:

```json
"WfcRuntime": {
  "WfcTimeBudgetMs": 50,
  "EnableBacktracking": true,
  "MaxBacktrackSteps": 2048
}
```

## Practical guidance

- WfcTimeBudgetMs defaults to 50 ms; lowering it reduces per-chunk CPU but may increase failures and fallback generation frequency.
- EnableBacktracking improves success rates but can increase time per-chunk.
- When changing rules or weights, either delete saves (ClearAllSavedChunks) or use RegenerateChunksInView with overwriteSaves=true to preview changes immediately.

## Where to edit

- appsettings.json in the project root (TerrainGeneration2D/appsettings.json) contains defaults. Update that file and check the runtime F10 panel to tune values live.
