# Terrain Rules

Purpose

- Define per-tile placement constraints and soft heuristics used by both WFC and the fallback generator.
- Typical fields include elevation thresholds, noise thresholds, and per-tile weights.

Common fields

- ElevationMin / ElevationMax (float, 0..1): constrain tiles to certain altitude ranges from the HeightMap.
- DetailNoiseThreshold / MountainNoiseThreshold (float 0..1): used to gate noisy features like mountains or beach detail.
- DetailChance (float 0..1): probabilistic placement used by fallback generator for variety.
- Adjacency rules are typically stored separately but referenced by tile name/ID.

How ChunkedTilemap uses rules

- In GenerateRandomChunk the code uses config.GetRuleForType(tileId) to inspect ElevationMin/Max and noise thresholds when picking a fallback tile by height.
- Rules must be permissive enough to avoid constant contradictions when used as soft guidance by WFC; stricter rules increase backtracking/failure likelihood.

Example rule (JSON)

```json
"Plains": { "ElevationMin": 0.26, "ElevationMax": 0.6 },
"Forest": { "ElevationMin": 0.28, "ElevationMax": 0.6, "DetailNoiseThreshold": 0.55 }
```

Design guidance

- Keep elevation bands overlapping slightly (e.g., Beach end and Plains start) to provide transition zones and reduce contradictions.
- Use DetailChance/noise thresholds for visual variety rather than hard placement where possible.
- Document any tile IDs and names so that TileTypeRegistry and appsettings mapping stay consistent.

Validation & testing

- Manual: change a rule, open F10, Regenerate visible chunks and use F12 to verify visual transitions are smooth.
- Automated: unit test that config parsing yields expected rule objects and that GenerateRandomChunk respects threshold boundaries.

References

- See TileTypeRegistry (tile-type-registry.md) for registry interactions.
- See map-generation/wfc/09-rule-examples.md for a full example mapping section to paste into appsettings.json.
