# 09 — WFC Rule & Configuration Examples

This page provides concrete, runnable-ish configuration snippets and rule examples that map to the runtime code paths (TileTypeRegistry, TerrainRules, WfcWeights, Heuristics). These examples are designed to be adapted into the project's appsettings.json and provide practical starting points.

## Example tile rules (TerrainRules)

The following JSON illustrates a small set of tile rules that specify elevation thresholds, noise thresholds, and soft placement guidance. The runtime code expects rules accessible by tile id or name; adapt keys to match the project's TileTypeRegistry mapping.

```json
"TerrainRules": {
  "Ocean": { "ElevationMax": 0.18 },
  "Beach": { "ElevationMin": 0.18, "ElevationMax": 0.26, "DetailChance": 0.15 },
  "Plains": { "ElevationMin": 0.26, "ElevationMax": 0.6 },
  "Forest": { "ElevationMin": 0.28, "ElevationMax": 0.6, "DetailNoiseThreshold": 0.55 },
  "Mountain": { "ElevationMin": 0.6, "MountainNoiseThreshold": 0.6 },
  "Snow": { "ElevationMin": 0.85 }
}
```

Notes:

- Elevation thresholds are normalized (0..1) relative to the HeightMap generator output.
- DetailChance and noise thresholds are used by fallback generation to add variation; WFC may use these values as biases.

## Example adjacency rules

Adjacency rules specify which tiles are allowed next to each other. The following example uses simple named rules; the real runtime uses TileTypeRegistry adjacency tables.

```json
"AdjacencyRules": {
  "Ocean": { "N": ["Ocean", "Beach"], "E": ["Ocean", "Beach"], "S": ["Ocean", "Beach"], "W": ["Ocean", "Beach"] },
  "Beach": { "N": ["Plains", "Forest", "Beach"], "E": ["Beach", "Ocean", "Plains"], "S": ["Ocean", "Beach"], "W": ["Plains", "Beach"] },
  "Plains": { "N": ["Plains", "Forest", "Beach"], "E": ["Plains", "Forest"], "S": ["Plains", "Beach"], "W": ["Plains", "Forest"] },
  "Forest": { "N": ["Forest", "Plains"], "E": ["Forest", "Plains"], "S": ["Plains", "Forest"], "W": ["Forest", "Plains"] },
  "Mountain": { "N": ["Mountain", "Snow", "Plains"], "E": ["Mountain", "Plains"], "S": ["Mountain", "Plains"], "W": ["Mountain", "Plains"] },
  "Snow": { "N": ["Snow", "Mountain"], "E": ["Snow", "Mountain"], "S": ["Mountain", "Snow"], "W": ["Snow", "Mountain"] }
}
```

These rules are intentionally permissive to reduce contradictions during generation; stricter rules increase the chance of contradictions and rely on backtracking/time budgets.

## Example WFC weight matrix (WfcWeights)

Weights influence candidate selection when collapsing and tie-breaking when selecting the next cell.

```json
"WfcWeights": {
  "tileWeights": {
    "Ocean": 1.0,
    "Beach": 0.8,
    "Plains": 1.0,
    "Forest": 0.9,
    "Mountain": 0.6,
    "Snow": 0.4
  },
  "heuristicWeights": {
    "candidateCountWeight": 1.0,
    "heightInfluenceWeight": 0.5,
    "distanceBiasWeight": 0.1
  }
}
```

Adjust tileWeights to bias WFC toward or away from specific tiles.

## Example heuristics configuration (Heuristics)

```json
"Heuristics": {
  "preferLowEntropy": true,
  "randomTieBreak": true,
  "heightmapInfluence": true,
  "distanceBiasFactor": 0.1
}
```

## Full example section to paste into appsettings.json

```json
"Mapping": {
  "TerrainRules": {
    "Ocean": { "ElevationMax": 0.18 },
    "Beach": { "ElevationMin": 0.18, "ElevationMax": 0.26, "DetailChance": 0.15 },
    "Plains": { "ElevationMin": 0.26, "ElevationMax": 0.6 },
    "Forest": { "ElevationMin": 0.28, "ElevationMax": 0.6, "DetailNoiseThreshold": 0.55 },
    "Mountain": { "ElevationMin": 0.6, "MountainNoiseThreshold": 0.6 },
    "Snow": { "ElevationMin": 0.85 }
  },
  "AdjacencyRules": {
    "Ocean": { "N": ["Ocean", "Beach"], "E": ["Ocean", "Beach"], "S": ["Ocean", "Beach"], "W": ["Ocean", "Beach"] },
    "Beach": { "N": ["Plains", "Forest", "Beach"], "E": ["Beach", "Ocean", "Plains"], "S": ["Ocean", "Beach"], "W": ["Plains", "Beach"] },
    "Plains": { "N": ["Plains", "Forest", "Beach"], "E": ["Plains", "Forest"], "S": ["Plains", "Beach"], "W": ["Plains", "Forest"] },
    "Forest": { "N": ["Forest", "Plains"], "E": ["Forest", "Plains"], "S": ["Plains", "Forest"], "W": ["Forest", "Plains"] },
    "Mountain": { "N": ["Mountain", "Snow", "Plains"], "E": ["Mountain", "Plains"], "S": ["Mountain", "Plains"], "W": ["Mountain", "Plains"] },
    "Snow": { "N": ["Snow", "Mountain"], "E": ["Snow", "Mountain"], "S": ["Mountain", "Snow"], "W": ["Snow", "Mountain"] }
  },
  "WfcWeights": {
    "tileWeights": { "Ocean": 1.0, "Beach": 0.8, "Plains": 1.0, "Forest": 0.9, "Mountain": 0.6, "Snow": 0.4 },
    "heuristicWeights": { "candidateCountWeight": 1.0, "heightInfluenceWeight": 0.5, "distanceBiasWeight": 0.1 }
  },
  "Heuristics": { "preferLowEntropy": true, "randomTieBreak": true, "heightmapInfluence": true, "distanceBiasFactor": 0.1 },
  "WfcRuntime": { "WfcTimeBudgetMs": 50, "EnableBacktracking": true, "MaxBacktrackSteps": 2048 }
}
```

## Regression guidance

- When adding stricter adjacency constraints, increase WfcTimeBudgetMs or enable more backtracking to reduce fallback frequency.
- Use RegenerateChunksInView and/or ClearAllSavedChunks when making rule changes to ensure saved chunks match updated rules.

## Troubleshooting

- Symptom: many chunks generated using fallback (random) generator. Likely causes:
  - Time budget too low; increase WfcTimeBudgetMs.
  - Constraints too tight; relax adjacency rules or add more permissive fallback allowances.
  - Weights bias selection toward rare tiles producing contradictions; rebalance tileWeights.

- Symptom: visually jarring seams at chunk borders. Likely causes:
  - Deterministic seed differences or inconsistent rule application across chunks; ensure TileTypeRegistry and HeightMap config are consistent across runs.
  - Consider increasing the active buffer or coordinating border constraints across adjacent chunk generation (future improvement).

## Where to implement

- These JSON snippets are intended for appsettings.json; actual runtime config keys may differ slightly—use them as a starting point and adapt to the project's configuration shape.
